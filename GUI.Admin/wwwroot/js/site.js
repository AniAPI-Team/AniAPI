﻿document.addEventListener('DOMContentLoaded', () => {
    if (window.innerWidth < 1200) {
        document.getElementById('root').classList.add('mobile');
    }

    document.querySelectorAll('.sidebar .nav-link').forEach((el) => {
        if (el.nextElementSibling) {
            const activeItems = el.nextElementSibling.querySelectorAll('.nav-link.active');
            new bootstrap.Collapse(el.nextElementSibling, { toggle: activeItems.length > 0 }).hide();
        }

        el.addEventListener('click', (e) => {
            const nextEl = el.nextElementSibling;
            const parentEl = el.parentElement;

            if (nextEl) {
                e.preventDefault();

                const icon = el.querySelector('.submenu-icon');
                const collapsed = nextEl.classList.contains('show');

                if (collapsed) {
                    new bootstrap.Collapse(nextEl).hide();
                    icon.classList.remove('bi-chevron-down');
                    icon.classList.add('bi-chevron-right');
                }
                else {
                    new bootstrap.Collapse(nextEl).show();
                    icon.classList.remove('bi-chevron-right');
                    icon.classList.add('bi-chevron-down');
                }
            }
        });
    });
});

const pageLoaded = () => {
    document.getElementById('page-loader').classList.add('ended');
    setTimeout(() => {
        document.getElementById('page-loader').remove();
    }, 310);
}

const toggleCollapse = () => {
    const root = document.getElementById('root');
    root.classList.toggle('collapsed');

    const size = root.classList.contains('collapsed') ? 70 : 280;

    if (typeof (onToggleCollapse) === typeof (Function)) {
        onToggleCollapse(size);
    }
}


const formatNumber = (n) => {
    const m = n.toString();
    const l = m.length;
    let i = l - 1;
    let j = 1;
    let n2 = '';

    const k = m.toString().indexOf('.');
    if (k !== -1) {
        n2 += m.substring(k);
        i = k - 1;
    }

    while (i >= 0) {
        n2 = m[i] + n2;

        if (j % 3 === 0 && i != 0) {
            n2 = ',' + n2;
            j = 0;
        }

        i--;
        j++;
    }

    return n2;
}

const formatDate = (date) => {
    const y = date.getFullYear();
    const m = date.getMonth() + 1;
    const d = date.getDate();

    return y + '-' +
        (m < 10 ? '0' + m : m) + '-' +
        (d < 10 ? '0' + d : d);
}

const formatTime = (date) => {
    const y = date.getFullYear();
    const m = date.getMonth() + 1;
    const d = date.getDate();

    const z = (date.getHours() / 24) > 0.5 ? 'PM' : 'AM';
    const h = date.getHours();
    const mm = date.getMinutes();

    return y + '-' +
        (m < 10 ? '0' + m : m) + '-' +
        (d < 10 ? '0' + d : d) + ' ' +
        (h < 10 ? '0' + h : h) + ':' +
        (mm < 10 ? '0' + mm : mm) + ' ' + z;
}

const getCurrentTime = () => {
    const now = new Date();

    const z = (now.getHours() / 24) > 0.5 ? 'PM' : 'AM';
    const h = now.getHours() % 12;
    const m = now.getMinutes();
    const s = now.getSeconds();

    return (h < 10 ? '0' + h : h) + ':' +
        (m < 10 ? '0' + m : m) + ':' +
        (s < 10 ? '0' + s : s) + ' ' + z;
}

const getCurrentTimestamp = (date) => {
    return Math.floor(date.getTime() / 1000);
}

const byteConverter = (bytes, size = true, decimals = 2) => {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + (size ? (' ' + sizes[i]) : '');
}

const APIConverters = {
    format: (f) => {
        switch (f) {
            case 0: return 'TV';
            case 1: return 'TV Short';
            case 2: return 'Movie';
            case 3: return 'Special';
            case 4: return 'OVA';
            case 5: return 'ONA';
            case 6: return 'Music';
        }
    },
    status: (s) => {
        switch (s) {
            case 0: return 'Completed';
            case 1: return 'Releasing';
            case 2: return 'Coming soon';
            case 3: return 'Cancelled';
        }
    },
    season: (s) => {
        switch (s) {
            case 0: return 'Winter';
            case 1: return 'Spring';
            case 2: return 'Summer';
            case 3: return 'Fall';
            case 4: return 'Unknown';
        }
    }
}

String.prototype.toHex = function () {
    var hash = 0;
    if (this.length === 0) return hash;
    for (var i = 0; i < this.length; i++) {
        hash = this.charCodeAt(i) + ((hash << 5) - hash);
        hash = hash & hash;
    }
    var color = '#';
    for (var i = 0; i < 3; i++) {
        var value = (hash >> (i * 8)) & 255;
        color += ('00' + value.toString(16)).substr(-2);
    }
    return color;
}