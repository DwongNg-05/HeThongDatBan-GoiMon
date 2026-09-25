const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');

const source = readFileSync(join(__dirname, '../HeThongDatBan-GoiMon/wwwroot/js/login-lockout.js'), 'utf8');

function page() {
    let milliseconds = 0;
    let tick;
    let onInput;
    let onVisibility;
    let stopped = false;
    const classes = new Set(['alert-warning']);
    const notice = {
        dataset: { remainingSeconds: '900' }, hidden: false, textContent: '',
        classList: { replace: (oldValue, newValue) => { classes.delete(oldValue); classes.add(newValue); } }
    };
    const countdown = { textContent: '' };
    const identifier = { value: 'quanly1', addEventListener: (_, callback) => { onInput = callback; } };
    runInNewContext(source, {
        document: {
            getElementById: id => ({ 'login-lockout': notice, 'lockout-countdown': countdown, Identifier: identifier })[id],
            addEventListener: (_, callback) => { onVisibility = callback; }
        },
        performance: { now: () => milliseconds },
        setInterval: callback => { tick = callback; return 1; },
        clearInterval: () => { stopped = true; }
    });
    return {
        notice, countdown, classes,
        advance: value => { milliseconds += value; tick(); },
        resumeAfter: value => { milliseconds += value; onVisibility(); },
        changeIdentifier: value => { identifier.value = value; onInput(); },
        stopped: () => stopped
    };
}

test('counts actual elapsed time and catches up after a hidden tab resumes', () => {
    const ui = page();
    assert.equal(ui.countdown.textContent, '15:00');
    ui.advance(61000);
    assert.equal(ui.countdown.textContent, '13:59');
    ui.resumeAfter(600000);
    assert.equal(ui.countdown.textContent, '03:59');
});

test('expires without a negative timer and tells the user to retry', () => {
    const ui = page();
    ui.advance(899999);
    assert.equal(ui.countdown.textContent, '00:01');
    ui.advance(1);
    assert.equal(ui.notice.textContent, 'Đã hết thời gian khóa. Bạn có thể thử đăng nhập lại.');
    assert.ok(ui.classes.has('alert-info'));
    assert.ok(ui.stopped());
});

test('hides the previous account countdown when a different identifier is entered', () => {
    const ui = page();
    ui.changeIdentifier('quanly2');
    assert.ok(ui.notice.hidden);
    ui.advance(10000);
    ui.changeIdentifier(' QUANLY1 ');
    assert.equal(ui.notice.hidden, false);
    assert.equal(ui.countdown.textContent, '14:50');
});

test('ordinary login pages do not create a timer', () => {
    runInNewContext(source, {
        document: { getElementById: () => null },
        setInterval: () => assert.fail('Unexpected timer')
    });
});
