var AboutJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('ab-nav').classList.add('ab-open');
        el('ab-scrim').classList.add('ab-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('ab-nav').classList.remove('ab-open');
        el('ab-scrim').classList.remove('ab-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.ab-mi');
        if (li) {
            li.classList.toggle('ab-exp');
        }
    }

    function search(value) {
        var q = (value || '').trim();
        clearTimeout(timer);
        if (q.length < 2) {
            closeSugg();
            lastQ = '';
            return;
        }
        timer = setTimeout(function () {
            if (q === lastQ && el('ab-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('About/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('ab-sugg').classList.add('ab-open');
    }

    function closeSugg() {
        el('ab-sugg').classList.remove('ab-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.ab-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('About/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.ab-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('ab-act');
        }
        btn.classList.add('ab-act');
        el('ab-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.ab-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#ab-grid .ab-card').length) });
        $WaitOn();
        $ApiRequest('About/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('About/Send', JSON.stringify([
            { key: 'name', vlu: el('ab-name').value },
            { key: 'email', vlu: el('ab-email').value },
            { key: 'topic', vlu: el('ab-topic').value },
            { key: 'message', vlu: el('ab-msg').value }
        ]));
    }

    function sent() {
        el('ab-name').value = '';
        el('ab-email').value = '';
        el('ab-topic').value = '';
        el('ab-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('About/Subscribe', JSON.stringify([{ key: 'email', vlu: el('ab-nl-email').value }]));
    }

    function subscribed() {
        el('ab-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('About/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('ab-lb').classList.add('ab-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('ab-lb')) {
            return;
        }
        var lb = el('ab-lb');
        if (lb) {
            lb.classList.remove('ab-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#ab-lb-body [data-step="' + dir + '"]');
        if (b && el('ab-lb').classList.contains('ab-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('About/Pack', JSON.stringify([
            { key: 'type', vlu: el('ab-pk-type').value },
            { key: 'days', vlu: el('ab-pk-days').value },
            { key: 'climate', vlu: el('ab-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.ab-fv');
        for (var i = 0; i < fields.length; i++) {
            var v = fields[i].getAttribute('data-v');
            if (v) {
                fields[i].value = v;
            }
        }
    }

    function reveal() {
        preset();
        document.addEventListener('click', function (e) {
            var box = el('ab-search');
            if (box && !box.contains(e.target)) {
                closeSugg();
            }
        });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                closeSugg();
                closeNav();
                closeLb();
            }
            if (e.key === 'ArrowRight') {
                step('next');
            }
            if (e.key === 'ArrowLeft') {
                step('prev');
            }
        });
        window.addEventListener('scroll', function () {
            var h = el('ab-head');
            if (h) {
                h.classList.toggle('ab-scrolled', window.pageYOffset > 8);
            }
        }, { passive: true });
        window.addEventListener('resize', function () {
            if (window.innerWidth > 767) {
                closeNav();
            }
        });
    }

    return {
        reveal: function () { reveal(); },
        openNav: function () { openNav(); },
        closeNav: function () { closeNav(); },
        toggleSub: function (btn) { toggleSub(btn); },
        search: function (v) { search(v); },
        openSugg: function () { openSugg(); },
        closeSugg: function () { closeSugg(); },
        filter: function () { filter(); },
        chip: function (btn, g, v) { chip(btn, g, v); },
        typed: function () { typed(); },
        reset: function () { reset(); },
        more: function () { more(); },
        send: function () { send(); },
        sent: function () { sent(); },
        subscribe: function () { subscribe(); },
        subscribed: function () { subscribed(); },
        photo: function (id) { photo(id); },
        openLb: function () { openLb(); },
        closeLb: function (e) { closeLb(e); },
        pack: function () { pack(); }
    };

})();

AboutJs.reveal();
