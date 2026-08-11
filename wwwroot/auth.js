const Auth = (() => {
    function pageBase() {
        return location.pathname.includes("/admin/") ? "../" : "";
    }

    function saveSession(token, customer) {
        localStorage.setItem("eprs_token", token);
        localStorage.setItem("eprs_customer", JSON.stringify(customer));
    }

    function clearSession() {
        localStorage.removeItem("eprs_token");
        localStorage.removeItem("eprs_customer");
    }

    function currentCustomer() {
        const raw = localStorage.getItem("eprs_customer");
        return raw ? JSON.parse(raw) : null;
    }

    function isLoggedIn() {
        return !!localStorage.getItem("eprs_token");
    }

    function isAdmin() {
        const c = currentCustomer();
        return !!c && (c.role === "Admin" || c.role === "Administrator");
    }

    // Redirects to login.html if not authenticated. Call at the top of any protected page.
    function requireLogin() {
        if (!isLoggedIn()) {
            const redirect = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = `${pageBase()}login.html?redirect=${redirect}`;
        }
    }

    function requireAdmin() {
        requireLogin();
        if (!isAdmin()) {
            window.location.href = `${pageBase()}index.html`;
        }
    }

    function logout() {
        clearSession();
        window.location.href = `${pageBase()}index.html`;
    }

    return { pageBase, saveSession, clearSession, currentCustomer, isLoggedIn, isAdmin, requireLogin, requireAdmin, logout };
})();
