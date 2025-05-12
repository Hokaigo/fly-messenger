export function scrollToBottom(selector = ".chat-messages") {
    const container = document.querySelector(selector);
    if (!container) return;
    container.scrollTop = container.scrollHeight;
}

export function showError(message, { selector = "#clientError", timeout = 5000 } = {}) {
    const el = document.querySelector(selector);
    if (!el) return;
    el.textContent = message;
    el.classList.remove("d-none");
    setTimeout(() => el.classList.add("d-none"), timeout);
}
