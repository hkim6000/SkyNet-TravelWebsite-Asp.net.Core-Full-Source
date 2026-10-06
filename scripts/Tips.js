var TipsJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('tp-nav').classList.add('tp-open');
        el('tp-scrim').classList.add('tp-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('tp-nav').classList.remove('tp-open');
        el('tp-scrim').classList.remove('tp-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.tp-mi');
        if (li) {
            li.classList.toggle('tp-exp');
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
            if (q === lastQ && el('tp-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Tips/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('tp-sugg').classList.add('tp-open');
    }

    function closeSugg() {
        el('tp-sugg').classList.remove('tp-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.tp-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Tips/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.tp-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('tp-act');
        }
        btn.classList.add('tp-act');
        el('tp-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.tp-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#tp-grid .tp-card').length) });
        $WaitOn();
        $ApiRequest('Tips/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('Tips/Send', JSON.stringify([
            { key: 'name', vlu: el('tp-name').value },
            { key: 'email', vlu: el('tp-email').value },
            { key: 'topic', vlu: el('tp-topic').value },
            { key: 'message', vlu: el('tp-msg').value }
        ]));
    }

    function sent() {
        el('tp-name').value = '';
        el('tp-email').value = '';
        el('tp-topic').value = '';
        el('tp-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('Tips/Subscribe', JSON.stringify([{ key: 'email', vlu: el('tp-nl-email').value }]));
    }

    function subscribed() {
        el('tp-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('Tips/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('tp-lb').classList.add('tp-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('tp-lb')) {
            return;
        }
        var lb = el('tp-lb');
        if (lb) {
            lb.classList.remove('tp-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#tp-lb-body [data-step="' + dir + '"]');
        if (b && el('tp-lb').classList.contains('tp-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('Tips/Pack', JSON.stringify([
            { key: 'type', vlu: el('tp-pk-type').value },
            { key: 'days', vlu: el('tp-pk-days').value },
            { key: 'climate', vlu: el('tp-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.tp-fv');
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
            var box = el('tp-search');
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
            var h = el('tp-head');
            if (h) {
                h.classList.toggle('tp-scrolled', window.pageYOffset > 8);
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

TipsJs.reveal();
