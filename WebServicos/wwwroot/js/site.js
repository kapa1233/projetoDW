// ── WebServicos — JavaScript Global ──

// Fechar automaticamente os alertas após 5 segundos
document.addEventListener("DOMContentLoaded", function () {
    const alerts = document.querySelectorAll(".alert-dismissible:not(.alert-danger)");
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 5000);
    });
});

// Confirmação antes de submeter formulários de eliminação
document.querySelectorAll("form[data-confirm]").forEach(function (form) {
    form.addEventListener("submit", function (e) {
        const msg = form.getAttribute("data-confirm") || "Tem a certeza?";
        if (!confirm(msg)) {
            e.preventDefault();
        }
    });
});

// Tooltip Bootstrap em todos os elementos com data-bs-toggle="tooltip"
const tooltipElements = document.querySelectorAll('[data-bs-toggle="tooltip"]');
tooltipElements.forEach(el => new bootstrap.Tooltip(el));
