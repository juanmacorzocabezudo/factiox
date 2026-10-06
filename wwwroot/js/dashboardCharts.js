window.renderDashboardCharts = function (datos) {
    const moneda = new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' });
    const porcentaje = new Intl.NumberFormat('es-ES', { maximumFractionDigits: 1 });

    function crearGrafico(id, configuracion) {
        const anterior = Chart.getChart(id);
        if (anterior) anterior.destroy();
        const canvas = document.getElementById(id);
        if (canvas) new Chart(canvas, configuracion);
    }

    function crearBarras(id, importes, etiqueta, color) {
        crearGrafico(id, {
            type: 'bar',
            data: {
                labels: datos.meses,
                datasets: [{ label: etiqueta, data: importes, backgroundColor: color, borderRadius: 4 }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                locale: 'es-ES',
                plugins: {
                    legend: { display: false },
                    tooltip: { callbacks: { label: contexto => moneda.format(contexto.parsed.y) } }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { maxRotation: 45, minRotation: 0 } },
                    y: { beginAtZero: true, ticks: { callback: valor => moneda.format(valor) } }
                }
            }
        });
    }

    crearBarras('dashboardVentas', datos.ventas, 'Ventas', '#2e7d32');
    crearBarras('dashboardCompras', datos.compras, 'Compras', '#81c784');

    const total = datos.presupuestos.reduce((suma, cantidad) => suma + cantidad, 0);
    const porcentajeEstado = cantidad => porcentaje.format(total === 0 ? 0 : cantidad * 100 / total);
    crearGrafico('dashboardPresupuestos', {
        type: 'doughnut',
        data: {
            labels: datos.estados.map((estado, indice) =>
                `${estado}: ${datos.presupuestos[indice]} (${porcentajeEstado(datos.presupuestos[indice])}%)`),
            datasets: [{ data: datos.presupuestos, backgroundColor: ['#78909c', '#ff9800', '#4caf50', '#f44336', '#607d8b'] }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            locale: 'es-ES',
            cutout: '55%',
            plugins: {
                legend: { position: 'bottom', labels: { boxWidth: 12, padding: 12 } },
                tooltip: { callbacks: { label: contexto => contexto.label } }
            }
        }
    });
};