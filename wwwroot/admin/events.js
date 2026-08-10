Auth.requireAdmin();
renderNav("admin-events");

async function loadLookups() {
    const [venues, categories] = await Promise.all([Api.get("/venues"), Api.get("/categories")]);
    const venueSelect = document.getElementById("venueId");
    const categorySelect = document.getElementById("categoryId");

    venueSelect.innerHTML = venues.length
        ? venues.map((v) => `<option value="${v.venueId}">${escapeHtml(v.name)} (${v.totalCapacity})</option>`).join("")
        : `<option value="">No venues — add one first</option>`;

    categorySelect.innerHTML = categories.length
        ? categories.map((c) => `<option value="${c.categoryId}">${escapeHtml(c.name)}</option>`).join("")
        : `<option value="">No categories — add one first</option>`;

    venueSelect.disabled = venues.length === 0;
    categorySelect.disabled = categories.length === 0;
    document.getElementById("submit-btn").disabled = venues.length === 0 || categories.length === 0;
}

async function loadEvents() {
    const loading = document.getElementById("list-loading");
    const empty = document.getElementById("list-empty");
    const host = document.getElementById("list-host");
    loading.style.display = "block";
    empty.style.display = "none";
    host.innerHTML = "";

    try {
        const data = await Api.get("/events?page=1&pageSize=100");
        const events = Array.isArray(data) ? data : data.items || [];
        loading.style.display = "none";
        if (!events.length) {
            empty.style.display = "block";
            return;
        }
        host.innerHTML = `
      <table class="data-table">
        <thead>
          <tr><th>Name</th><th>Venue</th><th>Category</th><th>When</th><th>Price</th><th></th></tr>
        </thead>
        <tbody>
          ${events
              .map(
                  (ev) => `<tr>
              <td>${escapeHtml(ev.name)}</td>
              <td>${escapeHtml(ev.venueName)}</td>
              <td>${escapeHtml(ev.categoryName)}</td>
              <td>${formatDate(ev.eventDate || ev.eventDateTime)} ${ev.startTime ? formatTime(ev.startTime) : ""}</td>
              <td>${formatMoney(ev.ticketPrice)}</td>
              <td><button type="button" class="btn btn-ghost btn-sm" data-delete="${ev.eventId}">Delete</button></td>
            </tr>`
              )
              .join("")}
        </tbody>
      </table>`;

        host.querySelectorAll("[data-delete]").forEach((btn) => {
            btn.addEventListener("click", async () => {
                const id = Number(btn.dataset.delete);
                if (!confirmAction("Delete this event?")) return;
                try {
                    await Api.del(`/events/${id}`);
                    toast("Event deleted.", "success");
                    loadEvents();
                } catch (err) {
                    toast(errorMessage(err), "error");
                }
            });
        });
    } catch (err) {
        loading.style.display = "none";
        toast(errorMessage(err), "error");
    }
}

document.getElementById("event-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertHost = document.getElementById("form-alert");
    const submitBtn = document.getElementById("submit-btn");
    alertHost.innerHTML = "";

    const localValue = document.getElementById("eventDateTime").value;
    // Send as local ISO-like string; API binds to DateTime
    const eventDateTime = localValue.length === 16 ? `${localValue}:00` : localValue;

    const payload = {
        name: document.getElementById("name").value.trim(),
        venueId: Number(document.getElementById("venueId").value),
        categoryId: Number(document.getElementById("categoryId").value),
        eventDateTime,
        durationMinutes: Number(document.getElementById("durationMinutes").value),
        ticketPrice: Number(document.getElementById("ticketPrice").value),
        capacity: Number(document.getElementById("capacity").value),
    };

    submitBtn.disabled = true;
    try {
        await Api.post("/events", payload);
        toast("Event added.", "success");
        e.target.reset();
        document.getElementById("durationMinutes").value = "120";
        await loadLookups();
        loadEvents();
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
        submitBtn.disabled = false;
    }
});

(async () => {
    try {
        await loadLookups();
        await loadEvents();
    } catch (err) {
        toast(errorMessage(err), "error");
    }
})();
