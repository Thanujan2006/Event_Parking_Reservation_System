renderNav();
Auth.requireLogin();

const paymentId = new URLSearchParams(window.location.search).get("paymentId");

async function init() {
    if (!paymentId) {
        toast("Missing payment reference.", "error");
        window.location.href = "my-bookings.html";
        return;
    }

    try {
        const r = await Api.get(`/payments/${paymentId}/receipt`);

        document.getElementById("receipt-number").textContent = r.receiptNumber;
        document.getElementById("booking-number").textContent = r.bookingNumber;
        document.getElementById("event-name").textContent = r.eventName;
        document.getElementById("event-date").textContent = formatDate(r.eventDate);
        document.getElementById("customer-name").textContent = r.customerName;
        document.getElementById("customer-email").textContent = r.customerEmail;
        document.getElementById("paid-at").textContent = new Date(r.paidAt).toLocaleString();

        const seatLines = r.seats
            .map((s) => `<div class="summary-line"><span>Seat ${escapeHtml(s.seatRow)}${escapeHtml(s.seatNumber)}</span><span>${formatMoney(s.priceAtBooking)}</span></div>`)
            .join("");
        const parkingLine = r.parkingSlotNumber
            ? `<div class="summary-line"><span>Parking ${escapeHtml(r.parkingSlotNumber)}</span><span>${formatMoney(r.parkingFee)}</span></div>`
            : "";
        document.getElementById("receipt-lines").innerHTML = seatLines + parkingLine;
        document.getElementById("receipt-total").textContent = formatMoney(r.totalAmount);

        document.getElementById("loading").style.display = "none";
        document.getElementById("receipt-content").style.display = "block";
    } catch (err) {
        toast(errorMessage(err), "error");
        window.location.href = "my-bookings.html";
    }
}

init();
