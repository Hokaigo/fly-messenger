export async function postFormData(url, formData) {
    const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
    const token = tokenInput ? tokenInput.value : "";
    const response = await fetch(url, {
        method: "POST",
        credentials: "same-origin",
        headers: { "RequestVerificationToken": token },
        body: formData
    });
    return response;
}

export async function postUrlEncoded(url, data) {
    const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
    const token = tokenInput ? tokenInput.value : "";
    const body = new URLSearchParams(data);
    const response = await fetch(url, {
        method: "POST",
        credentials: "same-origin",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "RequestVerificationToken": token
        },
        body
    });
    return response;
}
