// Online Classroom — Mesh WebRTC + SignalR signaling
// QEYD: Production üçün böyük ölçəkdə SFU media server (LiveKit, mediasoup) lazımdır.
// Bu demo 1 müəllim + az sayda tələbə üçün nəzərdə tutulub.

(function () {
    'use strict';

    const root = document.getElementById('classroomRoot');
    if (!root) return;

    const lessonId = parseInt(root.dataset.lessonId, 10);
    const isTeacher = root.dataset.isTeacher === 'true';
    const apiBase = root.dataset.apiBase;
    const token = root.dataset.token;
    const myUserId = root.dataset.userId;
    const myUserName = root.dataset.userName;

    const hubUrl = apiBase + '/hubs/online-class';
    const iceConfig = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };

    // ── State ──
    let localStream = null;
    let conn = null;
    const peers = {};   // connectionId -> { pc, userId, userName, tile }

    // ── Screen share state ──
    let screenStream = null;           // mənim paylaşdığım ekran stream-i
    let cameraVideoTrack = null;       // orijinal kamera video track (bərpa üçün)
    let isSharing = false;             // mən paylaşıram?
    let remoteSharerConnId = null;     // başqası paylaşırsa onun connectionId-i
    let myConnectionId = null;         // öz hub connection id-im

    // ── Browser support ──
    if (!window.RTCPeerConnection || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        (window.acadexAlert || function(o){ alert((o&&o.message)||o); })({ title: 'Dəstəklənmir', message: 'Brauzeriniz WebRTC dəstəkləmir.', type: 'warning' });
        return;
    }

    const $ = id => document.getElementById(id);
    const grid = $('videoGrid');
    const partList = $('participantList');
    const partCount = $('partCount');
    const chatMsgs = $('chatMsgs');

    function setPartCount() { partCount.textContent = Object.keys(peers).length + 1; }

    // ── Side panel tabs ──
    document.querySelectorAll('.side-tabs button').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.side-tabs button').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const tab = btn.dataset.tab;
            $('sideParticipants').style.display = tab === 'participants' ? '' : 'none';
            $('sideChat').style.display = tab === 'chat' ? 'flex' : 'none';
        });
    });

    // ── Tile management ──
    function addRemoteTile(connectionId, userName) {
        if ($('tile-' + connectionId)) return;
        const tile = document.createElement('div');
        tile.className = 'video-tile';
        tile.id = 'tile-' + connectionId;
        const v = document.createElement('video');
        v.autoplay = true; v.playsInline = true;
        const name = document.createElement('div');
        name.className = 'name'; name.textContent = userName || 'İştirakçı';
        const shareTag = document.createElement('div');
        shareTag.className = 'share-tag';
        shareTag.textContent = 'Ekran';
        const expandBtn = document.createElement('button');
        expandBtn.className = 'tile-expand';
        expandBtn.type = 'button';
        expandBtn.title = 'Böyüt';
        expandBtn.innerHTML = '<i class="fas fa-expand"></i>';
        expandBtn.addEventListener('click', () => expandStream(v, name.textContent));
        tile.appendChild(v); tile.appendChild(name); tile.appendChild(shareTag); tile.appendChild(expandBtn);
        grid.appendChild(tile);
        return v;
    }

    function removeRemoteTile(connectionId) {
        const t = $('tile-' + connectionId);
        if (t) t.remove();
    }

    function rebuildParticipantList() {
        partList.innerHTML = '';
        // Öz sətrim
        const meRow = document.createElement('div');
        meRow.className = 'participant-row';
        meRow.innerHTML =
            '<div class="participant-dot"></div>' +
            '<span class="participant-name"></span>' +
            '<span class="participant-role ' + (isTeacher ? 'teacher' : 'student') + '">' + (isTeacher ? 'Müəllim' : 'Tələbə') + '</span>' +
            '<span class="participant-mic" id="meMic"><i class="fas fa-microphone"></i></span>';
        meRow.querySelector('.participant-name').textContent = (myUserName || 'Siz') + ' (siz)';
        partList.appendChild(meRow);

        Object.values(peers).forEach(p => {
            const row = document.createElement('div');
            row.className = 'participant-row';
            const isTeacherRole = (p.userRole === 'Teacher' || p.userRole === 'SuperAdmin');
            const roleLabel = isTeacherRole ? 'Müəllim' : 'Tələbə';
            const roleClass = isTeacherRole ? 'teacher' : 'student';

            const dot = document.createElement('div');
            dot.className = 'participant-dot' + (p.muted ? ' muted' : '');

            const nameSpan = document.createElement('span');
            nameSpan.className = 'participant-name';
            nameSpan.textContent = p.userName || 'Naməlum iştirakçı';

            const roleSpan = document.createElement('span');
            roleSpan.className = 'participant-role ' + roleClass;
            roleSpan.textContent = roleLabel;

            const micSpan = document.createElement('span');
            micSpan.className = 'participant-mic' + (p.muted ? ' muted' : '');
            micSpan.innerHTML = p.muted ? '<i class="fas fa-microphone-slash"></i>' : '<i class="fas fa-microphone"></i>';

            row.appendChild(dot);
            row.appendChild(nameSpan);
            row.appendChild(roleSpan);
            row.appendChild(micSpan);

            // Müəllim tələbə sətrində mute düyməsi görür
            if (isTeacher && !isTeacherRole && p.userId) {
                const muteBtn = document.createElement('button');
                muteBtn.className = 'p-mute-btn';
                muteBtn.type = 'button';
                muteBtn.textContent = 'Sustur';
                muteBtn.addEventListener('click', () => muteParticipant(p.userId, p.userName));
                row.appendChild(muteBtn);
            }

            partList.appendChild(row);
        });
        setPartCount();
    }

    function escapeHtml(s) {
        const d = document.createElement('div');
        d.textContent = s || '';
        return d.innerHTML;
    }

    // ── Peer connection helper ──
    function createPeer(connectionId, userId, userName, userRole) {
        const pc = new RTCPeerConnection(iceConfig);

        // Local stream tracks-i əlavə et
        if (localStream) {
            localStream.getTracks().forEach(t => pc.addTrack(t, localStream));
        }

        pc.onicecandidate = ev => {
            if (ev.candidate && conn) {
                conn.invoke('SendIceCandidate', lessonId, connectionId, ev.candidate).catch(() => {});
            }
        };

        pc.ontrack = ev => {
            const v = $('tile-' + connectionId)?.querySelector('video') || addRemoteTile(connectionId, userName);
            if (v && v.srcObject !== ev.streams[0]) {
                v.srcObject = ev.streams[0];
            }
            // Əgər bu peer hazırda ekran paylaşırsa, stage-i onun stream-i ilə yenilə
            if (remoteSharerConnId === connectionId) {
                const stageVideo = $('screenStageVideo');
                if (stageVideo && ev.streams[0]) stageVideo.srcObject = ev.streams[0];
            }
        };

        pc.onconnectionstatechange = () => {
            if (pc.connectionState === 'failed' || pc.connectionState === 'closed') {
                removePeer(connectionId);
            }
        };

        peers[connectionId] = { pc, userId, userName: userName || 'Naməlum iştirakçı', userRole: userRole || '', muted: false };
        addRemoteTile(connectionId, userName || 'Naməlum iştirakçı');
        rebuildParticipantList();
        return pc;
    }

    // Peer metadata yenilə (ad/rol sonradan offer/answer ilə gələ bilər)
    function updatePeerInfo(connectionId, userName, userRole) {
        const p = peers[connectionId];
        if (!p) return;
        let changed = false;
        if (userName && p.userName !== userName) { p.userName = userName; changed = true; }
        if (userRole && p.userRole !== userRole) { p.userRole = userRole; changed = true; }
        if (changed) {
            const tile = $('tile-' + connectionId);
            const nameEl = tile?.querySelector('.name');
            if (nameEl) nameEl.textContent = userName;
            rebuildParticipantList();
        }
    }

    function removePeer(connectionId) {
        const p = peers[connectionId];
        if (!p) return;
        try { p.pc.close(); } catch {}
        delete peers[connectionId];
        removeRemoteTile(connectionId);
        rebuildParticipantList();
    }

    // ── Hub event handlers ──
    async function onUserJoined(payload) {
        // Mövcud participant kimi mən offer göndərirəm yeni gələnə
        const pc = createPeer(payload.connectionId, payload.userId, payload.userFullName, payload.userRole);
        try {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            await conn.invoke('SendOffer', lessonId, payload.connectionId, offer);
        } catch (e) { console.error('createOffer failed', e); }
    }

    async function onReceiveOffer(payload) {
        let p = peers[payload.fromConnectionId];
        const pc = p ? p.pc : createPeer(payload.fromConnectionId, payload.fromUserId, payload.fromUserFullName, payload.fromUserRole);
        updatePeerInfo(payload.fromConnectionId, payload.fromUserFullName, payload.fromUserRole);
        try {
            await pc.setRemoteDescription(new RTCSessionDescription(payload.offer));
            const answer = await pc.createAnswer();
            await pc.setLocalDescription(answer);
            await conn.invoke('SendAnswer', lessonId, payload.fromConnectionId, answer);
        } catch (e) { console.error('handle offer failed', e); }
    }

    async function onReceiveAnswer(payload) {
        const p = peers[payload.fromConnectionId];
        if (!p) return;
        updatePeerInfo(payload.fromConnectionId, payload.fromUserFullName, payload.fromUserRole);
        try {
            await p.pc.setRemoteDescription(new RTCSessionDescription(payload.answer));
        } catch (e) { console.error('handle answer failed', e); }
    }

    async function onReceiveIce(payload) {
        const p = peers[payload.fromConnectionId];
        if (!p) return;
        try { await p.pc.addIceCandidate(new RTCIceCandidate(payload.candidate)); }
        catch (e) { console.error('addIceCandidate failed', e); }
    }

    // ── Chat ──
    function appendChatMessage(msg) {
        // XSS qoruması: textContent ilə yazırıq, innerHTML deyil
        const wrap = document.createElement('div');
        wrap.className = 'chat-msg';
        const meta = document.createElement('div');
        meta.className = 'meta';
        const nameSpan = document.createElement('span');
        nameSpan.textContent = msg.senderName || 'User';
        const timeSpan = document.createElement('span');
        const t = new Date(msg.sentAt);
        timeSpan.textContent = isNaN(t) ? '' : t.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        meta.appendChild(nameSpan); meta.appendChild(timeSpan);
        const body = document.createElement('div');
        body.textContent = msg.messageText || '';
        wrap.appendChild(meta); wrap.appendChild(body);
        chatMsgs.appendChild(wrap);
        chatMsgs.scrollTop = chatMsgs.scrollHeight;
    }

    async function loadChatHistory() {
        try {
            const res = await fetch(apiBase + '/api/onlinelesson/' + lessonId + '/chat', {
                headers: { 'Authorization': 'Bearer ' + token }
            });
            if (!res.ok) return;
            const list = await res.json();
            list.forEach(appendChatMessage);
        } catch {}
    }

    function bindChatUI() {
        const sendBtn = $('chatSend'), inp = $('chatInput');
        const send = async () => {
            const text = inp.value.trim();
            if (!text || !conn) return;
            try {
                await conn.invoke('SendClassMessage', lessonId, text);
                inp.value = '';
            } catch (e) { (window.acadexAlert || function(o){ alert((o&&o.message)||o); })({ title: 'Xəta', message: 'Mesaj göndərilmədi', type: 'warning' }); }
        };
        sendBtn.addEventListener('click', send);
        inp.addEventListener('keypress', e => { if (e.key === 'Enter') { e.preventDefault(); send(); } });
    }

    // ── Screen share ──

    // Bütün peer-lərdə video sender-i verilmiş track ilə əvəz et
    function replaceVideoTrackForAllPeers(newTrack) {
        Object.values(peers).forEach(p => {
            try {
                const sender = p.pc.getSenders().find(s => s.track && s.track.kind === 'video');
                if (sender) sender.replaceTrack(newTrack);
            } catch (e) { console.error('replaceTrack failed', e); }
        });
    }

    async function startScreenShare() {
        // Browser dəstəyi
        if (!navigator.mediaDevices || !navigator.mediaDevices.getDisplayMedia) {
            return notify('Dəstəklənmir', 'Ekran paylaşımı bu brauzerdə dəstəklənmir.', 'warning');
        }
        // Classroom-a qoşulmuş olmalı
        if (!conn || conn.state !== 'Connected') {
            return notify('Bağlantı yoxdur', 'Ekran paylaşmadan əvvəl classroom-a qoşulmalısınız.', 'warning');
        }
        // Artıq biri (mən deyil) paylaşırsa
        if (remoteSharerConnId && remoteSharerConnId !== myConnectionId) {
            return notify('Mümkün deyil', 'Başqa iştirakçı artıq ekran paylaşır.', 'warning');
        }
        if (isSharing) return;

        let stream;
        try {
            stream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: true });
        } catch (err) {
            // İstifadəçi ləğv etdi və ya icazə verilmədi
            if (err && (err.name === 'NotAllowedError' || err.name === 'AbortError')) {
                return notify('Ləğv edildi', 'Ekran paylaşımı ləğv edildi.', 'info');
            }
            console.error('getDisplayMedia failed', err);
            return notify('Xəta', 'Ekran paylaşımı başladıla bilmədi.', 'danger');
        }

        screenStream = stream;
        const screenTrack = stream.getVideoTracks()[0];
        if (!screenTrack) {
            stream.getTracks().forEach(t => t.stop());
            screenStream = null;
            return notify('Xəta', 'Ekran video axını tapılmadı.', 'danger');
        }

        // Orijinal kamera track-i yadda saxla, sonra peer-lərdə screen track-ə keç
        cameraVideoTrack = localStream ? localStream.getVideoTracks()[0] || null : null;
        replaceVideoTrackForAllPeers(screenTrack);

        // Lokal preview-da öz ekranımı göstər
        const localVideo = $('localVideo');
        if (localVideo) localVideo.srcObject = stream;

        // Browser-in "Stop sharing" düyməsi
        screenTrack.onended = () => { stopScreenShare(); };

        isSharing = true;
        updateScreenButton(true);

        // Hub-a bildir (tək paylaşan qaydası serverdə yoxlanır)
        try { await conn.invoke('StartScreenShare', lessonId); }
        catch (e) { console.error('StartScreenShare invoke failed', e); }
    }

    async function stopScreenShare() {
        if (!isSharing) return;
        isSharing = false;

        // Kamera track-ə geri qayıt (varsa)
        if (cameraVideoTrack && cameraVideoTrack.readyState === 'live') {
            replaceVideoTrackForAllPeers(cameraVideoTrack);
        } else {
            // Kamera yoxdursa/bağlıdırsa null göndər (video off)
            replaceVideoTrackForAllPeers(null);
        }

        // Ekran stream-ini bağla
        if (screenStream) {
            screenStream.getTracks().forEach(t => { try { t.stop(); } catch {} });
            screenStream = null;
        }

        // Lokal preview-u kameraya qaytar
        const localVideo = $('localVideo');
        if (localVideo) localVideo.srcObject = localStream;

        updateScreenButton(false);

        try { if (conn) await conn.invoke('StopScreenShare', lessonId); }
        catch (e) { console.error('StopScreenShare invoke failed', e); }
    }

    function updateScreenButton(active) {
        const btn = $('btnScreen');
        if (!btn) return;
        btn.classList.toggle('sharing', active);
        btn.title = active ? 'Paylaşımı dayandır' : 'Ekranı paylaş';
        btn.innerHTML = active ? '<i class="fas fa-stop-circle"></i>' : '<i class="fas fa-desktop"></i>';
    }

    // Toolbar mic/cam düymələrini media vəziyyətinə uyğunlaşdır
    function syncToolbarMediaState(micOn, camOn) {
        const micBtn = $('btnMic'), camBtn = $('btnCam');
        if (micBtn) {
            micBtn.classList.toggle('off', !micOn);
            micBtn.innerHTML = micOn ? '<i class="fas fa-microphone"></i>' : '<i class="fas fa-microphone-slash"></i>';
        }
        if (camBtn) {
            camBtn.classList.toggle('off', !camOn);
            camBtn.innerHTML = camOn ? '<i class="fas fa-video"></i>' : '<i class="fas fa-video-slash"></i>';
        }
    }

    // Remote screen share status UI
    function showScreenStage(connectionId, sharerName, isLocal) {
        remoteSharerConnId = connectionId;
        const stage = $('screenStage');
        const stageVideo = $('screenStageVideo');
        const nameEl = $('screenSharerName');
        const area = document.querySelector('.video-area');

        if (nameEl) nameEl.textContent = (isLocal ? 'Siz' : (sharerName || 'İştirakçı'));

        // Hansı video stream-i stage-də göstərək
        if (isLocal) {
            if (stageVideo && screenStream) stageVideo.srcObject = screenStream;
        } else {
            // Paylaşanın tile video stream-ini stage-ə güzgülə
            const tileVideo = $('tile-' + connectionId)?.querySelector('video');
            if (stageVideo && tileVideo && tileVideo.srcObject) stageVideo.srcObject = tileVideo.srcObject;
            // Tile-a paylaşım etiketi
            const tile = $('tile-' + connectionId);
            if (tile) tile.classList.add('sharing');
        }

        if (stage) stage.style.display = 'block';
        if (area) area.classList.add('has-stage');
    }

    function hideScreenStage(connectionId) {
        // Yalnız uyğun paylaşan dayananda gizlət
        if (connectionId && remoteSharerConnId && connectionId !== remoteSharerConnId) return;
        remoteSharerConnId = null;
        const stage = $('screenStage');
        const stageVideo = $('screenStageVideo');
        const area = document.querySelector('.video-area');
        if (stageVideo) stageVideo.srcObject = null;
        if (stage) stage.style.display = 'none';
        if (area) area.classList.remove('has-stage');
        if (connectionId) {
            const tile = $('tile-' + connectionId);
            if (tile) tile.classList.remove('sharing');
        }
    }

    function notify(title, message, type) {
        (window.acadexAlert || function (o) { alert((o && o.message) || o); })({ title: title, message: message, type: type || 'info' });
    }

    // ── Toast ──
    let toastTimer = null;
    function showToast(text) {
        const t = $('clsToast'), txt = $('clsToastText');
        if (!t || !txt) return;
        txt.textContent = text;
        t.classList.add('show');
        if (toastTimer) clearTimeout(toastTimer);
        toastTimer = setTimeout(() => t.classList.remove('show'), 4000);
    }

    // ── Fullscreen / böyüt modalı ──
    function expandStream(videoEl, label) {
        const modal = $('fsModal'), modalVideo = $('fsModalVideo'), modalTitle = $('fsModalTitle');
        if (!modal || !modalVideo || !videoEl) return;
        modalVideo.srcObject = videoEl.srcObject;
        modalVideo.muted = videoEl.muted; // öz videomu səssiz saxla
        if (modalTitle) modalTitle.textContent = label || '';
        modal.classList.add('open');
    }
    function closeExpand() {
        const modal = $('fsModal'), modalVideo = $('fsModalVideo');
        if (modal) modal.classList.remove('open');
        if (modalVideo) modalVideo.srcObject = null;
    }
    // Stage expand üçün qlobal hook (inline onclick-dən çağırılır)
    window.acadexClassroomExpand = function (videoElId, label) {
        const v = document.getElementById(videoElId);
        if (v) expandStream(v, label);
    };

    // ── Müəllim: mute participant ──
    async function muteParticipant(targetUserId, targetName) {
        if (!conn || !targetUserId) return;
        try {
            await conn.invoke('MuteParticipant', lessonId, targetUserId);
            showToast((targetName || 'Tələbə') + ' susduruldu');
        } catch (e) { console.error('MuteParticipant failed', e); }
    }

    // Lokal mikrofonu məcburi bağla (müəllim əmri ilə)
    function applyForcedMute() {
        if (!localStream) return;
        localStream.getAudioTracks().forEach(t => t.enabled = false);
        const micBtn = $('btnMic');
        if (micBtn) {
            micBtn.classList.add('off');
            micBtn.innerHTML = '<i class="fas fa-microphone-slash"></i>';
        }
    }

    // ── Toolbar ──
    function bindToolbar() {
        $('btnMic').addEventListener('click', () => {
            if (!localStream) return;
            const enabled = localStream.getAudioTracks().some(t => t.enabled);
            localStream.getAudioTracks().forEach(t => t.enabled = !enabled);
            $('btnMic').classList.toggle('off', enabled);
            $('btnMic').innerHTML = enabled
                ? '<i class="fas fa-microphone-slash"></i>' : '<i class="fas fa-microphone"></i>';
        });

        $('btnCam').addEventListener('click', () => {
            if (!localStream) return;
            const enabled = localStream.getVideoTracks().some(t => t.enabled);
            localStream.getVideoTracks().forEach(t => t.enabled = !enabled);
            $('btnCam').classList.toggle('off', enabled);
            $('btnCam').innerHTML = enabled
                ? '<i class="fas fa-video-slash"></i>' : '<i class="fas fa-video"></i>';
        });

        const screenBtn = $('btnScreen');
        if (screenBtn) {
            screenBtn.addEventListener('click', () => {
                if (isSharing) stopScreenShare();
                else startScreenShare();
            });
        }

        const muteAllBtn = $('btnMuteAll');
        if (muteAllBtn) {
            muteAllBtn.addEventListener('click', async () => {
                const ok = await (window.acadexConfirm
                    ? window.acadexConfirm({ title: 'Hamısını sustur', message: 'Bütün tələbələrin mikrofonu bağlansın?', confirmText: 'Sustur', cancelText: 'Ləğv et', type: 'warning' })
                    : Promise.resolve(true));
                if (!ok) return;
                try { await conn.invoke('MuteAllParticipants', lessonId); showToast('Bütün tələbələr susduruldu'); }
                catch (e) { console.error('MuteAllParticipants failed', e); }
            });
        }

        // Fullscreen modal bağlama
        const fsClose = $('fsModalClose');
        if (fsClose) fsClose.addEventListener('click', closeExpand);
        const fsModal = $('fsModal');
        if (fsModal) fsModal.addEventListener('click', e => { if (e.target === fsModal) closeExpand(); });
        document.addEventListener('keydown', e => { if (e.key === 'Escape') closeExpand(); });

        $('btnLeave').addEventListener('click', async () => {
            const ok = await (window.acadexConfirm
                ? window.acadexConfirm({ title: 'Classroom-u tərk et', message: 'Classroom-u tərk etmək istəyirsiniz?', confirmText: 'Tərk et', cancelText: 'Qal', type: 'warning' })
                : Promise.resolve(confirm('Classroom-u tərk etmək istəyirsiniz?')));
            if (!ok) return;
            await leaveAndCleanup();
            history.back();
        });

        const endBtn = $('btnEnd');
        if (endBtn) {
            endBtn.addEventListener('click', async () => {
                const ok = await (window.acadexConfirm
                    ? window.acadexConfirm({ title: 'Dərsi bitir', message: 'Dərsi bitirmək istəyirsiniz? Bütün iştirakçılar disconnect olacaq.', confirmText: 'Bitir', cancelText: 'Ləğv et', type: 'danger' })
                    : Promise.resolve(confirm('Dərsi bitirmək istəyirsiniz? Bütün iştirakçılar disconnect olacaq.')));
                if (!ok) return;
                try {
                    const res = await fetch(apiBase + '/api/onlinelesson/' + lessonId + '/end', {
                        method: 'POST',
                        headers: { 'Authorization': 'Bearer ' + token }
                    });
                    if (!res.ok) { (window.acadexAlert || function(o){ alert((o&&o.message)||o); })({ title: 'Xəta', message: 'Dərsi bitirmək alınmadı', type: 'danger' }); return; }
                    showEndedOverlay();
                } catch { (window.acadexAlert || function(o){ alert((o&&o.message)||o); })({ title: 'Şəbəkə xətası', message: 'Şəbəkə xətası baş verdi', type: 'danger' }); }
            });
        }
    }

    async function leaveAndCleanup() {
        try { if (isSharing && conn) await conn.invoke('StopScreenShare', lessonId); } catch {}
        if (screenStream) { screenStream.getTracks().forEach(t => { try { t.stop(); } catch {} }); screenStream = null; }
        try { if (conn) await conn.invoke('LeaveLesson', lessonId); } catch {}
        Object.values(peers).forEach(p => { try { p.pc.close(); } catch {} });
        if (localStream) localStream.getTracks().forEach(t => t.stop());
        try { if (conn) await conn.stop(); } catch {}
    }

    function showEndedOverlay() {
        $('endedOverlay').style.display = 'flex';
        leaveAndCleanup();
    }

    // ── Pre-join lobby ──
    // İstifadəçi qoşulmazdan əvvəl mikrofon/kamera seçir.
    let preMicOn = true;
    let preCamOn = true;
    let preStream = null;

    async function initPreJoinPreview() {
        // Preview üçün media al (icazə pəncərəsi burada çıxır)
        try {
            preStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: true });
            const pv = $('preJoinVideo');
            if (pv) pv.srcObject = preStream;
        } catch (err) {
            console.warn('Preview media alınmadı:', err && err.name);
            // İcazə verilmədisə — toggle-ları off göstər, "media olmadan qoşul" mümkündür
            preMicOn = false; preCamOn = false;
            updatePreToggleUI();
            const hint = $('preJoinHint');
            if (hint) hint.innerHTML = '<i class="fas fa-triangle-exclamation" style="color:#f59e0b;"></i> Kamera/mikrofon icazəsi verilmədi. Media olmadan da qoşula bilərsiniz.';
        }
    }

    function updatePreToggleUI() {
        const micBtn = $('preMic'), camBtn = $('preCam'), preview = $('preJoinVideo')?.closest('.prejoin-preview');
        if (micBtn) {
            micBtn.classList.toggle('on', preMicOn);
            micBtn.classList.toggle('off', !preMicOn);
            micBtn.querySelector('i').className = preMicOn ? 'fas fa-microphone' : 'fas fa-microphone-slash';
        }
        if (camBtn) {
            camBtn.classList.toggle('on', preCamOn);
            camBtn.classList.toggle('off', !preCamOn);
            camBtn.querySelector('i').className = preCamOn ? 'fas fa-video' : 'fas fa-video-slash';
        }
        if (preview) preview.classList.toggle('cam-off', !preCamOn);
    }

    function bindPreJoin() {
        const micBtn = $('preMic'), camBtn = $('preCam');
        if (micBtn) micBtn.addEventListener('click', () => {
            preMicOn = !preMicOn;
            if (preStream) preStream.getAudioTracks().forEach(t => t.enabled = preMicOn);
            updatePreToggleUI();
        });
        if (camBtn) camBtn.addEventListener('click', () => {
            preCamOn = !preCamOn;
            if (preStream) preStream.getVideoTracks().forEach(t => t.enabled = preCamOn);
            updatePreToggleUI();
        });

        const joinBtn = $('preJoinBtn');
        if (joinBtn) joinBtn.addEventListener('click', () => finishPreJoin(true));

        const noMediaBtn = $('preJoinNoMedia');
        if (noMediaBtn) noMediaBtn.addEventListener('click', () => finishPreJoin(false));
    }

    async function finishPreJoin(useMedia) {
        // Preview stream-i bağla
        if (preStream) {
            preStream.getTracks().forEach(t => { try { t.stop(); } catch {} });
            preStream = null;
        }
        const pv = $('preJoinVideo');
        if (pv) pv.srcObject = null;

        // Lobby-ni gizlət
        const overlay = $('preJoinOverlay');
        if (overlay) overlay.classList.add('hidden');

        // Seçilmiş media ilə əsl classroom-a başla
        await start(useMedia, preMicOn, preCamOn);
    }

    // ── Init ──
    async function start(useMedia, micOn, camOn) {
        // 1) getUserMedia — yalnız istifadəçi media ilə qoşulmaq seçibsə
        if (useMedia) {
            try {
                localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: true });
                // Lobby-də edilən seçimləri tətbiq et
                localStream.getAudioTracks().forEach(t => t.enabled = micOn);
                localStream.getVideoTracks().forEach(t => t.enabled = camOn);
                $('localVideo').srcObject = localStream;
                // Toolbar düymə vəziyyətlərini lobby seçiminə uyğunlaşdır
                syncToolbarMediaState(micOn, camOn);
            } catch (err) {
                console.error(err);
                (window.acadexAlert || function (o) { alert((o && o.message) || o); })({ title: 'İcazə yoxdur', message: 'Kamera və ya mikrofon icazəsi rədd edildi. Media olmadan davam edilir.', type: 'warning' });
                localStream = null;
                syncToolbarMediaState(false, false);
            }
        } else {
            // Media olmadan qoşulma
            localStream = null;
            syncToolbarMediaState(false, false);
        }

        // 2) SignalR connect
        conn = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        conn.on('UserJoined', onUserJoined);
        conn.on('UserLeft', payload => { hideScreenStage(payload.connectionId); removePeer(payload.connectionId); });
        conn.on('ReceiveOffer', onReceiveOffer);
        conn.on('ReceiveAnswer', onReceiveAnswer);
        conn.on('ReceiveIceCandidate', onReceiveIce);
        conn.on('ReceiveClassMessage', appendChatMessage);
        conn.on('LessonEnded', showEndedOverlay);
        conn.on('LessonCancelled', showEndedOverlay);

        // Screen share event-ləri
        conn.on('UserStartedScreenShare', payload => {
            const isLocal = payload.connectionId === myConnectionId;
            showScreenStage(payload.connectionId, payload.userFullName, isLocal);
        });
        conn.on('UserStoppedScreenShare', payload => {
            hideScreenStage(payload.connectionId);
        });
        conn.on('ScreenShareDenied', payload => {
            // Server rədd etdi — lokal paylaşımı geri qaytar
            if (isSharing) {
                isSharing = false;
                if (cameraVideoTrack && cameraVideoTrack.readyState === 'live') replaceVideoTrackForAllPeers(cameraVideoTrack);
                if (screenStream) { screenStream.getTracks().forEach(t => { try { t.stop(); } catch {} }); screenStream = null; }
                const localVideo = $('localVideo');
                if (localVideo) localVideo.srcObject = localStream;
                updateScreenButton(false);
            }
            notify('Mümkün deyil', (payload.currentSharerName || 'Başqa iştirakçı') + ' artıq ekran paylaşır.', 'warning');
        });

        // Müəllim tərəfindən məcburi mute
        conn.on('ForceMutedByTeacher', payload => {
            applyForcedMute();
            showToast('Müəllim mikrofonunuzu bağladı.');
        });
        // Participant mute status (badge yeniləmə)
        conn.on('ParticipantMuted', payload => {
            // payload.userId həmin tələbənin userId-i
            const entry = Object.values(peers).find(p => p.userId === payload.userId);
            if (entry) { entry.muted = !!payload.muted; rebuildParticipantList(); }
        });
        conn.on('AllParticipantsMuted', payload => {
            Object.values(peers).forEach(p => {
                if (p.userRole !== 'Teacher' && p.userRole !== 'SuperAdmin') p.muted = true;
            });
            rebuildParticipantList();
        });

        try {
            await conn.start();
            myConnectionId = conn.connectionId;
            await conn.invoke('JoinLesson', lessonId);
            await loadChatHistory();
            bindChatUI();
            bindToolbar();
            setPartCount();
        } catch (err) {
            console.error('Hub connection failed', err);
            (window.acadexAlert || function(o){ alert((o&&o.message)||o); })({ title: 'Bağlantı uğursuz', message: 'Classroom-a qoşulmaq mümkün olmadı: ' + (err.message || err), type: 'danger' });
        }
    }

    window.addEventListener('beforeunload', () => { leaveAndCleanup(); });

    // Lobby axını: preview + seçim, sonra qoşulma
    bindPreJoin();
    initPreJoinPreview();
})();
