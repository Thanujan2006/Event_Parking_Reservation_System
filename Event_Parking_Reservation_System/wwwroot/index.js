renderNav("events");

async function loadFilterOptions() {
    try {
        const [venues, categories] = await Promise.all([Api.get("/venues"), Api.get("/categories")]);
        const venueSelect = document.getElementById("f-venue");
        venues.forEach((v) => venueSelect.insertAdjacentHTML("beforeend", `<option value="${v.venueId}">${escapeHtml(v.name)}</option>`));
        const categorySelect = document.getElementById("f-category");
        categories.forEach((c) => categorySelect.insertAdjacentHTML("beforeend", `<option value="${c.categoryId}">${escapeHtml(c.name)}</option>`));
    } catch (err) {
        // Filters are a nice-to-have; don't block the page if they fail to load.
        console.error(err);
    }
}

function ticketCardHtml(ev) {
    const soldOut = ev.seatsAvailable <= 0;
    return `
    <a class="ticket" href="event.html?id=${ev.eventId}" style="text-decoration:none;color:inherit;">
      <div class="ticket-body">
        <span class="ticket-eyebrow">${escapeHtml(ev.categoryName)}</span>
        <h3 class="ticket-title">${escapeHtml(ev.name)}</h3>
        <div class="ticket-meta">
          <span>&#128197; ${formatDate(ev.eventDate)}</span>
          <span>&#128337; ${formatTime(ev.startTime)}</span>
          <span>&#128205; ${escapeHtml(ev.venueName)}</span>
        </div>
        <span class="badge ${soldOut ? "badge-cancelled" : "badge-confirmed"}">${soldOut ? "Sold out" : `${ev.seatsAvailable} seats left`}</span>
      </div>
      <div class="ticket-stub">
        <div class="ticket-divider"></div>
        <span class="ticket-eyebrow">From</span>
        <span class="ticket-price">${formatMoney(ev.ticketPrice)}</span>
      </div>
    </a>
  `;
}

async function search() {
    const grid = document.getElementById("events-grid");
    const loading = document.getElementById("events-loading");
    const empty = document.getElementById("events-empty");
    grid.innerHTML = "";
    empty.style.display = "none";
    loading.style.display = "block";

    const params = new URLSearchParams();
    const name = document.getElementById("f-name").value.trim();
    const date = document.getElementById("f-date").value;
    const venueId = document.getElementById("f-venue").value;
    const categoryId = document.getElementById("f-category").value;
    if (name) params.set("name", name);
    if (date) params.set("date", date);
    if (venueId) params.set("venueId", venueId);
    if (categoryId) params.set("categoryId", categoryId);

    try {
        const events = await Api.get(`/events?${params.toString()}`);
        loading.style.display = "none";
        if (events.length === 0) {
            empty.style.display = "block";
            return;
        }
        grid.innerHTML = events.map(ticketCardHtml).join("");
    } catch (err) {
        loading.style.display = "none";
        toast(errorMessage(err), "error");
    }
}

document.getElementById("search-btn").addEventListener("click", search);
document.getElementById("clear-btn").addEventListener("click", () => {
    document.getElementById("f-name").value = "";
    document.getElementById("f-date").value = "";
    document.getElementById("f-venue").value = "";
    document.getElementById("f-category").value = "";
    search();
});

loadFilterOptions();
search();
