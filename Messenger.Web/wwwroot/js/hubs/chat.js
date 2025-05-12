import { postFormData, postUrlEncoded } from "../modules/api.js";

export default async function initChat({ scrollToBottom, showError, createMessageElement }) {
    const form = document.getElementById("messageForm");
    if (!form) return;

    const textInput = form.querySelector("input[name='text']");
    const fileInput = form.querySelector("input[type='file']");
    const fileLabel = form.querySelector("label[for='fileInput']");
    const sendBtn = form.querySelector("button[type='submit']");
    const currentUserId = window.currentUserId;

    let formMode = "normal";
    let editingMessageId = null;
    let deletingMessageId = null;
    let cancelBtn = null;
    let confirmBtn = null;
    let menuEl = null;
    function getCsrfToken() {
        const tokenInput = form.querySelector("input[name='__RequestVerificationToken']");
        return tokenInput ? tokenInput.value : "";
    }

    function updateFormVisibility({ text, file, send }) {
        textInput.disabled = !text;
        textInput.style.display = text ? "" : "none";
        fileInput.disabled = !file;
        fileInput.style.display = file ? "" : "none";
        fileLabel.style.display = file ? "" : "none";
        sendBtn.style.display = send ? "" : "none";
    }

    function removeExtraButtons() {
        if (cancelBtn) { cancelBtn.remove(); cancelBtn = null; }
        if (confirmBtn) { confirmBtn.remove(); confirmBtn = null; }
    }

    function resetFormMode() {
        formMode = "normal";
        editingMessageId = null;
        deletingMessageId = null;
        form.classList.remove("edit-mode", "delete-mode");
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
        formMode = "edit";
        editingMessageId = messageId;
        form.classList.add("edit-mode");

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
        formMode = "delete";
        deletingMessageId = messageId;
        form.classList.add("delete-mode");

        updateFormVisibility({ text: false, file: false, send: false });

        confirmBtn = createButton({
            text: "Confirm Delete",
            className: "btn btn-danger",
            onClick: async () => {
                const token = getCsrfToken();
                await fetch("/Chats/DeleteMessage", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "RequestVerificationToken": token
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

    async function handleSubmit(e) {
        e.preventDefault();

        if (formMode === "edit") {
            const newText = textInput.value.trim();
            if (!newText) return showError("Text cannot be empty");
            await postUrlEncoded("/Chats/EditMessage", {
                messageId: editingMessageId,
                newText
            });
            resetFormMode();
            resetFormInputs();
            return;
        }

        const text = textInput.value.trim();
        const hasFile = fileInput.files.length > 0;
        if (!text && !hasFile) return showError("Text cannot be empty");

        const fd = new FormData(form);
        if (!text) fd.delete("text");

        const res = await postFormData(form.action, fd);
        if (!res.ok) {
            const ct = res.headers.get("content-type") || "";
            const err = ct.includes("json") ? (await res.json()).error : await res.text();
            return showError(err);
        }

        resetFormInputs();
        scrollToBottom();
    }

    const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/chat").build();

    connection.on("ReceiveMessage", msg => {
        const el = createMessageElement(msg, currentUserId, enterContextMenu);
        document.querySelector(".chat-messages").appendChild(el);
        scrollToBottom();
    });

    connection.on("MessageEdited", dto => {
        const wrapper = document.querySelector(`[data-message-id="${dto.id}"]`);
        if (!wrapper) return;
        const bubble = wrapper.querySelector(".message-bubble");
        let txt = bubble.querySelector(".message-text");
        if (txt) {
            txt.textContent = dto.text;
        } else {
            txt = document.createElement("div");
            txt.className = "message-text";
            txt.textContent = dto.text;
            bubble.insertBefore(txt, bubble.querySelector(".message-time"));
        }
        wrapper.classList.add("edited");
    });

    connection.on("MessageDeleted", id => {
        const el = document.querySelector(`[data-message-id="${id}"]`);
        if (el) el.remove();
    });

    await connection.start();
    await connection.invoke("JoinChat", form.chatId.value);

    scrollToBottom();

    document.body.addEventListener("contextmenu", e => {
        const wrapper = e.target.closest(".message-wrapper.own");
        if (!wrapper) return;
        e.preventDefault();
        const id = wrapper.dataset.messageId;
        const text = wrapper.querySelector(".message-text")?.textContent || "";
        enterContextMenu(e.pageX, e.pageY, id, text);
    });

    form.addEventListener("submit", handleSubmit);
    fileInput.addEventListener("change", () => {
        fileLabel.textContent = fileInput.files.length ? "File selected" : "Choose file";
    });

    function enterContextMenu(x, y, id, text) {
        if (menuEl) menuEl.remove();
        menuEl = document.createElement("div");
        menuEl.className = "dropdown-menu show";
        Object.assign(menuEl.style, {
            position: "absolute",
            top: `${y}px`,
            left: `${x}px`,
            background: "#2c2c2c",
            border: "1px solid #444",
            boxShadow: "0 2px 6px rgba(0,0,0,0.5)",
            minWidth: "8rem",
            zIndex: 10000
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
            if (action === "edit") enterEditMode(id, text);
            if (action === "delete") enterDeleteMode(id);
        });
    }
}
