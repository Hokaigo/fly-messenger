(async function initChatList() {
    const list = document.querySelector(".chat-list");
    if (!list) return;

    const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/chatList").build();

    connection.on("ChatListUpdated", summary => {
        const selector = `[data-chat-id="${summary.chatId}"]`;
        const existing = list.querySelector(selector);

        const time = summary.lastMessageTime ? new Date(summary.lastMessageTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : "";

        const li = document.createElement("li");
        li.className = "list-group-item p-0";
        li.dataset.chatId = summary.chatId;
        li.innerHTML = `
      <a href="/Chats/Open?chatId=${summary.chatId}" class="chat-link-custom d-block p-3">
        <div class="chat-header d-flex justify-content-between">
          <span class="chat-username fw-bold">${summary.otherUserName}</span>
          <span class="chat-time chat-unread-message small">${time}</span>
        </div>
        ${summary.lastMessage ? `<div class="chat-preview text-truncate chat-unread-message">${summary.lastMessage}</div>` : ""}
      </a>
    `;

        if (existing) existing.remove();
        list.prepend(li);
    });

    try {
        await connection.start();
        console.log("ChatListHub connected");
    } catch (err) {
        console.error("ChatListHub error:", err.toString());
    }
})();
