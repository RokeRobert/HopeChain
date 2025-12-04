const SERVER_URL = 'http://localhost:5001'; 

document.addEventListener('DOMContentLoaded', () => {
    // 1. VERIFICAR SESIÓN (CON EL NUEVO OBJETO JSON)
    const usuarioStr = localStorage.getItem('usuario');

    if (!usuarioStr) {
        alert("No has iniciado sesión. Redirigiendo...");
        window.location.href = "login.html";
        return;
    }

    const usuario = JSON.parse(usuarioStr);

    // Opcional: Si una ONG intenta entrar aquí, la mandamos a su panel
    if (usuario.tipo === 'ONG') {
        window.location.href = "PerfilONG.html";
        return;
    }

    // 2. CARGAR DATOS DEL USUARIO
    cargarDatosPerfil(usuario.id);
});

async function cargarDatosPerfil(id) {
    try {
        // Llamamos al endpoint que ya tienes en Program.cs: /api/mi-cuenta/{id}
        const response = await fetch(`${SERVER_URL}/api/mi-cuenta/${id}`);
        
        if (!response.ok) throw new Error("Error al obtener datos del perfil");

        const data = await response.json();

        // 3. PINTAR DATOS EN EL HTML
        // Asegúrate de que los IDs en tu HTML coincidan con estos:
        
        // Círculo con inicial
        const inicial = data.nombres ? data.nombres.charAt(0).toUpperCase() : "U";
        const avatarElement = document.querySelector('.profile-header .avatar-circle'); // O busca por ID si tienes
        if (avatarElement) avatarElement.textContent = inicial;

        // Nombre y Rol
        document.getElementById('nombreUsuario').textContent = `${data.nombres} ${data.apellidoPaterno}`;
        
        // Correo y País 
        const emailEl = document.getElementById('correoUsuario');
        if(emailEl) emailEl.textContent = data.correoElectronico;
        
        const paisEl = document.getElementById('paisUsuario');
        if(paisEl) paisEl.textContent = data.pais || "No especificado";

        // Estadísticas (Total Donado)
        const totalDonadoEl = document.getElementById('totalDonado');
        if(totalDonadoEl) totalDonadoEl.textContent = `$${data.totalDonado.toFixed(2)}`;

        const numDonacionesEl = document.getElementById('numDonaciones');
        if(numDonacionesEl) numDonacionesEl.textContent = data.donacionesRealizadas;

        // 4. TABLA DE HISTORIAL
        const tbody = document.querySelector('#tablaHistorial tbody'); 
        if (tbody) {
            tbody.innerHTML = ''; // Limpiar "Cargando..."

            if (data.historial && data.historial.length > 0) {
                data.historial.forEach(donacion => {
                    const row = document.createElement('tr');
                    // Formatear fecha
                    const fecha = new Date(donacion.fecha).toLocaleDateString();
                    
                    row.innerHTML = `
                        <td>${fecha}</td>
                        <td>$${donacion.monto.toFixed(2)}</td>
                        <td>${donacion.nombre_ONG || 'Campaña General'}</td>
                        <td><span class="badge-success">Completado</span></td>
                    `;
                    tbody.appendChild(row);
                });
            } else {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Aún no tienes donaciones.</td></tr>';
            }
        }

    } catch (error) {
        console.error(error);
        alert("No se pudieron cargar los detalles de la cuenta.");
    }
}