Auth.requireAdmin();
renderNav("categories");

async function loadCategories() {
    const loading = document.getElementById("list-loading");
    const empty = document.getElementById("list-empty");
    const host = document.getElementById("list-host");
    loading.style.display = "block";
    empty.style.display = "none";
    host.innerHTML = "";

    try {
        const categories = await Api.get("/categories");
        loading.style.display = "none";
        if (!categories.length) {
            empty.style.display = "block";
            return;
        }
        host.innerHTML = `
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Name</th><th></th></tr>
        </thead>
        <tbody>
          ${categories
              .map(
                  (c) => `<tr>
              <td>${c.categoryId}</td>
              <td>${escapeHtml(c.name)}</td>
              <td><button type="button" class="btn btn-ghost btn-sm" data-delete="${c.categoryId}">Delete</button></td>
            </tr>`
              )
              .join("")}
        </tbody>
      </table>`;

        host.querySelectorAll("[data-delete]").forEach((btn) => {
            btn.addEventListener("click", async () => {
                const id = Number(btn.dataset.delete);
                if (!confirmAction("Delete this category?")) return;
                try {
                    await Api.del(`/categories/${id}`);
                    toast("Category deleted.", "success");
                    loadCategories();
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

document.getElementById("category-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const alertHost = document.getElementById("form-alert");
    const submitBtn = document.getElementById("submit-btn");
    alertHost.innerHTML = "";

    const payload = { name: document.getElementById("name").value.trim() };

    submitBtn.disabled = true;
    try {
        await Api.post("/categories", payload);
        toast("Category added.", "success");
        e.target.reset();
        loadCategories();
    } catch (err) {
        alertHost.innerHTML = `<div class="alert alert-error">${escapeHtml(errorMessage(err))}</div>`;
    } finally {
        submitBtn.disabled = false;
    }
});

loadCategories();
