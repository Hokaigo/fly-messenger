import { createMessageElement } from './messageRenderer.js';

const currentUserId = window.currentUserId;
let connection;
let menuEl = null;

let formMode = 'normal';
let editingMessageId = null;
let deletingMessageId = null;

const form = document.getElementById("messageForm");
const textInput = form.querySelector("input[name='text']");
const fileInput = form.querySelector("input[type='file']");
const fileLabel = form.querySelector("label[for='fileInput']");
const sendBtn = form.querySelector("button[type='submit']");
let cancelBtn = null;
let confirmBtn = null;

function getCsrfToken() {
    const tokenInput = form.querySelector("input[name='__RequestVerificationToken']");
    return tokenInput ? tokenInput.value : "";
}

function updateFormVisibility({ text, file, send }) {
    textInput.disabled = !text;
    textInput.style.display = text ? '' : 'none';
    fileInput.disabled = !file;
    fileInput.style.display = file ? '' : 'none';
    fileLabel.style.display = file ? '' : 'none';
    sendBtn.style.display = send ? '' : 'none';
}

function removeExtraButtons() {
    if (cancelBtn) { cancelBtn.remove(); cancelBtn = null; }
    if (confirmBtn) { confirmBtn.remove(); confirmBtn = null; }
}

function resetFormMode() {
    formMode = 'normal';
    editingMessageId = null;
    deletingMessageId = null;
    form.classList.remove('edit-mode', 'delete-mode');
    updateFormVisibility({ text: true, file: true, send: true });
    sendBtn.textContent = "Send";
    removeExtraButtons();
}

function resetFormInputs() {
    form.reset();
    fileLabel.textContent = "Choose file";
    fileLabel.classList.remove("selected");
}

function createButton({ text, className, onClick }) {
    const btn = document.createElement("button");
    btn.type = "button";
    btn.textContent = text;
    btn.className = className;
    btn.addEventListener("click", e => {
        e.preventDefault();
        onClick();
    });
    return btn;
}

function enterEditMode(messageId, currentText) {
    resetFormMode();
    formMode = 'edit';
    editingMessageId = messageId;
    form.classList.add('edit-mode');

    textInput.value = currentText;
    textInput.focus();

    updateFormVisibility({ text: true, file: false, send: true });
    sendBtn.textContent = "Save";

    cancelBtn = createButton({
        text: "Cancel",
        className: "btn btn-secondary ms-2",
        onClick: () => {
            resetFormMode();
            resetFormInputs();
        }
    });
    sendBtn.insertAdjacentElement("afterend", cancelBtn);
}

function enterDeleteMode(messageId) {
    resetFormMode();
    formMode = 'delete';
    deletingMessageId = messageId;
    form.classList.add('delete-mode');

    updateFormVisibility({ text: false, file: false, send: false });

    confirmBtn = createButton({
        text: "Confirm Delete",
        className: "btn btn-danger",
        onClick: async () => {
            const token = getCsrfToken();
            await fetch('/Chats/DeleteMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams({ messageId: deletingMessageId })
            });
            resetFormMode();
            resetFormInputs();
        }
    });

    cancelBtn = createButton({
        text: "Cancel",
        className: "btn btn-secondary ms-2",
        onClick: () => {
            resetFormMode();
            resetFormInputs();
        }
    });

    sendBtn.insertAdjacentElement("beforebegin", confirmBtn);
    confirmBtn.insertAdjacentElement("afterend", cancelBtn);
}

async function initChat() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/chat")
        .build();

    connection.on("ReceiveMessage", msg => {
        const el = createMessageElement(msg, currentUserId, showContextMenu);
        document.querySelector(".chat-messages").appendChild(el);
        scrollToBottom();
    });

    connection.on("MessageEdited", dto => {
        const wrapper = document.querySelector(`[data-message-id="${dto.id}"]`);
        if (!wrapper) return;
        const bubble = wrapper.querySelector(".message-bubble");

        let txtDiv = bubble.querySelector(".message-text");
        if (txtDiv) {
            txtDiv.textContent = dto.text;
        } else {
            txtDiv = document.createElement("div");
            txtDiv.className = "message-text";
            txtDiv.textContent = dto.text;
            const timeDiv = bubble.querySelector(".message-time");
            bubble.insertBefore(txtDiv, timeDiv);
        }

        wrapper.classList.add("edited");
    });

    connection.on("MessageDeleted", id => {
        const el = document.querySelector(`[data-message-id="${id}"]`);
        if (el) el.remove();
    });

    await connection.start();
    const chatId = document.getElementById("chatId").value;
    await connection.invoke("JoinChat", chatId);
    scrollToBottom();

    document.body.addEventListener("contextmenu", e => {
        const wrapper = e.target.closest(".message-wrapper.own");
        if (!wrapper) return;
        e.preventDefault();
        const id = wrapper.dataset.messageId;
        const text = wrapper.querySelector(".message-text")?.textContent || "";
        showContextMenu(e.pageX, e.pageY, id, text);
    });

    form.addEventListener("submit", async e => {
        e.preventDefault();
        const token = getCsrfToken();

        if (formMode === 'edit') {
            const newText = textInput.value.trim();
            if (!newText) {
                return showError("Text cannot be empty");
            }

            await fetch('/Chats/EditMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams({
                    messageId: editingMessageId,
                    newText
                })
            });

            resetFormMode();
            resetFormInputs();
        }
        else if (formMode === 'normal') {
            const text = textInput.value.trim();
            const hasFile = fileInput.files.length > 0;
            if (!text && !hasFile) {
                return showError("Text cannot be empty");
            }

            const fd = new FormData(form);
            if (!text) {
                fd.delete("text");
            }

            const res = await fetch(form.action, {
                method: "POST",
                body: fd,
                credentials: "same-origin",
                headers: {
                    "RequestVerificationToken": token
                }
            });

            if (!res.ok) {
                let errMsg = `Error ${res.status}`;
                const contentType = res.headers.get("content-type") || "";

                if (contentType.includes("application/json")) {
                    const data = await res.json();
                    errMsg = data.error || errMsg;
                } else {
                    errMsg = await res.text();
                }
                showError(errMsg);
            } else {
                resetFormInputs();
            }

            scrollToBottom();
        }
    });


    fileInput.addEventListener("change", () => {
        if (fileInput.files.length > 0) {
            fileLabel.textContent = "File selected";
            fileLabel.classList.add("selected");
        } else {
            fileLabel.textContent = "Choose file";
            fileLabel.classList.remove("selected");
        }
    });
}

window.addEventListener("DOMContentLoaded", initChat);

function scrollToBottom() {
    const c = document.querySelector(".chat-messages");
    if (c) c.scrollTop = c.scrollHeight;
}

function showError(msg) {
    const b = document.getElementById("clientError");
    if (!b) return;
    b.textContent = msg;
    b.classList.remove("d-none");
    setTimeout(() => b.classList.add("d-none"), 5000);
}

function showContextMenu(x, y, messageId, currentText) {
    if (menuEl) menuEl.remove();
    menuEl = document.createElement("div");
    menuEl.className = "dropdown-menu show";
    Object.assign(menuEl.style, {
        position: "absolute",
        top: `${y}px`,
        left: `${x}px`,
        minWidth: "8rem",
        background: "#2c2c2c",
        border: "1px solid #444",
        boxShadow: "0 2px 6px rgba(0,0,0,0.5)"
    });
    menuEl.innerHTML = `
        <a href="#" class="dropdown-item text-light" data-action="edit">Edit</a>
        <a href="#" class="dropdown-item text-light" data-action="delete">Delete</a>
    `;
    document.body.append(menuEl);

    menuEl.addEventListener("click", ev => {
        ev.preventDefault();
        const action = ev.target.dataset.action;
        menuEl.remove();
        menuEl = null;
        if (action === "edit") enterEditMode(messageId, currentText);
        if (action === "delete") enterDeleteMode(messageId);
    });
}
