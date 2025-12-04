// ==========================================
// CONFIGURACIÓN
// ==========================================
// Asegúrate de que este puerto sea el mismo que usa tu C# (revisa la consola negra del servidor)
const SERVER_BASE_URL = 'http://localhost:5001'; 
const API_ONGS = '/api/ongs'; 

// Elementos del DOM
const ongsContainer = document.querySelector(".ongs-container");
const filterBtns = document.querySelectorAll('.filter-btn');
const searchInput = document.getElementById('buscarOng');
const statusMessage = document.getElementById('status-message');

// Variable para almacenar las ONGs cargadas del servidor
let allOngs = [];

// ==========================================
// 1. CARGAR DATOS
// ==========================================
async function cargarONGs() {
    statusMessage.textContent = "Cargando organizaciones...";
    
    try {
        const response = await fetch(SERVER_BASE_URL + API_ONGS);

        if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

        const data = await response.json();
        
        if (data.length === 0) {
            statusMessage.textContent = "No se encontraron organizaciones registradas.";
            return;
        }

        allOngs = data;
        statusMessage.textContent = ""; 
        renderizarTarjetas(allOngs);

    } catch (error) {
        console.error("Error al cargar ONGs:", error);
        statusMessage.innerHTML = `<span style="color: red;">Error de conexión. Revisa que el servidor C# esté corriendo.</span>`;
    }
}

// ==========================================
// 2. RENDERIZADO DE TARJETAS 
// ==========================================
function renderizarTarjetas(lista) {
    ongsContainer.innerHTML = ''; 

if (lista.length === 0) {
    ongsContainer.innerHTML = `
        <div class="sin-resultados">
            <i>📭</i>
            <p>No hay resultados para tu búsqueda.</p>
        </div>
    `;
    return;
}



    lista.forEach(ong => {
        
        let imagenUrl = 'https://via.placeholder.com/300x200?text=Sin+Imagen'; // Default

        if (ong.logo) {
            // Si la ruta en BD ya trae 'http' (es externa), la usamos tal cual.
            // Si no, asumimos que es una imagen guardada en nuestro servidor (wwwroot)
            if (ong.logo.startsWith('http')) {
                imagenUrl = ong.logo;
            } else {
                // Concatenamos: http://localhost:5001 + / + logos_ong/foto.jpg
                imagenUrl = `${SERVER_BASE_URL}/${ong.logo}`;
            }
        }
        // -----------------------------------

        const nombre = ong.nombre || "Sin Nombre";
        const desc = ong.descripcion || "Sin descripción disponible.";
        const categoria = ong.sector || "General";
        const rating = "⭐⭐⭐⭐⭐"; 

        const card = document.createElement("div");
        card.className = "ong-card";
        card.dataset.category = categoria; 

        card.innerHTML = `
            <img src="${imagenUrl}" alt="${nombre}" onerror="this.src='https://via.placeholder.com/300x200?text=Error+Carga'">
            <h3>${nombre}</h3>
            <p>${desc}</p>
            <div class="tags">
                <span class="tag">${categoria}</span>
                <span class="tag">${ong.pais || 'Global'}</span>
            </div>
            <div class="rating">${rating}</div>
            <button class="btn-vermas" onclick="verDetalle(${ong.id})">Ver más</button>
        `;
        
        ongsContainer.appendChild(card);
    });
}

// ==========================================
// 3. FILTROS Y BÚSQUEDA
// ==========================================
function aplicarFiltros() {
    const textoBusqueda = searchInput.value.toLowerCase();
    const btnActivo = document.querySelector('.filter-btn.active');
    const categoriaSeleccionada = btnActivo ? btnActivo.dataset.category : 'all';

    const filtradas = allOngs.filter(ong => {
        const coincideTexto = ong.nombre.toLowerCase().includes(textoBusqueda) || 
                              (ong.descripcion && ong.descripcion.toLowerCase().includes(textoBusqueda));
        
        const sectorOng = normalizeString(ong.sector);
        const catFiltro = normalizeString(categoriaSeleccionada);

        const coincideCategoria = (categoriaSeleccionada === 'all') || (sectorOng.includes(catFiltro));

        return coincideTexto && coincideCategoria;
    });

    renderizarTarjetas(filtradas);
}

function normalizeString(str) {
    return str ? str.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase() : "";
}

filterBtns.forEach(btn => {
    btn.addEventListener('click', () => {
        filterBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        aplicarFiltros(); 
    });
});

searchInput.addEventListener('input', aplicarFiltros);

// ==========================================
// 4. DETALLES
// ==========================================
function verDetalle(id) {
    const ongSeleccionada = allOngs.find(o => o.id === id);
    if (ongSeleccionada) {
        localStorage.setItem("ongDetalle", JSON.stringify(ongSeleccionada));
        window.location.href = "detalleONG.html"; 
    }
}

// INICIAR
document.addEventListener('DOMContentLoaded', cargarONGs);