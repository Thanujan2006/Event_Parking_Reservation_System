renderNav("dashboard");
Auth.requireLogin();

async function load() {
    const customer = Auth.currentCustomer();

    try {
        const dash = await Api.get(`/dashboard/customer/${customer.customerId}`);

        document.getElementById("upcoming-list").innerHTML = dash.upcomingBookings.length
            ? dash.upcomingBookings
                .map(
                    (b) => `
        <div class="summary-line" style="align-items:flex-start;">
          <span>
            <a href="${b.status === "Pending" ? `checkout.html?bookingId=${b.bookingId}` : "my-bookings.html"}">${escapeHtml(b.eventName)}</a>
            <br /><span class="muted" style="font-size:0.8rem;">${escapeHtml(b.bookingNumber)} · ${b.status}</span>
          </span>
          <span>${formatMoney(b.totalAmount)}</span>
        </div>`
                )
                .join("")
            : `<p class="muted" style="font-size:0.88rem;">No upcoming bookings. <a href="index.html">Browse events</a>.</p>`;

        document.getElementById("parking-list").innerHTML = dash.reservedParking.length
            ? dash.reservedParking.map((p) => `<div class="summary-line"><span>Slot ${escapeHtml(p.slotNumber)}</span><span>${formatMoney(p.fee)}</span></div>`).join("")
            : `<p class="muted" style="font-size:0.88rem;">No parking reserved.</p>`;

        document.getElementById("payments-list").innerHTML = dash.recentPayments.length
            ? dash.recentPayments
                .map(
                    (p) => `<div class="summary-line">
          <span><a href="receipt.html?paymentId=${p.paymentId}">${escapeHtml(p.bookingNumber)}</a></span>
          <span>${formatMoney(p.amount)}</span>
        </div>`
                )
                .join("")
            : `<p class="muted" style="font-size:0.88rem;">No payments yet.</p>`;

        document.getElementById("unread-count").textContent = `${dash.unreadNotificationCount} unread`;
        document.getElementById("notifications-list").innerHTML = dash.unreadNotifications.length
            ? dash.unreadNotifications
                .map(
                    (n) => `<div class="alert alert-info" style="margin-bottom:8px;" data-id="${n.notificationId}">
          ${escapeHtml(n.message)}
          <button class="btn btn-ghost btn-sm" style="margin-left:8px;" data-action="mark-read" data-id="${n.notificationId}">Mark read</button>
        </div>`
                )
                .join("")
            : `<p class="muted" style="font-size:0.88rem;">You're all caught up.</p>`;

        document.querySelectorAll("[data-action='mark-read']").forEach((btn) =>
            btn.addEventListener("click", async () => {
                try {
                    await Api.put(`/notifications/${btn.dataset.id}/read`);
                    load();
                } catch (err) {
                    toast(errorMessage(err), "error");
                }
            })
        );

        document.getElementById("loading").style.display = "none";
        document.getElementById("dash-content").style.display = "block";
    } catch (err) {
        toast(errorMessage(err), "error");
    }
}

load();
