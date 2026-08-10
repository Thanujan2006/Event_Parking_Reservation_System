renderNav();
Auth.requireLogin();

const bookingId = new URLSearchParams(window.location.search).get("bookingId");
let countdownInterval = null;

async function init() {
    if (!bookingId) return showNotFound();

    try {
        const booking = await Api.get(`/bookings/${bookingId}`);
        if (booking.status === "Confirmed") {
            window.location.href = `my-bookings.html`;
            return;
        }
        if (booking.status !== "Pending") {
            showNotFound();
            return;
        }

        renderBooking(booking);
        document.getElementById("loading").style.display = "none";
        document.getElementById("checkout-content").style.display = "block";
        startCountdown(booking.holdExpiresAt);
    } catch (err) {
        showNotFound();
    }
}

function showNotFound() {
    document.getElementById("loading").style.display = "none";
    document.getElementById("checkout-empty").style.display = "block";
}

function renderBooking(booking) {
    document.getElementById("booking-number").textContent = booking.bookingNumber;
    document.getElementById("event-name").textContent = booking.eventName;

    const lines = booking.seats
        .map((s) => `<div class="summary-line"><span>Seat ${escapeHtml(s.seatRow)}${escapeHtml(s.seatNumber)}</span><span>${formatMoney(s.priceAtBooking)}</span></div>`)
        .join("");
    const parkingLine = booking.parking
        ? `<div class="summary-line"><span>Parking ${escapeHtml(booking.parking.slotNumber)}</span><span>${formatMoney(booking.parking.fee)}</span></div>`
        : "";

    document.getElementById("summary-lines").innerHTML = lines + parkingLine;
    document.getElementById("summary-total").textContent = formatMoney(booking.totalAmount);
}

function startCountdown(holdExpiresAt) {
    const timerEl = document.getElementById("hold-timer");
    const expiry = new Date(holdExpiresAt).getTime();

    function tick() {
        const remainingMs = expiry - Date.now();
        if (remainingMs <= 0) {
            clearInterval(countdownInterval);
            timerEl.textContent = "⏳ Hold expired";
            document.getElementById("checkout-alert").innerHTML =
                `<div class="alert alert-error">Your seat hold has expired and your seats were released. Please book again.</div>`;
            document.getElementById("pay-btn").disabled = true;
            document.getElementById("cancel-btn").style.display = "none";
            return;
        }
        const minutes = Math.floor(remainingMs / 60000);
        const seconds = Math.floor((remainingMs % 60000) / 1000);
        timerEl.textContent = `⏳ ${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
        timerEl.classList.toggle("is-urgent", remainingMs < 120000);
    }

    tick();
    countdownInterval = setInterval(tick, 1000);
}

async function pay() {
    if (!confirmAction("Confirm payment for this booking? This is a simulated transaction for demo purposes.")) return;

    const payBtn = document.getElementById("pay-btn");
    payBtn.disabled = true;
    payBtn.textContent = "Processing…";

    try {
        const payment = await Api.post(`/bookings/${bookingId}/payment`);
        clearInterval(countdownInterval);
        toast("Payment successful — booking confirmed!", "success");
        window.location.href = `receipt.html?paymentId=${payment.paymentId}`;
    } catch (err) {
        document.getElementById("checkout-alert").innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
        payBtn.disabled = false;
        payBtn.textContent = "Pay now (simulation)";
    }
}

async function cancel() {
    if (!confirmAction("Cancel this booking? Your seats and parking slot will be released.")) return;

    try {
        await Api.del(`/bookings/${bookingId}`);
        clearInterval(countdownInterval);
        toast("Booking cancelled.", "info");
        window.location.href = "index.html";
    } catch (err) {
        toast(errorMessage(err), "error");
    }
}

document.getElementById("pay-btn").addEventListener("click", pay);
document.getElementById("cancel-btn").addEventListener("click", cancel);

init();
