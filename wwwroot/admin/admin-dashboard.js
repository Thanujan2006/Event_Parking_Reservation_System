Auth.requireAdmin();
renderNav("admin");

async function loadStats() {
    try {
        const [venues, categories, eventsPage] = await Promise.all([
            Api.get("/venues"),
            Api.get("/categories"),
            Api.get("/events?page=1&pageSize=1"),
        ]);
        document.getElementById("stat-venues").textContent = Array.isArray(venues) ? venues.length : 0;
        document.getElementById("stat-categories").textContent = Array.isArray(categories) ? categories.length : 0;
        const total = eventsPage?.totalCount ?? (Array.isArray(eventsPage) ? eventsPage.length : (eventsPage?.items?.length ?? 0));
        document.getElementById("stat-events").textContent = total;
    } catch (err) {
        toast(errorMessage(err), "error");
    }
}

loadStats();
