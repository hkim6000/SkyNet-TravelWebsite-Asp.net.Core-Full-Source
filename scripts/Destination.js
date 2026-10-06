var DestinationJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('dt-nav').classList.add('dt-open');
        el('dt-scrim').classList.add('dt-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('dt-nav').classList.remove('dt-open');
        el('dt-scrim').classList.remove('dt-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.dt-mi');
        if (li) {
            li.classList.toggle('dt-exp');
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
            if (q === lastQ && el('dt-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Destination/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('dt-sugg').classList.add('dt-open');
    }

    function closeSugg() {
        el('dt-sugg').classList.remove('dt-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.dt-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Destination/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.dt-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('dt-act');
        }
        btn.classList.add('dt-act');
        el('dt-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.dt-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#dt-grid .dt-card').length) });
        $WaitOn();
        $ApiRequest('Destination/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('Destination/Send', JSON.stringify([
            { key: 'name', vlu: el('dt-name').value },
            { key: 'email', vlu: el('dt-email').value },
            { key: 'topic', vlu: el('dt-topic').value },
            { key: 'message', vlu: el('dt-msg').value }
        ]));
    }

    function sent() {
        el('dt-name').value = '';
        el('dt-email').value = '';
        el('dt-topic').value = '';
        el('dt-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('Destination/Subscribe', JSON.stringify([{ key: 'email', vlu: el('dt-nl-email').value }]));
    }

    function subscribed() {
        el('dt-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('Destination/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('dt-lb').classList.add('dt-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('dt-lb')) {
            return;
        }
        var lb = el('dt-lb');
        if (lb) {
            lb.classList.remove('dt-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#dt-lb-body [data-step="' + dir + '"]');
        if (b && el('dt-lb').classList.contains('dt-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('Destination/Pack', JSON.stringify([
            { key: 'type', vlu: el('dt-pk-type').value },
            { key: 'days', vlu: el('dt-pk-days').value },
            { key: 'climate', vlu: el('dt-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.dt-fv');
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
            var box = el('dt-search');
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
            var h = el('dt-head');
            if (h) {
                h.classList.toggle('dt-scrolled', window.pageYOffset > 8);
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

DestinationJs.reveal();
