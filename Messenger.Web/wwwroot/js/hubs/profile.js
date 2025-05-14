(async function initProfileHub() {
    const any = document.querySelector("[data-user-id]");
    if (!any) return;

    const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/profile").build();

    connection.on("ProfileUpdated", dto => {
        const userId = dto.userId;
        const newAvatar = dto.profilePicUrl;
        const newInitial = (dto.userName || "?")[0].toUpperCase();

        document.querySelectorAll(`[data-user-id="${userId}"]`).forEach(el => {
            if (el.tagName === "IMG") {
                if (newAvatar) {
                    el.src = `${newAvatar}?v=${Date.now()}`;
                } else {
                    const ph = document.createElement("div");
                    ph.className = "avatar-placeholder rounded-circle";
                    ph.style.cssText = el.getAttribute("style") || "";
                    ph.textContent = newInitial;
                    ph.setAttribute("data-user-id", userId);
                    el.replaceWith(ph);
                }
            } else if (el.classList.contains("avatar-placeholder")) {
                if (newAvatar) {
                    const img = document.createElement("img");
                    img.className = "rounded-circle";
                    img.style.cssText = el.getAttribute("style") || "";
                    img.src = `${newAvatar}?v=${Date.now()}`;
                    img.setAttribute("data-user-id", userId);
                    el.replaceWith(img);
                } else {
                    el.textContent = newInitial;
                }
            }
        });

        document.querySelectorAll(`[data-user-id="${userId}"][data-user-field]`).forEach(el => {
            const field = el.getAttribute("data-user-field");
            if (field && dto[field] !== undefined) {
                el.textContent = dto[field];
            }
        });
    });

    try {
        await connection.start();
        console.log("ProfileHub connected");
        const profileUserId = any.getAttribute("data-user-id");
        if (profileUserId) {
            await connection.invoke("JoinProfileGroup", profileUserId);
        }
    } catch (err) {
        console.error("ProfileHub error:", err.toString());
    }
})();
