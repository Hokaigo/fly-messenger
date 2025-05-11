const statusConn = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/userState")
    .build();

statusConn.on("UserStatusChanged", (userId, isOnline) => {
    document.querySelectorAll(`.participant[data-user-id="${userId}"]`)
        .forEach(li => {
            const dot = li.querySelector(".participant-status");
            if (!dot) return;
            dot.style.background = isOnline ? "limegreen" : "gray";
            dot.title = isOnline ? "Online" : "Offline";
        });
});

statusConn
    .start()
    .catch(err => console.error("SignalR connection error:", err));
