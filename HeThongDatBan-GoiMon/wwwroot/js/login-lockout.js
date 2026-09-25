(() => {
    const notice = document.getElementById("login-lockout");
    const countdown = document.getElementById("lockout-countdown");
    const identifier = document.getElementById("Identifier");
    if (!notice || !countdown || !identifier) return;

    const lockedIdentifier = identifier.value.trim().toUpperCase();
    const duration = Number(notice.dataset.remainingSeconds) * 1000;
    const startedAt = performance.now();
    const render = () => {
        // This is presentation only; every login attempt is checked against SQL Server.
        const remaining = Math.max(0, Math.ceil((duration - (performance.now() - startedAt)) / 1000));
        if (remaining === 0) {
            notice.textContent = "Đã hết thời gian khóa. Bạn có thể thử đăng nhập lại.";
            notice.classList.replace("alert-warning", "alert-info");
            clearInterval(timer);
            return;
        }
        countdown.textContent = `${String(Math.floor(remaining / 60)).padStart(2, "0")}:${String(remaining % 60).padStart(2, "0")}`;
    };
    const timer = setInterval(render, 1000);
    document.addEventListener("visibilitychange", render);
    identifier.addEventListener("input", () => {
        notice.hidden = identifier.value.trim().toUpperCase() !== lockedIdentifier;
    });
    render();
})();
