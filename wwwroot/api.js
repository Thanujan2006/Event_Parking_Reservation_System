// Thin wrapper around fetch(). Every API call in the app goes through this.
const Api = (() => {
    function authHeaders() {
        const token = localStorage.getItem("eprs_token");
        return token ? { Authorization: `Bearer ${token}` } : {};
    }

    async function request(method, path, body) {
        const res = await fetch(`${window.API_BASE_URL}${path}`, {
            method,
            headers: {
                "Content-Type": "application/json",
                ...authHeaders(),
            },
            body: body !== undefined ? JSON.stringify(body) : undefined,
        });

        // 204 No Content, or empty body
        const text = await res.text();
        const data = text ? JSON.parse(text) : null;

        if (!res.ok) {
            const message = (data && data.message) || `Request failed (${res.status})`;
            const err = new Error(message);
            err.status = res.status;
            err.error = data && data.error;
            err.data = data;
            throw err;
        }
        return data;
    }

    return {
        get: (path) => request("GET", path),
        post: (path, body) => request("POST", path, body ?? {}),
        put: (path, body) => request("PUT", path, body ?? {}),
        del: (path) => request("DELETE", path),
    };
})();
