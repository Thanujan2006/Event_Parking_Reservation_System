renderNav();

const resetToken = new URLSearchParams(window.location.search).get("token");
const alertHost = document.getElementById("form-alert");

if (!resetToken) {
    alertHost.innerHTML = `<div class="alert alert-error">This link is missing its reset token. Please use the link exactly as it appeared in your email.</div>`;
    document.getElementById("reset-form").style.display = "none";
}

document.getElementById("reset-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const submitBtn = document.getElementById("submit-btn");
    const newPassword = document.getElementById("password").value;
    alertHost.innerHTML = "";

    submitBtn.disabled = true;
    submitBtn.textContent = "Resetting…";

    try {
        const result = await Api.post("/auth/reset-password", { token: resetToken, newPassword });
        alertHost.innerHTML = `<div class="alert alert-info">${escapeHtml(result.message)} <a href="login.html">Log in now</a>.</div>`;
        document.getElementById("reset-form").style.display = "none";
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
        submitBtn.disabled = false;
        submitBtn.textContent = "Reset password";
    }
});
