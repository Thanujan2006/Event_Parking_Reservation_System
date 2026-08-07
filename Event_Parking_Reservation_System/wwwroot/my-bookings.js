renderNav("bookings");
Auth.requireLogin();

const badgeClass = { Pending: "badge-pending", Confirmed: "badge-confirmed", Cancelled: "badge-cancelled", Expired: "badge-expired" };

async function load() {
    const customer = Auth.currentCustomer();
    const loading = document.getElementById("loading");
    const empty = document.getElementById("empty");
    const list = document.getElementById("bookings-list");

    try {
        const bookings = await Api.get(`/bookings/customer/${customer.customerId}`);
        loading.style.display = "none";

        if (bookings.length === 0) {
            empty.style.display = "block";
            return;
        }

        list.innerHTML = bookings.map(bookingRowHtml).join("");

        list.querySelectorAll("[data-action='pay']").forEach((btn) =>
            btn.addEventListener("click", () => (window.location.href = `checkout.html?bookingId=${btn.dataset.id}`))
        );
        list.querySelectorAll("[data-action='cancel']").forEach((btn) =>
            btn.addEventListener("click", () => cancelBooking(btn.dataset.id))
        );
        list.querySelectorAll("[data-action='receipt']").forEach((btn) =>
            btn.addEventListener("click", () => loadReceiptLink(btn))
        );
    } catch (err) {
        loading.style.display = "none";
        toast(errorMessage(err), "error");
    }
}

function bookingRowHtml(b) {
    const seatsSummary = b.seats.map((s) => `${s.seatRow}${s.seatNumber}`).join(", ") || "No seats attached";
    const parkingSummary = b.parking ? ` · Parking ${b.parking.slotNumber}` : "";

    let actions = "";
    if (b.status === "Pending") {
        actions = `<button class="btn btn-accent btn-sm" data-action="pay" data-id="${b.bookingId}">Pay now</button>
                <button class="btn btn-ghost btn-sm" data-action="cancel" data-id="${b.bookingId}">Cancel</button>`;
    } else if (b.status === "Confirmed") {
        actions = `<button class="btn btn-ghost btn-sm" data-action="receipt" data-id="${b.bookingId}">View receipt</button>
                <button class="btn btn-ghost btn-sm" data-action="cancel" data-id="${b.bookingId}">Cancel booking</button>`;
    }

    return `
    <div class="panel">
      <div class="flex" style="justify-content:space-between;flex-wrap:wrap;gap:10px;">
        <div>
          <span class="badge ${badgeClass[b.status] || "badge-pending"}">${b.status}</span>
          <h3 style="margin:8px 0 2px;">${escapeHtml(b.eventName)}</h3>
          <p class="muted mb-0" style="font-size:0.88rem;">${escapeHtml(b.bookingNumber)} · ${seatsSummary}${parkingSummary}</p>
        </div>
        <div style="text-align:right;">
          <div style="font-family:var(--font-data);font-size:1.1rem;">${formatMoney(b.totalAmount)}</div>
          <div class="flex gap-8 mt-24" style="justify-content:flex-end;">${actions}</div>
        </div>
      </div>
    </div>
  `;
}

async function cancelBooking(bookingId) {
    if (!confirmAction("Cancel this booking? Seats and parking will be released back for others.")) return;
    try {
        await Api.del(`/bookings/${bookingId}`);
        toast("Booking cancelled.", "info");
        load();
    } catch (err) {
        toast(errorMessage(err), "error");
    }
}

async function loadReceiptLink(btn) {
    try {
        const customer = Auth.currentCustomer();
        const payments = await Api.get(`/payments/customer/${customer.customerId}`);
        const payment = payments.find((p) => p.bookingId === Number(btn.dataset.id));
        if (payment) window.location.href = `receipt.html?paymentId=${payment.paymentId}`;
        else toast("Receipt not found for this booking.", "error");
    } catch (err) {
        toast(errorMessage(err), "error");
    }
}

load();
