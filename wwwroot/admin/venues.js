Auth.requireAdmin();
renderNav("venues");

async function loadVenues() {
    const loading = document.getElementById("list-loading");
    const empty = document.getElementById("list-empty");
    const host = document.getElementById("list-host");
    loading.style.display = "block";
    empty.style.display = "none";
    host.innerHTML = "";

    try {
        const venues = await Api.get("/venues");
        loading.style.display = "none";
        if (!venues.length) {
            empty.style.display = "block";
            return;
        }
        host.innerHTML = `
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Name</th><th>Capacity</th><th></th></tr>
        </thead>
        <tbody>
          ${venues
              .map(
                  (v) => `<tr>
              <td>${v.venueId}</td>
              <td>${escapeHtml(v.name)}</td>
              <td>${v.totalCapacity}</td>
              <td><button type="button" class="btn btn-ghost btn-sm" data-delete="${v.venueId}">Delete</button></td>
            </tr>`
              )
              .join("")}
        </tbody>
      </table>`;

        host.querySelectorAll("[data-delete]").forEach((btn) => {
            btn.addEventListener("click", async () => {
                const id = Number(btn.dataset.delete);
                if (!confirmAction("Delete this venue?")) return;
                try {
                    await Api.del(`/venues/${id}`);
                    toast("Venue deleted.", "success");
                    loadVenues();
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

document.getElementById("venue-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertHost = document.getElementById("form-alert");
    const submitBtn = document.getElementById("submit-btn");
    alertHost.innerHTML = "";

    const payload = {
        name: document.getElementById("name").value.trim(),
        address: document.getElementById("address").value.trim(),
        totalCapacity: Number(document.getElementById("capacity").value),
    };

    submitBtn.disabled = true;
    try {
        await Api.post("/venues", payload);
        toast("Venue added.", "success");
        e.target.reset();
        loadVenues();
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
    } finally {
        submitBtn.disabled = false;
    }
});

loadVenues();
