renderNav();

const params = new URLSearchParams(window.location.search);
const redirectTo = params.get("redirect");

document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertHost = document.getElementById("form-alert");
    const submitBtn = document.getElementById("submit-btn");
    alertHost.innerHTML = "";

    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    submitBtn.disabled = true;
    submitBtn.textContent = "Logging in…";

    try {
        const result = await Api.post("/auth/login", { email, password });
        Auth.saveSession(result.token, result.customer);
        toast(`Welcome back, ${result.customer.fullName}!`, "success");
        window.location.href = redirectTo || (result.customer.role === "Admin" ? "admin/index.html" : "index.html");
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = "Log in";
    }
});
