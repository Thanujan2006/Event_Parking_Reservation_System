renderNav();

document.getElementById("forgot-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertHost = document.getElementById("form-alert");
    const submitBtn = document.getElementById("submit-btn");
    const email = document.getElementById("email").value.trim();

    submitBtn.disabled = true;
    submitBtn.textContent = "Sending…";

    try {
        const result = await Api.post("/auth/forgot-password", { email });
        // Always a generic success message — the API never reveals whether the email exists.
        alertHost.innerHTML = `<div class="alert alert-info">${escapeHtml(result.message)}</div>`;
        document.getElementById("forgot-form").style.display = "none";
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
        submitBtn.disabled = false;
        submitBtn.textContent = "Send reset link";
    }
});
