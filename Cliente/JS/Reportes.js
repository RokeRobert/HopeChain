const SERVER_URL = 'http://localhost:5001';
let donationChart = null;
let trendChart = null;
let historialActual = []; 
let nombreOngActual = "";

document.addEventListener('DOMContentLoaded', () => {
    console.log("Iniciando Reportes...");
    cargarDashboard();
    cargarTablaDesglose();
    
    // Al cambiar la fecha, recargamos
    document.getElementById('filtro-periodo').addEventListener('change', cargarDashboard);
});

// 1. CARGAR DASHBOARD (Sin filtro de sector)
async function cargarDashboard() {
    const periodo = document.getElementById('filtro-periodo').value;
    const url = `${SERVER_URL}/api/reportes/dashboard?periodo=${periodo}`;
    
    console.log("Consultando métricas:", url);

    try {
        const res = await fetch(url);
        
        if(res.ok) {
            const data = await res.json();
            console.log("Datos recibidos:", data); // <--- Revision de consola si falla

            const fmt = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' });
            
            // Actualizar Tarjetas (Verificamos que el elemento exista antes de asignar)
            const elTotal = document.getElementById('total-donado-global');
            if(elTotal) elTotal.textContent = fmt.format(data.totalDonadoGlobal);

            const elOngs = document.getElementById('ongs-activas');
            if(elOngs) elOngs.textContent = data.ongsActivas;

            const elTrans = document.getElementById('donaciones-count-global');
            if(elTrans) elTrans.textContent = data.totalTransacciones;
            
            const elDir = document.getElementById('tx-directas');
            if(elDir) elDir.textContent = data.transaccionesDirectas;

            const elCamp = document.getElementById('tx-campanas');
            if(elCamp) elCamp.textContent = data.transaccionesCampanas;

            // Renderizar Gráficas
            renderizarGraficoSectores(data.sectorDistribution);
            renderizarGraficoTendencia(data.flujoTendencia);
        } else {
            console.error("Error del servidor:", res.status);
        }
    } catch (e) { console.error("Error dashboard:", e); }
}

function renderizarGraficoSectores(datos) {
    const canvas = document.getElementById('grafico-sectores');
    if (!canvas) return; // Protección

    const ctx = canvas.getContext('2d');
    if (donationChart) donationChart.destroy();

    // Si no hay datos, mostramos vacío para que no explote chart.js
    const labels = datos && datos.length > 0 ? datos.map(d => d.sector) : ["Sin datos"];
    const values = datos && datos.length > 0 ? datos.map(d => d.monto) : [1];
    const colors = datos && datos.length > 0 
        ? ['#007bff', '#28a745', '#ffc107', '#dc3545', '#6f42c1'] 
        : ['#e0e0e0']; // Gris si vacío

    donationChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderWidth: 0
            }]
        },
        options: { 
            responsive: true, 
            maintainAspectRatio: false, // Importante para que respete el div contenedor
            plugins: { legend: { position: 'bottom' } } 
        }
    });
}

function renderizarGraficoTendencia(datos) {
    const canvas = document.getElementById('grafico-tendencia');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (trendChart) trendChart.destroy();

    const labels = datos ? datos.map(d => d.mes) : [];
    const values = datos ? datos.map(d => d.monto) : [];

    trendChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Recaudado ($)',
                data: values,
                borderColor: '#00cc66',
                backgroundColor: 'rgba(0, 204, 102, 0.1)',
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true } }
        }
    });
}

// 2. CARGAR TABLA TOP ONGs
async function cargarTablaDesglose() {
    const tbody = document.getElementById('tablaTopOngs');
    if(!tbody) return;
    
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center">Calculando datos...</td></tr>';

    try {
        const res = await fetch(`${SERVER_URL}/api/reportes/desglose-fondos`);
        const lista = await res.json();

        tbody.innerHTML = '';
        const fmt = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' });

        if (lista.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding:20px;">Aún no hay donaciones registradas.</td></tr>';
            return;
        }

        lista.forEach(item => {
            const row = document.createElement('tr');
            
            row.innerHTML = `
                <td style="font-weight:600; color:#333;">${item.nombre}</td>
                <td style="font-weight:700;">${fmt.format(item.totalGeneral)}</td>
                <td>
                    <div class="progress-container">
                        <div class="bar-direct" style="width: ${item.porcentajeDirecto}%" title="Directo: ${item.porcentajeDirecto}%"></div>
                        <div class="bar-campaign" style="width: ${item.porcentajeCampanas}%" title="Campaña: ${item.porcentajeCampanas}%"></div>
                    </div>
                    <div class="money-detail">
                        <span><span class="legend-dot" style="background:#007bff"></span> ${fmt.format(item.donadoDirecto)}</span>
                        <span><span class="legend-dot" style="background:#28a745"></span> ${fmt.format(item.donadoCampanas)}</span>
                    </div>
                </td>
                <td>
                    <button class="btn-ver-detalle" onclick="verDetalleHistorial(${item.id})">Ver</button>
                </td>
            `;
            tbody.appendChild(row);
        });

    } catch (e) { 
        console.error(e); 
        tbody.innerHTML = '<tr><td colspan="4" style="color:red; text-align:center">Error de conexión</td></tr>';
    }
}

// 3. HISTORIAL Y PDF
window.verDetalleHistorial = async function(id) {
    const modal = document.getElementById('modalHistorial');
    const tbody = document.getElementById('tablaModalBody');
    const titulo = document.getElementById('modalTituloOng');

    modal.style.display = 'flex';
    titulo.textContent = "Cargando...";
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center">Obteniendo datos...</td></tr>';

    try {
        const res = await fetch(`${SERVER_URL}/api/reportes/historial-ong/${id}`);
        if(res.ok) {
            const data = await res.json();
            historialActual = data.historial;
            nombreOngActual = data.nombreOng;

            titulo.textContent = `Historial: ${data.nombreOng}`;
            tbody.innerHTML = '';

            if (data.historial.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align:center">No hay movimientos.</td></tr>';
                return;
            }

            const fmt = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' });

            data.historial.forEach(mov => {
                const colorTipo = mov.tipo === 'Directo' ? '#007bff' : '#28a745';
                const fecha = new Date(mov.fecha).toLocaleDateString();
                
                tbody.innerHTML += `
                    <tr style="border-bottom:1px solid #eee;">
                        <td style="padding:10px;">${fecha}</td>
                        <td style="padding:10px;">${mov.concepto}</td>
                        <td style="padding:10px; text-align:center;">
                            <span style="background:${colorTipo}; color:white; padding:3px 8px; border-radius:10px; font-size:0.8em;">${mov.tipo}</span>
                        </td>
                        <td style="padding:10px; text-align:right; font-weight:bold;">${fmt.format(mov.monto)}</td>
                    </tr>
                `;
            });
        }
    } catch (error) {
        console.error(error);
        tbody.innerHTML = '<tr><td colspan="4" style="color:red; text-align:center">Error de conexión</td></tr>';
    }
}

// Generar PDF Completo (Dashboard)
window.exportarDashboardPDF = async function() {
    const btn = document.querySelector('.btn-filtrar[onclick="exportarDashboardPDF()"]');
    const contenido = document.getElementById('contenido-pdf');
    
    if(!contenido) return;

    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Generando...';

    try {
        const canvas = await html2canvas(contenido, { scale: 2, useCORS: true });
        const imgData = canvas.toDataURL('image/png');
        const { jsPDF } = window.jspdf;
        const pdf = new jsPDF('p', 'mm', 'a4');
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = (canvas.height * pdfWidth) / canvas.width;

        pdf.setFontSize(18);
        pdf.text("Informe de Transparencia HopeChain", 10, 15);
        pdf.setFontSize(10);
        pdf.text(`Generado el: ${new Date().toLocaleString()}`, 10, 22);
        pdf.addImage(imgData, 'PNG', 0, 30, pdfWidth, pdfHeight);
        pdf.save("Reporte_Plataforma.pdf");

    } catch (error) {
        console.error("Error PDF:", error);
        alert("Error al generar PDF");
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-file-pdf"></i> Descargar Informe';
    }
}

// Descargar PDF Historial (Modal)
window.descargarReportePDF = function() {
    if (!window.jspdf) { alert("Error PDF"); return; }
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    const fmt = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' });

    doc.setFontSize(18);
    doc.text(`Historial de Donaciones`, 14, 20);
    doc.setFontSize(12);
    doc.text(`Organización: ${nombreOngActual}`, 14, 30);

    const columnas = ["Fecha", "Concepto", "Tipo", "Monto"];
    const filas = historialActual.map(h => [
        new Date(h.fecha).toLocaleDateString(),
        h.concepto,
        h.tipo,
        fmt.format(h.monto)
    ]);

    doc.autoTable({ startY: 40, head: [columnas], body: filas, theme: 'striped' });
    doc.save(`Historial_${nombreOngActual}.pdf`);
}