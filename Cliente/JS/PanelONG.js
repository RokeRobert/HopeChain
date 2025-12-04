// ==========================================
// CONFIGURACIÓN
// ==========================================
const SERVER_URL = 'http://localhost:5001';

// VARIABLE GLOBAL PARA CONTROLAR EDICIÓN
let idCampañaEditando = null; // Si es null = Creando. Si tiene número = Editando.

// ==========================================
// INICIALIZACIÓN
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
    verificarSesion();
    cargarDatosPerfil();
    cargarMisCampanas(); 
});

// 1. VERIFICAR SESIÓN
function verificarSesion() {
    const usuarioStr = localStorage.getItem('usuario');
    
    if (!usuarioStr) {
        alert("Debes iniciar sesión para acceder al panel.");
        window.location.href = "login.html";
        return;
    }

    const usuario = JSON.parse(usuarioStr);
    
    if (usuario.tipo.toUpperCase() !== 'ONG') {
        alert("Acceso denegado. Este panel es exclusivo para Organizaciones.");
        window.location.href = "index.html";
    }
}

// 2. CARGAR DATOS EN SIDEBAR
async function cargarDatosPerfil() {
    const usuario = JSON.parse(localStorage.getItem('usuario'));
    if (!usuario) return;

    document.getElementById('sidebarRepresentante').textContent = usuario.nombre;
    document.getElementById('sidebarId').textContent = usuario.rolId;

    try {
        const response = await fetch(`${SERVER_URL}/api/ong-perfil/${usuario.rolId}`);
        if (response.ok) {
            const ong = await response.json();
            document.getElementById('sidebarNombreOng').textContent = ong.nombre;

            // Logo
            const imgElement = document.getElementById('sidebarLogo');
            if (ong.logo) {
                const ruta = ong.logo.startsWith('http') ? ong.logo : `${SERVER_URL}/${ong.logo}`;
                imgElement.src = ruta;
            }

            // Estatus
            const statusSpan = document.getElementById('statusIndicador');
            if (ong.estatusID === 1) {
                statusSpan.textContent = "Activa";
                statusSpan.style.color = "#27ae60"; 
            } else {
                statusSpan.textContent = "Pendiente";
                statusSpan.style.color = "#f39c12"; 
            }
        }
    } catch (error) {
        console.error("Error cargando perfil ONG", error);
    }
}

// 3. CARGAR LISTA DE CAMPAÑAS
async function cargarMisCampanas() {
    const usuario = JSON.parse(localStorage.getItem('usuario'));
    const contenedor = document.getElementById('listaCampanas');
    
    try {
        const response = await fetch(`${SERVER_URL}/api/campanas/ong/${usuario.rolId}`);
        if (!response.ok) throw new Error("Error al obtener campañas");

        const campanas = await response.json();
        contenedor.innerHTML = '';

        if (campanas.length === 0) {
            contenedor.innerHTML = `
                <div style="text-align:center; padding: 20px; color:#777; background:#fff; border-radius:8px; border:1px dashed #ccc;">
                    <p>No tienes campañas activas.</p>
                    <small>¡Usa el formulario de abajo para crear la primera!</small>
                </div>`;
            return;
        }

        campanas.forEach(campana => {
            let imagenUrl = 'https://via.placeholder.com/200x150?text=Sin+Imagen';
            if (campana.imagen) {
                 imagenUrl = campana.imagen.startsWith('http') 
                    ? campana.imagen 
                    : `${SERVER_URL}/${campana.imagen}`;
            }

            // Escapar comillas para evitar errores en el onclick
            const nombreSeguro = campana.nombre.replace(/"/g, '&quot;').replace(/'/g, "\\'");
            // Reemplazamos saltos de línea para que no rompa el JS
            const descSegura = campana.descripcion.replace(/"/g, '&quot;').replace(/'/g, "\\'").replace(/\n/g, ' ');

            const card = document.createElement('div');
            card.className = 'campaign-item';
            
            card.innerHTML = `
                <div style="display:flex; gap:20px; align-items:start; flex-wrap: wrap;">
                    <img src="${imagenUrl}" alt="Foto" 
                         style="width:120px; height:80px; object-fit:cover; border-radius:8px; border:1px solid #eee;">
                    
                    <div style="flex:1; min-width: 200px;">
                        <h4>${campana.nombre}</h4>
                        <p style="margin-bottom:10px; font-size:0.9em; color:#666;">
                            ${campana.descripcion.substring(0, 120)}...
                        </p>
                        <span class="tag" style="background:#e8f5e9; color:#2e7d32; font-size:0.8em;">ID: ${campana.id}</span>
                    </div>
                </div>

                <div class="campaign-actions">
                    <button class="btn btn-secondary" onclick="prepararEdicion(${campana.id}, '${nombreSeguro}', '${descSegura}')">
                        <i class="fas fa-edit"></i> Editar
                    </button>
                    <button class="btn btn-danger" onclick="eliminarCampana(${campana.id})">
                        <i class="fas fa-trash-alt"></i> Eliminar
                    </button>
                </div>
            `;
            contenedor.appendChild(card);
        });

    } catch (error) {
        console.error(error);
        contenedor.innerHTML = `<p style="color:red; text-align:center;">Error de conexión.</p>`;
    }
}

// 4. PREPARAR EDICIÓN (Llenar formulario)
window.prepararEdicion = function(id, nombre, descripcion) {
    // Scroll hacia el formulario
    document.getElementById('seccionFormulario').scrollIntoView({ behavior: 'smooth' });

    // Llenar inputs
    document.getElementById('cTitulo').value = nombre;
    document.getElementById('cDesc').value = descripcion;
    
    // Cambiar estado global
    idCampañaEditando = id;

    // Cambiar visualmente el formulario
    document.getElementById('tituloFormulario').innerHTML = '<i class="fas fa-edit"></i> Editando Campaña #' + id;
    
    const btn = document.getElementById('btnSubmit');
    btn.innerHTML = '<i class="fas fa-save"></i> Guardar Cambios';
    btn.style.backgroundColor = '#fd7e14'; // Naranja

    // Mostrar botón cancelar
    document.getElementById('btnCancelar').style.display = 'inline-block';
}

// 5. CANCELAR EDICIÓN (Limpiar todo)
window.cancelarEdicion = function() {
    document.getElementById('formCampana').reset();
    document.getElementById('preview-img').style.display = 'none';
    
    idCampañaEditando = null;

    // Restaurar visuales
    document.getElementById('tituloFormulario').innerHTML = '<i class="fas fa-plus-circle"></i> Publicar Nueva Campaña';
    
    const btn = document.getElementById('btnSubmit');
    btn.innerHTML = '<i class="fas fa-bullhorn"></i> Registrar Campaña';
    btn.style.backgroundColor = ''; // Color original (CSS)

    document.getElementById('btnCancelar').style.display = 'none';
}

// 6. ENVIAR FORMULARIO (CREAR O EDITAR)
document.getElementById('formCampana').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const btn = document.getElementById('btnSubmit');
    const usuario = JSON.parse(localStorage.getItem('usuario'));
    
    // Deshabilitar botón
    btn.disabled = true;
    const textoOriginal = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';

    const formData = new FormData();
    formData.append('nombre', document.getElementById('cTitulo').value);
    formData.append('descripcion', document.getElementById('cDesc').value);
    formData.append('ongId', usuario.rolId);
    
    const fileInput = document.getElementById('cImagen');
    if(fileInput.files[0]) {
        formData.append('imagen', fileInput.files[0]);
    }

    try {
        let url = `${SERVER_URL}/api/campanas/crear`;
        let method = 'POST';

        // Si estamos editando, cambiamos la URL y el Método
        if (idCampañaEditando !== null) {
            url = `${SERVER_URL}/api/campanas/editar/${idCampañaEditando}`;
            method = 'PUT';
        }

        const response = await fetch(url, {
            method: method,
            body: formData
        });

        const result = await response.json();

        if (response.ok) {
            alert(idCampañaEditando ? "¡Campaña actualizada!" : "¡Campaña creada!");
            cancelarEdicion(); // Resetea el form y vuelve a modo crear
            cargarMisCampanas(); // Recarga la lista
        } else {
            throw new Error(result.message || "Error en el servidor");
        }

    } catch (error) {
        console.error(error);
        alert("Error: " + error.message);
    } finally {
        btn.disabled = false;
        btn.innerHTML = textoOriginal;
    }
});

// 7. ELIMINAR CAMPAÑA
window.eliminarCampana = async function(id) {
    if(!confirm("¿Estás seguro de eliminar esta campaña permanentemente?")) return;

    try {
        const response = await fetch(`${SERVER_URL}/api/campanas/eliminar/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            alert("Campaña eliminada.");
            cargarMisCampanas();
            
            // Si estábamos editando justo la que borramos, cancelamos edición
            if (idCampañaEditando === id) cancelarEdicion();
        } else {
            alert("Error al eliminar.");
        }
    } catch (error) {
        console.error(error);
        alert("Error de conexión.");
    }
}

// 8. PREVIEW IMAGEN
window.mostrarPreview = function(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const img = document.getElementById('preview-img');
            img.src = e.target.result;
            img.style.display = 'block';
        }
        reader.readAsDataURL(input.files[0]);
    }
}

