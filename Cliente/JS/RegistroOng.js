// ==========================================
// CONFIGURACIÓN
// ==========================================

const baseUrl = 'http://localhost:5001';

const API_ONGS_LISTA = '/api/ongs'; 
const API_CATALOGOS = '/api/catalogos'; 
const API_REGISTRO_ONG = '/api/RegistroOng'; 

// ==========================================
// INICIALIZACIÓN
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
    cargarListaONGs();   // Carga las ONGs para "Unirme a existente"
    cargarCatalogos();   // Carga Sectores y Países para "Nueva ONG"
    
    // Asegurar estado inicial correcto de los formularios
    toggleModo();
});

// ==========================================
// 1. LÓGICA DE INTERFAZ (UI)
// ==========================================

// Alternar entre "Unirme a existente" y "Crear Nueva"
window.toggleModo = function() {
    // Busca cuál radio button está seleccionado
    const radioSeleccionado = document.querySelector('input[name="modoOng"]:checked');
    if (!radioSeleccionado) return;

    const modo = radioSeleccionado.value;
    const bloqueExistente = document.getElementById('bloque-existente');
    const bloqueNueva = document.getElementById('bloque-nueva');

    if (modo === 'existente') {
        bloqueExistente.classList.remove('hidden');
        bloqueNueva.classList.add('hidden');
        setRequiredNueva(false); // Apagar validación de campos de nueva ONG
    } else {
        bloqueExistente.classList.add('hidden');
        bloqueNueva.classList.remove('hidden');
        setRequiredNueva(true); // Encender validación
    }
}

// Activa o desactiva el atributo 'required' para que el navegador no bloquee el envío
function setRequiredNueva(isRequired) {
    const camposIds = ['oNombre', 'oDesc', 'oRFC', 'cNombre', 'cCorreo'];
    camposIds.forEach(id => {
        const el = document.getElementById(id);
        if(el) el.required = isRequired;
    });
}

// ==========================================
// 2. CARGA DE DATOS (Backend)
// ==========================================

// Cargar lista de ONGs existentes
async function cargarListaONGs() {
    try {
        const res = await fetch(baseUrl + API_ONGS_LISTA);
        if (!res.ok) throw new Error("Error al cargar lista de ONGs");
        
        const ongs = await res.json();
        const select = document.getElementById('selectOngExistente');
        select.innerHTML = '<option value="">Seleccione su organización...</option>';
        
        if(ongs.length === 0) {
            const opt = document.createElement('option');
            opt.textContent = "No hay ONGs registradas aún";
            select.appendChild(opt);
            return;
        }

        ongs.forEach(ong => {
            const opt = document.createElement('option');
            opt.value = ong.id; // El ID de la ONG
            opt.textContent = ong.nombre;
            select.appendChild(opt);
        });
    } catch (e) { 
        console.error("Error cargando ONGs", e); 
    }
}

// Cargar Catálogos (Sectores y Países) - ¡IMPORTANTE PARA LA LLAVE FORÁNEA!
async function cargarCatalogos() {
    try {
        const res = await fetch(baseUrl + API_CATALOGOS);
        if (!res.ok) throw new Error("Error al cargar catálogos");
        
        const data = await res.json();

        // 1. Llenar Select de SECTOR (Tipos ONG)
        const selectSector = document.getElementById('oSector');
        selectSector.innerHTML = '<option value="">Selecciona el sector...</option>';
        
        if (data.sectores) {
            data.sectores.forEach(sector => {
                const opt = document.createElement('option');
                opt.value = sector.id; // Esto envía el número (ej: 1, 2) al servidor
                opt.textContent = sector.nombre; // Esto muestra el texto (ej: Salud)
                selectSector.appendChild(opt);
            });
        }

        // 2. Llenar Select de PAÍS
        const selectPais = document.getElementById('oPais');
        selectPais.innerHTML = '<option value="">Selecciona el país...</option>';
        
        if (data.paises) {
            data.paises.forEach(pais => {
                const opt = document.createElement('option');
                opt.value = pais.id;
                opt.textContent = pais.nombre;
                selectPais.appendChild(opt);
            });
        }

    } catch (e) {
        console.error("Error cargando catálogos", e);
    }
}

// ==========================================
// 3. ENVÍO DEL FORMULARIO
// ==========================================
document.getElementById('formRegistroONG').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const btn = document.querySelector('.btn-registrar');
    const errorMsg = document.getElementById('mensajeError');
    const modo = document.querySelector('input[name="modoOng"]:checked').value;

    // A. Validaciones Locales
    const pass = document.getElementById('uPass').value;
    const confirm = document.getElementById('uPassConfirm').value;
    
    if (pass !== confirm) {
        alert("Las contraseñas no coinciden");
        return;
    }

    if (modo === 'existente' && !document.getElementById('selectOngExistente').value) {
        alert("Por favor selecciona una organización de la lista.");
        return;
    }

    // Bloquear botón para evitar doble clic
    btn.disabled = true;
    btn.textContent = "Procesando...";
    errorMsg.style.display = 'none';

    try {
        // B. Preparar Datos (FormData es necesario para enviar archivos)
        const formData = new FormData();

        // -- Datos Usuario --
        formData.append('uNombre', document.getElementById('uNombre').value);
        formData.append('uApellidoPaterno', document.getElementById('uApellidoPaterno').value);
        formData.append('uApellidoMaterno', document.getElementById('uApellidoMaterno').value); 
        formData.append('uCorreo', document.getElementById('uCorreo').value);
        formData.append('uPass', pass);
        formData.append('modo', modo); // "existente" o "nueva"

        if (modo === 'existente') {
            // Solo enviamos el ID de la ONG seleccionada
            formData.append('ongId', document.getElementById('selectOngExistente').value);
        } 
        else {
            // -- Datos Nueva ONG --
            formData.append('oNombre', document.getElementById('oNombre').value);
            formData.append('oDesc', document.getElementById('oDesc').value);
            formData.append('oRFC', document.getElementById('oRFC').value);
            formData.append('oWeb', document.getElementById('oWeb').value);
            
            // Aquí enviamos los IDs seleccionados de los combos que cargamos dinámicamente
            formData.append('oSector', document.getElementById('oSector').value); 
            formData.append('oPais', document.getElementById('oPais').value);
            
            // -- Contacto ONG --
            formData.append('cNombre', document.getElementById('cNombre').value);
            formData.append('cTel', document.getElementById('cTel').value);
            formData.append('cCorreo', document.getElementById('cCorreo').value);

            // -- Archivos --
            const fileLogo = document.getElementById('fileLogo').files[0];
            const fileComp = document.getElementById('fileComprobante').files[0];

            if (fileLogo) formData.append('fileLogo', fileLogo);
            if (fileComp) formData.append('fileComprobante', fileComp);
        }

        console.log("Enviando datos a:", baseUrl + API_REGISTRO_ONG);

        // C. Enviar al Servidor
        const response = await fetch(baseUrl + API_REGISTRO_ONG, {
            method: 'POST',
            body: formData 
        });

        // D. Procesar Respuesta
        let result;
        try { 
            result = await response.json(); 
        } catch(e) { 
            result = { message: "Error de conexión o respuesta no válida" }; 
        }

        if (!response.ok) {
            throw new Error(result.message || "Error desconocido en el servidor");
        }

        // Éxito
        alert("¡Registro completado! " + (modo === 'nueva' ? "Tu ONG está pendiente de validación." : "Bienvenido al equipo."));
        window.location.href = "login.html"; // Redirigir al login

    } catch (error) {
        console.error(error);
        errorMsg.textContent = "Error: " + error.message;
        errorMsg.style.display = 'block';
    } finally {
        btn.disabled = false;
        btn.textContent = "Finalizar Registro";
    }
});