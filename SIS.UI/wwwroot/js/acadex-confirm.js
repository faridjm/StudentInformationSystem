/*!
 * Acadex Confirm — saytın native browser confirm()/alert() dialoq-larını
 * bütün admin / teacher / student / publicadmin panellərində Acadex theme-inə
 * uyğun custom modal-la əvəz edir.
 *
 * İstifadə (HTML):
 *   <form ... data-confirm="Silinsin?">              -> form submit-də modal
 *   <form ... data-confirm-title="..." data-confirm-message="..." data-confirm-text="Sil" data-confirm-cancel="Ləğv et" data-confirm-type="danger">
 *   <button data-confirm="..." onclick="..."> ...   -> button click-də modal
 *
 * İstifadə (JS):
 *   window.acadexConfirm({ title, message, confirmText, cancelText, type, onConfirm }) -> Promise<boolean>
 *   window.acadexAlert({ title, message })                                              -> Promise<void>
 */
(function () {
    'use strict';
    if (window.__acadexConfirmReady) return;
    window.__acadexConfirmReady = true;

    // ---------- Modal CSS (idempotent inject) ----------
    var css = '' +
        '.acx-modal-backdrop{position:fixed;inset:0;background:rgba(15,23,42,.55);backdrop-filter:blur(4px);z-index:99998;display:none;align-items:center;justify-content:center;animation:acx-fade .15s ease;}' +
        '.acx-modal-backdrop.show{display:flex;}' +
        '.acx-modal{background:#fff;color:#0f172a;width:min(92vw,460px);border-radius:16px;box-shadow:0 25px 60px rgba(0,0,0,.35);overflow:hidden;animation:acx-pop .18s ease;border:1px solid rgba(15,23,42,.08);}' +
        '.acx-modal-head{display:flex;align-items:center;gap:14px;padding:22px 24px 8px;}' +
        '.acx-modal-icon{width:46px;height:46px;border-radius:50%;display:inline-flex;align-items:center;justify-content:center;font-size:20px;flex-shrink:0;}' +
        '.acx-modal-icon.danger{background:rgba(220,38,38,.12);color:#dc2626;}' +
        '.acx-modal-icon.warning{background:rgba(245,158,11,.15);color:#b45309;}' +
        '.acx-modal-icon.info{background:rgba(79,70,229,.12);color:#4f46e5;}' +
        '.acx-modal-icon.success{background:rgba(16,185,129,.15);color:#047857;}' +
        '.acx-modal-title{font-size:17px;font-weight:700;margin:0;line-height:1.3;}' +
        '.acx-modal-body{padding:6px 24px 18px;color:#475569;font-size:14px;line-height:1.55;}' +
        '.acx-modal-foot{display:flex;gap:10px;justify-content:flex-end;padding:14px 20px 20px;background:#f8fafc;border-top:1px solid #e2e8f0;}' +
        '.acx-modal-btn{padding:10px 18px;border-radius:10px;border:none;font-size:14px;font-weight:600;cursor:pointer;transition:transform .08s,filter .15s;}' +
        '.acx-modal-btn:hover{filter:brightness(.95);}' +
        '.acx-modal-btn:active{transform:translateY(1px);}' +
        '.acx-modal-btn.cancel{background:#e2e8f0;color:#0f172a;}' +
        '.acx-modal-btn.confirm.danger{background:#dc2626;color:#fff;}' +
        '.acx-modal-btn.confirm.warning{background:#f59e0b;color:#fff;}' +
        '.acx-modal-btn.confirm.info{background:linear-gradient(135deg,#7c3aed,#4f46e5);color:#fff;}' +
        '.acx-modal-btn.confirm.success{background:#10b981;color:#fff;}' +
        '@keyframes acx-fade{from{opacity:0}to{opacity:1}}' +
        '@keyframes acx-pop{from{opacity:0;transform:translateY(-8px) scale(.97)}to{opacity:1;transform:none}}' +
        '@media (prefers-color-scheme: dark){.acx-modal{background:#0f172a;color:#f1f5f9;border-color:rgba(255,255,255,.08);}.acx-modal-body{color:#cbd5e1;}.acx-modal-foot{background:#1e293b;border-top-color:rgba(255,255,255,.06);}.acx-modal-btn.cancel{background:#334155;color:#f1f5f9;}}';

    var styleEl = document.createElement('style');
    styleEl.id = 'acx-confirm-style';
    styleEl.textContent = css;
    document.head.appendChild(styleEl);

    // ---------- Modal element (singleton) ----------
    var backdrop = document.createElement('div');
    backdrop.className = 'acx-modal-backdrop';
    backdrop.innerHTML =
        '<div class="acx-modal" role="dialog" aria-modal="true" aria-labelledby="acxModalTitle">' +
        '  <div class="acx-modal-head">' +
        '    <div class="acx-modal-icon danger" id="acxModalIcon"><i class="fas fa-triangle-exclamation"></i></div>' +
        '    <h3 class="acx-modal-title" id="acxModalTitle">Əmr təsdiqi</h3>' +
        '  </div>' +
        '  <div class="acx-modal-body" id="acxModalBody">Bu əməliyyatı təsdiqləyirsiniz?</div>' +
        '  <div class="acx-modal-foot">' +
        '    <button type="button" class="acx-modal-btn cancel" id="acxModalCancel">Ləğv et</button>' +
        '    <button type="button" class="acx-modal-btn confirm danger" id="acxModalConfirm">Sil</button>' +
        '  </div>' +
        '</div>';
    document.body.appendChild(backdrop);

    var modalEl = backdrop.querySelector('.acx-modal');
    var iconEl = backdrop.querySelector('#acxModalIcon');
    var titleEl = backdrop.querySelector('#acxModalTitle');
    var bodyEl = backdrop.querySelector('#acxModalBody');
    var cancelBtn = backdrop.querySelector('#acxModalCancel');
    var confirmBtn = backdrop.querySelector('#acxModalConfirm');

    var current = { resolve: null, alertOnly: false };
    var lastFocus = null;

    function escHandler(e) {
        if (e.key === 'Escape') close(false);
        if (e.key === 'Enter' && !current.alertOnly) close(true);
    }

    function open(opts) {
        var type = (opts.type || 'danger').toLowerCase();
        var iconClass = ({
            danger: 'fa-triangle-exclamation',
            warning: 'fa-circle-exclamation',
            info: 'fa-circle-info',
            success: 'fa-circle-check'
        })[type] || 'fa-triangle-exclamation';

        iconEl.className = 'acx-modal-icon ' + type;
        iconEl.innerHTML = '<i class="fas ' + iconClass + '"></i>';
        titleEl.textContent = opts.title || 'Əmr təsdiqi';
        bodyEl.innerHTML = opts.message || 'Davam etmək istəyirsiniz?';
        confirmBtn.className = 'acx-modal-btn confirm ' + type;
        confirmBtn.textContent = opts.confirmText || 'Təsdiq et';
        cancelBtn.textContent = opts.cancelText || 'Ləğv et';
        cancelBtn.style.display = opts.alertOnly ? 'none' : '';

        current.alertOnly = !!opts.alertOnly;

        backdrop.classList.add('show');
        lastFocus = document.activeElement;
        setTimeout(function () { confirmBtn.focus(); }, 50);
        document.addEventListener('keydown', escHandler);

        return new Promise(function (resolve) { current.resolve = resolve; });
    }

    function close(result) {
        backdrop.classList.remove('show');
        document.removeEventListener('keydown', escHandler);
        if (lastFocus && lastFocus.focus) try { lastFocus.focus(); } catch (e) { }
        if (current.resolve) {
            var r = current.resolve;
            current.resolve = null;
            r(result);
        }
    }

    confirmBtn.addEventListener('click', function () { close(true); });
    cancelBtn.addEventListener('click', function () { close(false); });
    backdrop.addEventListener('click', function (e) { if (e.target === backdrop) close(false); });

    // ---------- Public API ----------
    window.acadexConfirm = function (opts) {
        opts = opts || {};
        return open({
            title: opts.title,
            message: opts.message,
            confirmText: opts.confirmText,
            cancelText: opts.cancelText,
            type: opts.type || 'danger'
        }).then(function (ok) {
            if (ok && typeof opts.onConfirm === 'function') {
                try { opts.onConfirm(); } catch (e) { console.error(e); }
            }
            return ok;
        });
    };

    window.acadexAlert = function (opts) {
        if (typeof opts === 'string') opts = { message: opts };
        opts = opts || {};
        return open({
            title: opts.title || 'Bildiriş',
            message: opts.message || '',
            confirmText: opts.confirmText || 'OK',
            cancelText: '',
            type: opts.type || 'info',
            alertOnly: true
        });
    };

    // ---------- Auto-binding ----------
    // Form: <form data-confirm="..."> => submit-də modal göstər; submit yalnız təsdiq olunsa
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form || !form.matches) return;
        if (!form.matches('form[data-confirm], form[data-confirm-message]')) return;
        if (form.dataset.acxConfirmed === '1') {
            // İkinci submit — modal təsdiqindən sonra
            form.dataset.acxConfirmed = '';
            return;
        }
        e.preventDefault();
        e.stopPropagation();
        showFromDataset(form).then(function (ok) {
            if (ok) {
                form.dataset.acxConfirmed = '1';
                if (typeof form.requestSubmit === 'function') form.requestSubmit();
                else form.submit();
            }
        });
    }, true);

    // Button: <button data-confirm="..." type="button">  və ya  <a data-confirm="...">
    document.addEventListener('click', function (e) {
        var el = e.target.closest && e.target.closest('[data-confirm], [data-confirm-message]');
        if (!el) return;
        // form submit button-ları submit handler-ə düşür
        if (el.matches && el.matches('button[type="submit"]')) return;
        if (el.tagName === 'FORM') return;

        // Anchor / button click
        if (el.dataset.acxConfirmed === '1') { el.dataset.acxConfirmed = ''; return; }
        e.preventDefault();
        e.stopPropagation();
        showFromDataset(el).then(function (ok) {
            if (!ok) return;
            el.dataset.acxConfirmed = '1';
            if (el.tagName === 'A' && el.href) { window.location.href = el.href; }
            else el.click();
        });
    }, true);

    function showFromDataset(el) {
        var msg = el.dataset.confirmMessage || el.dataset.confirm || 'Davam etmək istəyirsiniz?';
        var title = el.dataset.confirmTitle || 'Əmr təsdiqi';
        return window.acadexConfirm({
            title: title,
            message: msg,
            confirmText: el.dataset.confirmText || 'Təsdiq et',
            cancelText: el.dataset.confirmCancel || 'Ləğv et',
            type: el.dataset.confirmType || 'danger'
        });
    }
})();
