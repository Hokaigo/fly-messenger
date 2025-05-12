import "./modules/api.js";
import { scrollToBottom, showError } from "./modules/domUtilities.js";
import { createMessageElement } from "./modules/messageRenderer.js";

import "./hubs/userState.js";  

if (document.querySelector(".chat-list")) {
    import("./hubs/chatList.js").catch(console.error);
}

if (document.querySelector("[data-user-id]")) {
    import("./hubs/profile.js").catch(console.error);
}

if (document.getElementById("messageForm")) {
    import("./hubs/chat.js").then(({ default: initChat }) => initChat({ scrollToBottom, showError, createMessageElement })).catch(console.error);
}

