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

    // ── Browser support ──
    if (!window.RTCPeerConnection || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        alert('Your browser does not support WebRTC.');
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
        tile.appendChild(v); tile.appendChild(name);
        grid.appendChild(tile);
        return v;
    }

    function removeRemoteTile(connectionId) {
        const t = $('tile-' + connectionId);
        if (t) t.remove();
    }

    function rebuildParticipantList() {
        partList.innerHTML = '<div class="participant-row"><div class="participant-dot"></div><span>' +
            escapeHtml(myUserName) + ' (siz)</span></div>';
        Object.values(peers).forEach(p => {
            const row = document.createElement('div');
            row.className = 'participant-row';
            row.innerHTML = '<div class="participant-dot"></div><span></span>';
            row.querySelector('span').textContent = p.userName || 'İştirakçı';
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
    function createPeer(connectionId, userId, userName) {
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
        };

        pc.onconnectionstatechange = () => {
            if (pc.connectionState === 'failed' || pc.connectionState === 'closed') {
                removePeer(connectionId);
            }
        };

        peers[connectionId] = { pc, userId, userName };
        addRemoteTile(connectionId, userName);
        rebuildParticipantList();
        return pc;
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
        const pc = createPeer(payload.connectionId, payload.userId, payload.userId);
        try {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            await conn.invoke('SendOffer', lessonId, payload.connectionId, offer);
        } catch (e) { console.error('createOffer failed', e); }
    }

    async function onReceiveOffer(payload) {
        let p = peers[payload.fromConnectionId];
        const pc = p ? p.pc : createPeer(payload.fromConnectionId, payload.fromUserId, payload.fromUserId);
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
            } catch (e) { alert('Mesaj göndərilmədi'); }
        };
        sendBtn.addEventListener('click', send);
        inp.addEventListener('keypress', e => { if (e.key === 'Enter') { e.preventDefault(); send(); } });
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

        $('btnLeave').addEventListener('click', async () => {
            if (!confirm('Classroom-u tərk etmək istəyirsiniz?')) return;
            await leaveAndCleanup();
            history.back();
        });

        const endBtn = $('btnEnd');
        if (endBtn) {
            endBtn.addEventListener('click', async () => {
                if (!confirm('Dərsi bitirmək istəyirsiniz? Bütün iştirakçılar disconnect olacaq.')) return;
                try {
                    const res = await fetch(apiBase + '/api/onlinelesson/' + lessonId + '/end', {
                        method: 'POST',
                        headers: { 'Authorization': 'Bearer ' + token }
                    });
                    if (!res.ok) { alert('Dərsi bitirmək alınmadı'); return; }
                    showEndedOverlay();
                } catch { alert('Şəbəkə xətası'); }
            });
        }
    }

    async function leaveAndCleanup() {
        try { if (conn) await conn.invoke('LeaveLesson', lessonId); } catch {}
        Object.values(peers).forEach(p => { try { p.pc.close(); } catch {} });
        if (localStream) localStream.getTracks().forEach(t => t.stop());
        try { if (conn) await conn.stop(); } catch {}
    }

    function showEndedOverlay() {
        $('endedOverlay').style.display = 'flex';
        leaveAndCleanup();
    }

    // ── Init ──
    async function start() {
        // 1) getUserMedia
        try {
            localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: true });
            $('localVideo').srcObject = localStream;
        } catch (err) {
            alert('Camera or microphone permission denied.');
            console.error(err);
            return;
        }

        // 2) SignalR connect
        conn = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        conn.on('UserJoined', onUserJoined);
        conn.on('UserLeft', payload => removePeer(payload.connectionId));
        conn.on('ReceiveOffer', onReceiveOffer);
        conn.on('ReceiveAnswer', onReceiveAnswer);
        conn.on('ReceiveIceCandidate', onReceiveIce);
        conn.on('ReceiveClassMessage', appendChatMessage);
        conn.on('LessonEnded', showEndedOverlay);
        conn.on('LessonCancelled', showEndedOverlay);

        try {
            await conn.start();
            await conn.invoke('JoinLesson', lessonId);
            await loadChatHistory();
            bindChatUI();
            bindToolbar();
            setPartCount();
        } catch (err) {
            console.error('Hub connection failed', err);
            alert('Classroom-a qoşulmaq mümkün olmadı: ' + (err.message || err));
        }
    }

    window.addEventListener('beforeunload', () => { leaveAndCleanup(); });

    start();
})();
