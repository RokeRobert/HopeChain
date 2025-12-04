document.addEventListener('DOMContentLoaded', () => {
    actualizarHeader();
});

function actualizarHeader() {
    const navContainer = document.querySelector('header nav');
    
    // Si la página no tiene <nav> (ej. error 404), salimos
    if (!navContainer) return;

    const usuarioStr = localStorage.getItem('usuario');
    
    // ===============================================
    // 1. MENÚ BASE (Visible para TODOS: Invitados y Usuarios)
    // ===============================================
    let html = `
        <a href="/Html/index.html">Inicio</a>
        <a href="/Html/ONGs.html">ONGs</a>
        <a href="/Html/Reportes.html">Reportes</a>
    `;

    // ===============================================
    // 2. LÓGICA DE USUARIOS LOGUEADOS
    // ===============================================
    if (usuarioStr) {
        const usuario = JSON.parse(usuarioStr);
        
        // --- CASO A: ES UNA ONG ---
        if (usuario.tipo === 'ONG') {
            html += `
                <a href="/Html/PanelONG.html" style="color:#2c82f6; font-weight:600;">
                    <i class="fas fa-chart-line"></i> Mi Panel
                </a>
            `;
        } 
        
        // --- CASO B: ES ADMINISTRADOR (RolID = 2) ---
        else if (usuario.rolId === 2) { 
            html += `
                <a href="/Html/PanelAdmin.html" style="color:#fd7e14; font-weight:600;">
                    <i class="fas fa-user-shield"></i> Admin
                </a>
            `;
        }
        
        // --- CASO C: ES DONANTE (Usuario Normal) ---
        else {
            html += `
                <a href="/Html/MiCuenta.html" style="color:#2c82f6; font-weight:600;">
                    <i class="fas fa-user-circle"></i> Mi Cuenta
                </a>
            `;
        }

        // --- BOTÓN DE SALIR (COMÚN PARA TODOS LOS LOGUEADOS) ---
        // Mostramos el nombre y el botón de cerrar sesión
        html += `
            <div id="auth-buttons" style="display: inline-block; margin-left: 20px;">
                <span class="user-welcome" style="font-size:0.9em; margin-right:10px; color:#555;">
                    Hola, ${usuario.nombre.split(' ')[0]}
                </span>
                <button onclick="cerrarSesionGlobal()" class="btn-logout-header" style="background:transparent; border:1px solid #dc3545; color:#dc3545; border-radius:20px; padding:6px 15px; cursor:pointer; font-weight:600; transition:0.3s;">
                    <i class="fas fa-sign-out-alt"></i> Salir
                </button>
            </div>
        `;

    } else {
        // ===============================================
        // 3. LÓGICA DE INVITADOS (NO LOGUEADOS)
        // ===============================================
        html += `
            <a href="/Html/Contacto.html">Contacto</a>
            <div id="auth-buttons" style="display: inline-block; margin-left: 20px;">
                <a href="/Html/login.html" class="btn-inicio-sesion">Iniciar sesión</a>
                <a href="/Html/Registro.html" class="btn-registrarse">Registrarse</a>
            </div>
        `;
    }

    // Inyectar el menú final en el HTML
    navContainer.innerHTML = html;
}

// Función global para cerrar sesión
window.cerrarSesionGlobal = function() {
    document.getElementById("modalCerrarSesion").style.display = "flex";
};
window.confirmarCerrarSesion = function () {
    localStorage.removeItem("usuario");
    window.location.href = "/Html/login.html";
};

window.cancelarCerrarSesion = function () {
    document.getElementById("modalCerrarSesion").style.display = "none";
};
