var BlogJs = (function () {

    var timer = null;
    var ftimer = null;
    var lastQ = '';

    function el(id) {
        return document.getElementById(id);
    }

    function openNav() {
        el('bl-nav').classList.add('bl-open');
        el('bl-scrim').classList.add('bl-open');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        el('bl-nav').classList.remove('bl-open');
        el('bl-scrim').classList.remove('bl-open');
        document.body.style.overflow = '';
    }

    function toggleSub(btn) {
        var li = btn.closest('.bl-mi');
        if (li) {
            li.classList.toggle('bl-exp');
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
            if (q === lastQ && el('bl-sugg').innerHTML !== '') {
                openSugg();
                return;
            }
            lastQ = q;
            $ApiRequest('Blog/Search', JSON.stringify([{ key: 'q', vlu: q }]));
        }, 250);
    }

    function openSugg() {
        el('bl-sugg').classList.add('bl-open');
    }

    function closeSugg() {
        el('bl-sugg').classList.remove('bl-open');
    }

    function values() {
        var list = [];
        var fields = document.querySelectorAll('.bl-fv');
        for (var i = 0; i < fields.length; i++) {
            list.push({ key: fields[i].getAttribute('data-k'), vlu: (fields[i].value || '').trim() });
        }
        return list;
    }

    function filter() {
        $WaitOn();
        $ApiRequest('Blog/Filter', JSON.stringify(values()));
    }

    function chip(btn, group, value) {
        var chips = btn.parentNode.querySelectorAll('.bl-chip');
        for (var i = 0; i < chips.length; i++) {
            chips[i].classList.remove('bl-act');
        }
        btn.classList.add('bl-act');
        el('bl-f-' + group).value = value;
        filter();
    }

    function typed() {
        clearTimeout(ftimer);
        ftimer = setTimeout(filter, 300);
    }

    function reset() {
        var fields = document.querySelectorAll('.bl-fv');
        for (var i = 0; i < fields.length; i++) {
            fields[i].value = '';
        }
        filter();
    }

    function more() {
        var list = values();
        list.push({ key: 'shown', vlu: String(document.querySelectorAll('#bl-grid .bl-card').length) });
        $WaitOn();
        $ApiRequest('Blog/More', JSON.stringify(list));
    }

    function send() {
        $WaitOn();
        $ApiRequest('Blog/Send', JSON.stringify([
            { key: 'name', vlu: el('bl-name').value },
            { key: 'email', vlu: el('bl-email').value },
            { key: 'topic', vlu: el('bl-topic').value },
            { key: 'message', vlu: el('bl-msg').value }
        ]));
    }

    function sent() {
        el('bl-name').value = '';
        el('bl-email').value = '';
        el('bl-topic').value = '';
        el('bl-msg').value = '';
    }

    function subscribe() {
        $ApiRequest('Blog/Subscribe', JSON.stringify([{ key: 'email', vlu: el('bl-nl-email').value }]));
    }

    function subscribed() {
        el('bl-nl-email').value = '';
    }

    function photo(id) {
        $ApiRequest('Blog/View', JSON.stringify(values().concat([{ key: 'id', vlu: id }])));
    }

    function openLb() {
        el('bl-lb').classList.add('bl-open');
        document.body.style.overflow = 'hidden';
    }

    function closeLb(e) {
        if (e && e.target && e.target !== el('bl-lb')) {
            return;
        }
        var lb = el('bl-lb');
        if (lb) {
            lb.classList.remove('bl-open');
        }
        document.body.style.overflow = '';
    }

    function step(dir) {
        var b = document.querySelector('#bl-lb-body [data-step="' + dir + '"]');
        if (b && el('bl-lb').classList.contains('bl-open')) {
            b.click();
        }
    }

    function pack() {
        $WaitOn();
        $ApiRequest('Blog/Pack', JSON.stringify([
            { key: 'type', vlu: el('bl-pk-type').value },
            { key: 'days', vlu: el('bl-pk-days').value },
            { key: 'climate', vlu: el('bl-pk-climate').value }
        ]));
    }

    function preset() {
        var fields = document.querySelectorAll('select.bl-fv');
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
            var box = el('bl-search');
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
            var h = el('bl-head');
            if (h) {
                h.classList.toggle('bl-scrolled', window.pageYOffset > 8);
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

BlogJs.reveal();
