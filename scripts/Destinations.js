var DestinationsJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('ds-nav').classList.add('ds-open');
        el('ds-scrim').classList.add('ds-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('ds-nav').classList.remove('ds-open');
        el('ds-scrim').classList.remove('ds-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.ds-mi');
        if (li) {
            li.classList.toggle('ds-exp');
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
            if (q === lastQ && el('ds-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Destinations/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('ds-sugg').classList.add('ds-open');
    }

    function closeSugg() {
        el('ds-sugg').classList.remove('ds-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.ds-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Destinations/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.ds-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('ds-act');
        }
        btn.classList.add('ds-act');
        el('ds-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.ds-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#ds-grid .ds-card').length) });
        $WaitOn();
        $ApiRequest('Destinations/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('Destinations/Send', JSON.stringify([
            { key: 'name', vlu: el('ds-name').value },
            { key: 'email', vlu: el('ds-email').value },
            { key: 'topic', vlu: el('ds-topic').value },
            { key: 'message', vlu: el('ds-msg').value }
        ]));
    }

    function sent() {
        el('ds-name').value = '';
        el('ds-email').value = '';
        el('ds-topic').value = '';
        el('ds-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('Destinations/Subscribe', JSON.stringify([{ key: 'email', vlu: el('ds-nl-email').value }]));
    }

    function subscribed() {
        el('ds-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('Destinations/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('ds-lb').classList.add('ds-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('ds-lb')) {
            return;
        }
        var lb = el('ds-lb');
        if (lb) {
            lb.classList.remove('ds-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#ds-lb-body [data-step="' + dir + '"]');
        if (b && el('ds-lb').classList.contains('ds-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('Destinations/Pack', JSON.stringify([
            { key: 'type', vlu: el('ds-pk-type').value },
            { key: 'days', vlu: el('ds-pk-days').value },
            { key: 'climate', vlu: el('ds-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.ds-fv');
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
            var box = el('ds-search');
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
            var h = el('ds-head');
            if (h) {
                h.classList.toggle('ds-scrolled', window.pageYOffset > 8);
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

DestinationsJs.reveal();
