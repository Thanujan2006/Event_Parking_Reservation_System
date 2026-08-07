renderNav();

async function run() {
    const token = new URLSearchParams(window.location.search).get("token");
    const icon = document.getElementById("status-icon");
    const title = document.getElementById("status-title");
    const message = document.getElementById("status-message");
    const loginLink = document.getElementById("login-link");

    if (!token) {
        icon.textContent = "⚠️";
        title.textContent = "Missing verification token";
        message.textContent = "This link looks incomplete. Please use the link exactly as it appeared in your email.";
        return;
    }

    try {
        const result = await Api.get(`/auth/verify-email?token=${encodeURIComponent(token)}`);
        icon.textContent = "✅";
        title.textContent = "Email verified!";
        message.textContent = result.message || "You can now log in.";
        loginLink.style.display = "inline-flex";
    } catch (err) {
        icon.textContent = "⚠️";
        title.textContent = "Verification failed";
        message.textContent = errorMessage(err);
    }
}

run();
