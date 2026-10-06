var PostJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('po-nav').classList.add('po-open');
        el('po-scrim').classList.add('po-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('po-nav').classList.remove('po-open');
        el('po-scrim').classList.remove('po-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.po-mi');
        if (li) {
            li.classList.toggle('po-exp');
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
            if (q === lastQ && el('po-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Post/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('po-sugg').classList.add('po-open');
    }

    function closeSugg() {
        el('po-sugg').classList.remove('po-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.po-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Post/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.po-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('po-act');
        }
        btn.classList.add('po-act');
        el('po-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.po-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#po-grid .po-card').length) });
        $WaitOn();
        $ApiRequest('Post/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('Post/Send', JSON.stringify([
            { key: 'name', vlu: el('po-name').value },
            { key: 'email', vlu: el('po-email').value },
            { key: 'topic', vlu: el('po-topic').value },
            { key: 'message', vlu: el('po-msg').value }
        ]));
    }

    function sent() {
        el('po-name').value = '';
        el('po-email').value = '';
        el('po-topic').value = '';
        el('po-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('Post/Subscribe', JSON.stringify([{ key: 'email', vlu: el('po-nl-email').value }]));
    }

    function subscribed() {
        el('po-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('Post/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('po-lb').classList.add('po-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('po-lb')) {
            return;
        }
        var lb = el('po-lb');
        if (lb) {
            lb.classList.remove('po-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#po-lb-body [data-step="' + dir + '"]');
        if (b && el('po-lb').classList.contains('po-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('Post/Pack', JSON.stringify([
            { key: 'type', vlu: el('po-pk-type').value },
            { key: 'days', vlu: el('po-pk-days').value },
            { key: 'climate', vlu: el('po-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.po-fv');
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
            var box = el('po-search');
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
            var h = el('po-head');
            if (h) {
                h.classList.toggle('po-scrolled', window.pageYOffset > 8);
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

PostJs.reveal();
