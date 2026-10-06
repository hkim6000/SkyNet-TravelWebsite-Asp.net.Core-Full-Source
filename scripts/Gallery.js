var GalleryJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('ga-nav').classList.add('ga-open');
        el('ga-scrim').classList.add('ga-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('ga-nav').classList.remove('ga-open');
        el('ga-scrim').classList.remove('ga-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.ga-mi');
        if (li) {
            li.classList.toggle('ga-exp');
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
            if (q === lastQ && el('ga-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Gallery/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('ga-sugg').classList.add('ga-open');
    }

    function closeSugg() {
        el('ga-sugg').classList.remove('ga-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.ga-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Gallery/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.ga-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('ga-act');
        }
        btn.classList.add('ga-act');
        el('ga-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.ga-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#ga-grid .ga-card').length) });
        $WaitOn();
        $ApiRequest('Gallery/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('Gallery/Send', JSON.stringify([
            { key: 'name', vlu: el('ga-name').value },
            { key: 'email', vlu: el('ga-email').value },
            { key: 'topic', vlu: el('ga-topic').value },
            { key: 'message', vlu: el('ga-msg').value }
        ]));
    }

    function sent() {
        el('ga-name').value = '';
        el('ga-email').value = '';
        el('ga-topic').value = '';
        el('ga-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('Gallery/Subscribe', JSON.stringify([{ key: 'email', vlu: el('ga-nl-email').value }]));
    }

    function subscribed() {
        el('ga-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('Gallery/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('ga-lb').classList.add('ga-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('ga-lb')) {
            return;
        }
        var lb = el('ga-lb');
        if (lb) {
            lb.classList.remove('ga-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#ga-lb-body [data-step="' + dir + '"]');
        if (b && el('ga-lb').classList.contains('ga-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('Gallery/Pack', JSON.stringify([
            { key: 'type', vlu: el('ga-pk-type').value },
            { key: 'days', vlu: el('ga-pk-days').value },
            { key: 'climate', vlu: el('ga-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.ga-fv');
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
            var box = el('ga-search');
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
            var h = el('ga-head');
            if (h) {
                h.classList.toggle('ga-scrolled', window.pageYOffset > 8);
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

GalleryJs.reveal();
