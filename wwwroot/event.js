renderNav("events");

const eventId = new URLSearchParams(window.location.search).get("id");
let selectedSeats = new Map(); // seatId -> seat object
let selectedParkingSlot = null; // slot object or null
let currentEvent = null;

async function init() {
    if (!eventId) return showNotFound();

    try {
        const [ev, seats, parking] = await Promise.all([
            Api.get(`/events/${eventId}`),
            Api.get(`/events/${eventId}/seats`),
            Api.get(`/events/${eventId}/parking-slots`).catch(() => []), // parking layout may not exist yet
        ]);
        currentEvent = ev;
        renderEventHeader(ev);
        renderSeatMap(seats);
        if (parking.length > 0) {
            document.getElementById("parking-panel").style.display = "block";
            renderParkingMap(parking);
        }
        document.getElementById("loading").style.display = "none";
        document.getElementById("event-content").style.display = "block";
        updateSummary();
    } catch (err) {
        showNotFound();
    }
}

function showNotFound() {
    document.getElementById("loading").style.display = "none";
    document.getElementById("event-empty").style.display = "block";
}

function renderEventHeader(ev) {
    document.getElementById("page-title").textContent = `${ev.name} — EventPark`;
    document.getElementById("ev-category").textContent = ev.categoryName;
    document.getElementById("ev-name").textContent = ev.name;
    document.getElementById("ev-meta").innerHTML = `
    <span>&#128197; ${formatDate(ev.eventDate)}</span>
    <span>&#128337; ${formatTime(ev.startTime)} – ${formatTime(ev.endTime)}</span>
    <span>&#128205; ${escapeHtml(ev.venueName)}</span>
    <span>&#127925; ${ev.seatsAvailable} of ${ev.capacity} seats left</span>
  `;
}

function renderSeatMap(seats) {
    const rows = {};
    seats.forEach((s) => {
        (rows[s.seatRow] ||= []).push(s);
    });

    const host = document.getElementById("seat-map");
    host.innerHTML = Object.keys(rows)
        .sort()
        .map((rowKey) => {
            const rowSeats = rows[rowKey].sort((a, b) => Number(a.seatNumber) - Number(b.seatNumber));
            const seatsHtml = rowSeats
                .map((s) => {
                    const taken = s.status !== "Available";
                    const cls = taken ? "is-taken" : selectedSeats.has(s.seatId) ? "is-selected" : "is-available";
                    return `<button type="button" class="seat ${cls}" data-seat-id="${s.seatId}" ${taken ? "disabled" : ""} title="${escapeHtml(s.seatType || "")} · ${formatMoney(s.price)}">${escapeHtml(s.seatNumber)}</button>`;
                })
                .join("");
            return `<div class="seat-row"><span class="seat-row-label">${escapeHtml(rowKey)}</span><div class="seat-row-seats">${seatsHtml}</div></div>`;
        })
        .join("");

    host.querySelectorAll(".seat:not([disabled])").forEach((btn) => {
        btn.addEventListener("click", () => toggleSeat(btn, seats));
    });
}

function toggleSeat(btn, seats) {
    const seatId = Number(btn.dataset.seatId);
    const seat = seats.find((s) => s.seatId === seatId);
    if (selectedSeats.has(seatId)) {
        selectedSeats.delete(seatId);
        btn.classList.remove("is-selected");
        btn.classList.add("is-available");
    } else {
        selectedSeats.set(seatId, seat);
        btn.classList.remove("is-available");
        btn.classList.add("is-selected");
    }
    updateSummary();
}

function renderParkingMap(slots) {
    const host = document.getElementById("parking-map");
    host.innerHTML = slots
        .map((slot) => {
            const taken = slot.status !== "Available";
            const cls = taken ? "is-taken" : selectedParkingSlot?.slotId === slot.slotId ? "is-selected" : "is-available";
            return `<button type="button" class="parking-slot ${cls}" data-slot-id="${slot.slotId}" ${taken ? "disabled" : ""}>
        <span>${escapeHtml(slot.slotNumber)}</span>
        <span style="opacity:.75">${formatMoney(slot.fee)}</span>
      </button>`;
        })
        .join("");

    host.querySelectorAll(".parking-slot:not([disabled])").forEach((btn) => {
        btn.addEventListener("click", () => toggleParkingSlot(btn, slots));
    });
}

function toggleParkingSlot(btn, slots) {
    const slotId = Number(btn.dataset.slotId);
    const alreadySelected = selectedParkingSlot?.slotId === slotId;

    document.querySelectorAll(".parking-slot.is-selected").forEach((el) => {
        el.classList.remove("is-selected");
        el.classList.add("is-available");
    });

    if (alreadySelected) {
        selectedParkingSlot = null;
    } else {
        selectedParkingSlot = slots.find((s) => s.slotId === slotId);
        btn.classList.remove("is-available");
        btn.classList.add("is-selected");
    }
    updateSummary();
}

function updateSummary() {
    const lines = document.getElementById("summary-lines");
    const totalEl = document.getElementById("summary-total");
    const reserveBtn = document.getElementById("reserve-btn");

    const seatsArr = [...selectedSeats.values()];
    if (seatsArr.length === 0 && !selectedParkingSlot) {
        lines.innerHTML = `<p class="muted" style="font-size:0.88rem;">Pick at least one seat to get started.</p>`;
        totalEl.textContent = formatMoney(0);
        reserveBtn.disabled = true;
        return;
    }

    const seatsTotal = seatsArr.reduce((sum, s) => sum + s.price, 0);
    const parkingFee = selectedParkingSlot?.fee ?? 0;

    lines.innerHTML =
        seatsArr
            .map((s) => `<div class="summary-line"><span>Seat ${escapeHtml(s.seatRow)}${escapeHtml(s.seatNumber)}</span><span>${formatMoney(s.price)}</span></div>`)
            .join("") +
        (selectedParkingSlot
            ? `<div class="summary-line"><span>Parking ${escapeHtml(selectedParkingSlot.slotNumber)}</span><span>${formatMoney(parkingFee)}</span></div>`
            : "");

    totalEl.textContent = formatMoney(seatsTotal + parkingFee);
    reserveBtn.disabled = seatsArr.length === 0;
}

async function reserveAndContinue() {
    if (!Auth.isLoggedIn()) {
        window.location.href = `login.html?redirect=${encodeURIComponent(window.location.pathname + window.location.search)}`;
        return;
    }

    const reserveBtn = document.getElementById("reserve-btn");
    reserveBtn.disabled = true;
    reserveBtn.textContent = "Reserving…";

    let booking;
    try {
        booking = await Api.post("/bookings", { eventId: Number(eventId) });
    } catch (err) {
        toast(errorMessage(err), "error");
        reserveBtn.disabled = false;
        reserveBtn.textContent = "Reserve & continue";
        return;
    }

    try {
        await Api.post(`/bookings/${booking.bookingId}/seats`, { seatIds: [...selectedSeats.keys()] });
        if (selectedParkingSlot) {
            await Api.post(`/bookings/${booking.bookingId}/parking`, { slotId: selectedParkingSlot.slotId });
        }
        window.location.href = `checkout.html?bookingId=${booking.bookingId}`;
    } catch (err) {
        // Someone beat us to a seat/slot — release the empty booking and let the user re-pick.
        toast(errorMessage(err), "error");
        try { await Api.del(`/bookings/${booking.bookingId}`); } catch { }
        reserveBtn.disabled = false;
        reserveBtn.textContent = "Reserve & continue";
        init(); // reload the map to reflect current availability
        document.getElementById("event-content").style.display = "block";
        document.getElementById("loading").style.display = "none";
    }
}

document.getElementById("reserve-btn").addEventListener("click", reserveAndContinue);

init();
