// ---------- Nav bar ----------
function renderNav(activePage) {
    const root = document.getElementById("nav-root");
    if (!root) return;

    const base = Auth.pageBase();
    const customer = Auth.currentCustomer();
    const loggedIn = Auth.isLoggedIn();
    const isAdmin = Auth.isAdmin();

    const links = [];
    if (isAdmin) {
        links.push({ href: `${base}admin/index.html`, label: "Dashboard", key: "admin" });
        links.push({ href: `${base}admin/venues.html`, label: "Venues", key: "venues" });
        links.push({ href: `${base}admin/categories.html`, label: "Categories", key: "categories" });
        links.push({ href: `${base}admin/events.html`, label: "Events", key: "admin-events" });
        links.push({ href: `${base}index.html`, label: "Public site", key: "events" });
    } else {
        links.push({ href: `${base}index.html`, label: "Events", key: "events" });
        if (loggedIn) {
            links.push({ href: `${base}my-bookings.html`, label: "My tickets", key: "bookings" });
            links.push({ href: `${base}dashboard.html`, label: "Dashboard", key: "dashboard" });
        }
    }

    const linksHtml = links
        .map(
            (l) =>
                `<a class="nav-link${l.key === activePage ? " is-active" : ""}" href="${l.href}">${l.label}</a>`
        )
        .join("");

    const rightHtml = loggedIn
        ? `<span class="nav-user">${escapeHtml(customer?.fullName ?? "")}${isAdmin ? " · Admin" : ""}</span>
       <button class="btn btn-ghost btn-sm" id="nav-logout-btn">Log out</button>`
        : `<a class="btn btn-ghost btn-sm" href="${base}login.html">Log in</a>
       <a class="btn btn-primary btn-sm" href="${base}register.html">Get tickets</a>`;

    root.innerHTML = `
    <nav class="nav">
      <a href="${isAdmin ? `${base}admin/index.html` : `${base}index.html`}" class="nav-brand">
        <span class="nav-brand-mark" aria-hidden="true">&#9679;&#9673;</span>
        EventPark<span class="nav-brand-accent">.</span>
      </a>
      <div class="nav-links">${linksHtml}</div>
      <div class="nav-right">${rightHtml}</div>
    </nav>
  `;

    const logoutBtn = document.getElementById("nav-logout-btn");
    if (logoutBtn) logoutBtn.addEventListener("click", () => Auth.logout());
}

// ---------- Toasts ----------
function toast(message, kind = "info") {
    let host = document.getElementById("toast-host");
    if (!host) {
        host = document.createElement("div");
        host.id = "toast-host";
        host.className = "toast-host";
        document.body.appendChild(host);
    }
    const el = document.createElement("div");
    el.className = `toast toast-${kind}`;
    el.textContent = message;
    host.appendChild(el);
    requestAnimationFrame(() => el.classList.add("is-visible"));
    setTimeout(() => {
        el.classList.remove("is-visible");
        setTimeout(() => el.remove(), 250);
    }, 4000);
}

function errorMessage(err) {
    return err?.message || "Something went wrong. Please try again.";
}

// ---------- Formatting ----------
function formatMoney(amount) {
    return `Rs. ${Number(amount).toLocaleString("en-LK", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function formatDate(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString("en-US", { weekday: "short", year: "numeric", month: "short", day: "numeric" });
}

function formatTime(timeStr) {
    // timeStr like "18:00:00"
    const [h, m] = timeStr.split(":").map(Number);
    const period = h >= 12 ? "PM" : "AM";
    const hour12 = h % 12 === 0 ? 12 : h % 12;
    return `${hour12}:${String(m).padStart(2, "0")} ${period}`;
}

function escapeHtml(str) {
    const div = document.createElement("div");
    div.textContent = str ?? "";
    return div.innerHTML;
}

// ---------- Confirm dialog (native, but centralised so it's easy to swap later) ----------
function confirmAction(message) {
    return window.confirm(message);
}
