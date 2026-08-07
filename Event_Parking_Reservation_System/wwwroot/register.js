renderNav();

document.getElementById("register-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertHost = document.getElementById("form-alert");
    const submitBtn = document.getElementById("submit-btn");
    alertHost.innerHTML = "";

    const payload = {
        fullName: document.getElementById("fullName").value.trim(),
        email: document.getElementById("email").value.trim(),
        phone: document.getElementById("phone").value.trim(),
        password: document.getElementById("password").value,
    };

    submitBtn.disabled = true;
    submitBtn.textContent = "Creating account…";

    try {
        await Api.post("/customers/register", payload);
        document.getElementById("register-form").style.display = "none";
        alertHost.innerHTML = `<div class="alert alert-info">
      Account created! We've sent a verification link to <strong>${escapeHtml(payload.email)}</strong>.
      Click it to activate your account, then <a href="login.html">log in</a>.
    </div>`;
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
        submitBtn.disabled = false;
        submitBtn.textContent = "Create account";
    }
});
