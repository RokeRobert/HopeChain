const SERVER_URL = 'http://localhost:5001';

document.addEventListener('DOMContentLoaded', () => {
    verificarAdmin();
    cargarPendientes();
});

// 1. PROTECCIÓN DE RUTA
function verificarAdmin() {
    const usuarioStr = localStorage.getItem('usuario');
    if (!usuarioStr) {
        window.location.href = 'login.html';
        return;
    }
    
    const usuario = JSON.parse(usuarioStr);
    
    
    if (usuario.rolId !== 2 && usuario.tipo !== 'ADMIN') { 
        alert("Acceso denegado. Área exclusiva para administradores.");
        window.location.href = 'Home2.html';
    }
}

// 2. CARGAR LISTA
async function cargarPendientes() {
    const tbody = document.querySelector('#tablaPendientes tbody');
    
    try {
        const response = await fetch(`${SERVER_URL}/api/admin/pendientes`);
        const ongs = await response.json();

        tbody.innerHTML = '';

        if (ongs.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center">No hay solicitudes pendientes.</td></tr>`;
            return;
        }

        ongs.forEach(ong => {
            // Rutas de archivos
            const logoUrl = ong.logo ? `${SERVER_URL}/${ong.logo}` : 'https://via.placeholder.com/50';
            const docUrl = ong.comprobante ? `${SERVER_URL}/${ong.comprobante}` : '#';
            const docTarget = ong.comprobante ? '_blank' : '';

            const row = document.createElement('tr');
            row.innerHTML = `
                <td><img src="${logoUrl}" alt="Logo" class="ong-logo-mini"></td>
                <td>
                    <strong>${ong.nombre}</strong><br>
                    <small style="color:#777;">${ong.descripcion.substring(0, 40)}...</small>
                </td>
                <td>${ong.rfc || 'N/A'}</td>
                <td>
                    <a href="${docUrl}" target="${docTarget}" class="btn-doc">
                        <i class="fas fa-file-pdf"></i> Ver PDF
                    </a>
                </td>
                <td>
                    <button class="btn-action btn-approve" onclick="procesarOng(${ong.id}, 1)">
                        <i class="fas fa-check"></i> Aprobar
                    </button>
                    <button class="btn-action btn-reject" onclick="procesarOng(${ong.id}, 3)">
                        <i class="fas fa-times"></i> Rechazar
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });

    } catch (error) {
        console.error(error);
        tbody.innerHTML = `<tr><td colspan="5" class="text-center" style="color:red;">Error al cargar datos.</td></tr>`;
    }
}

// 3. APROBAR / RECHAZAR
window.procesarOng = async function(id, nuevoEstatus) {
    const accion = nuevoEstatus === 1 ? "aprobar" : "rechazar";
    if (!confirm(`¿Estás seguro de ${accion} a esta organización?`)) return;

    try {
        const response = await fetch(`${SERVER_URL}/api/admin/cambiar-estatus`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ongId: id, nuevoEstatus: nuevoEstatus })
        });

        const result = await response.json();

        if (response.ok) {
            alert(result.message);
            cargarPendientes(); // Recargar la tabla
        } else {
            alert("Error: " + result.message);
        }
    } catch (error) {
        console.error(error);
        alert("Error de conexión.");
    }
}