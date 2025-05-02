(function () {
    const listConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/chatList")
        .build();

    listConnection.on("ChatListUpdated", summary => {
        const list = document.querySelector(".chat-list");
        if (!list) return;
        const selector = `[data-chat-id="${summary.chatId}"]`;
        const existing = list.querySelector(selector);
        const time = summary.lastMessageTime
            ? new Date(summary.lastMessageTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
            : '';
        const li = document.createElement("li");
        li.className = "list-group-item p-0";
        li.dataset.chatId = summary.chatId;
        li.innerHTML = `
            <a href="/Chats/Open?chatId=${summary.chatId}" class="chat-link-custom">
                <div class="chat-header">
                    <span class="chat-username">${summary.otherUserName}</span>
                    <span class="chat-time">${time}</span>
                </div>
                ${summary.lastMessage ? `<div class="chat-preview">${summary.lastMessage}</div>` : ''}
            </a>`;
        if (existing) existing.remove();
        list.prepend(li);
    });

    listConnection.start().catch(err => console.error("SignalR connection error:", err.toString()));
})();
