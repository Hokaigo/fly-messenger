export function createMessageElement(msg, currentUserId, showContextMenu) {
    const { id, userId, userName, text, fileUrl, fileName, fileType, dateSent, type } = msg;

    const isOwn = userId === currentUserId;
    const isFile = type === 1 && fileUrl;
    const isImage = isFile && fileType.startsWith("image/");
    const isVideo = isFile && fileType.startsWith("video/");

    const wrapper = document.createElement("div");
    wrapper.classList.add("message-wrapper", isOwn ? "own" : "other");
    wrapper.dataset.messageId = id;

    if (isOwn && typeof showContextMenu === 'function') {
        wrapper.addEventListener('contextmenu', e => {
            e.preventDefault();
            showContextMenu(e.pageX, e.pageY, id, text || "");
        });
    }

    const bubble = document.createElement("div");
    const bubbleClasses = ["message-bubble", (isImage || isVideo) ? "file-image" : null].filter(Boolean).join(" ");
    bubble.className = bubbleClasses;

    const header = document.createElement("div");
    header.className = "message-username";
    const link = document.createElement("a");
    link.href = `/Profile/Details/${userId}`;
    link.textContent = userName;
    link.style.cssText = "color:inherit;text-decoration:none;";
    header.appendChild(link);
    bubble.appendChild(header);

    if (isFile) {
        if (isImage || isVideo) {
            const media = document.createElement(isImage ? "img" : "video");
            media.classList.add("img-fluid", "mb-2");
            if (isVideo) media.setAttribute('controls', '');
            media.src = fileUrl;
            if (!isImage) {
                const source = document.createElement("source");
                source.src = fileUrl;
                source.type = fileType;
                media.appendChild(source);
            }
            bubble.appendChild(media);
        } else {
            const info = document.createElement("div");
            info.className = "file-info mb-1";
            info.textContent = fileName;
            bubble.appendChild(info);
        }

        const dl = document.createElement("a");
        dl.href = fileUrl;
        dl.download = fileName;
        dl.className = "btn btn-sm btn-outline-light mb-2";
        dl.textContent = "Download";
        bubble.appendChild(dl);

        if (text) {
            const txtDiv = document.createElement("div");
            txtDiv.className = "message-text";
            txtDiv.textContent = text;
            bubble.appendChild(txtDiv);
        }

    } else {
        const txtDiv = document.createElement("div");
        txtDiv.className = "message-text";
        txtDiv.textContent = text || "";
        bubble.appendChild(txtDiv);
    }

    const timeDiv = document.createElement("div");
    timeDiv.className = "message-time";
    timeDiv.textContent = new Date(dateSent).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    bubble.appendChild(timeDiv);

    wrapper.appendChild(bubble);
    return wrapper;
}
