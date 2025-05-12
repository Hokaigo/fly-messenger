(async function initUserState() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/userState")
        .build();

    connection.on("UserStatusChanged", (userId, isOnline) => {
        document.querySelectorAll(`[data-user-id="${userId}"]`).forEach(el => {
            const dot = el.id === "global-online-indicator" ? el : el.querySelector(".participant-status");
            if (!dot) return;
            dot.style.background = isOnline ? "limegreen" : "gray";
            dot.title = isOnline ? "Online" : "Offline";
        });
    });

    try {
        await connection.start();
        console.log("UserStateHub connected");
    } catch (err) {
        console.error("UserStateHub error:", err.toString());
    }
})();
