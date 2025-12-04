const SERVER_URL = 'http://localhost:5001';

// 1. RECUPERAR ID DE LA ONG
const ongLocal = JSON.parse(localStorage.getItem("ongDetalle"));
if (!ongLocal || !ongLocal.id) {
    alert("Error: No se seleccionó una ONG. Redirigiendo...");
    window.location.href = "ONGs.html";
}
const ONG_ID = ongLocal.id;

// VARIABLES GLOBALES
let montoSeleccionado = 0;
let campañaSeleccionadaNombre = "Donación General (ONG)";
let campañaSeleccionadaId = null; 

document.addEventListener("DOMContentLoaded", () => {
    cargarDetalleONG();
    cargarCampanasReales();
    cargarComentariosONG();      
    inicializarFormularioComentarios(); 
    inicializarEventosPago();
});

// ==========================================
// A. CARGAR INFO Y CAMPAÑAS
// ==========================================
async function cargarDetalleONG() {
    document.getElementById("ong-name").textContent = ongLocal.nombre;
    // Intentamos cargar info fresca del servidor si existe el endpoint, si no usamos local
    try {
        const res = await fetch(`${SERVER_URL}/api/detalle-ong/${ONG_ID}`);
        if(res.ok) {
            const data = await res.json();
            document.getElementById("ong-description").textContent = data.descripcion;
            
            // Imagen
            const logoUrl = data.logo && !data.logo.startsWith('http') 
                ? `${SERVER_URL}/${data.logo}` 
                : (data.logo || 'https://via.placeholder.com/150');
            
            document.getElementById("ong-image-container").innerHTML = 
                `<img src="${logoUrl}" style="width:100%; height:100%; object-fit:contain;">`;
                
            document.getElementById("ong-contact-info").innerHTML = `
                <p><strong>Plataforma Web:</strong> <a href="${data.plataformaWeb}" target="_blank">Visitar sitio</a></p>
            `;
        }
    } catch(e) { console.error(e); }

    cargarOpcionesDonacion();
}

async function cargarCampanasReales() {
    const container = document.getElementById("campaigns-container");
    container.innerHTML = '<p>Cargando campañas...</p>';

    try {
        const res = await fetch(`${SERVER_URL}/api/detalle-ong/${ONG_ID}/campanas`);
        const campanas = await res.json();

        container.innerHTML = '';

        if (campanas.length === 0) {
            container.innerHTML = '<p>Esta ONG no tiene campañas activas por el momento.</p>';
            return;
        }

        campanas.forEach(camp => {
            let img = 'https://via.placeholder.com/300x150?text=Sin+Imagen';
            if (camp.imagen) {
                img = camp.imagen.startsWith('http') ? camp.imagen : `${SERVER_URL}/${camp.imagen}`;
            }

            const div = document.createElement("div");
            div.className = "card";
            // Escapar comillas para evitar errores en onclick
            const nombreSafe = camp.nombreCampana.replace(/"/g, '&quot;').replace(/'/g, "\\'");
            
            div.innerHTML = `
                <img src="${img}" style="width:100%; height:150px; object-fit:cover; border-radius:8px;">
                <h3>${camp.nombreCampana}</h3>
                <p>${camp.descripcion ? camp.descripcion.substring(0, 80) : ''}...</p>
                <button class="btn btn-ver-detalle" style="margin-top:10px; width:100%;" 
                        onclick="seleccionarCampaña(${camp.campanaID}, '${nombreSafe}')">
                    Apoyar esta campaña
                </button>
            `;
            container.appendChild(div);
        });

    } catch (error) {
        console.error(error);
        container.innerHTML = '<p style="color:red;">Error al cargar campañas.</p>';
    }
}

// ==========================================
// B. LÓGICA DE DONACIÓN
// ==========================================
window.seleccionarCampaña = function(id, nombre) {
    campañaSeleccionadaId = id;
    campañaSeleccionadaNombre = "Campaña: " + nombre;
    
    // Actualizar UI
    const destinoLbl = document.getElementById("destino-donacion");
    if(destinoLbl) destinoLbl.textContent = campañaSeleccionadaNombre;

    // Mostrar botón reset
    const btnReset = document.getElementById("btnResetCampana");
    if(btnReset) btnReset.style.display = "inline-block";

    // Scroll
    document.getElementById("donation-section").scrollIntoView({ behavior: 'smooth' });
}

window.resetearDonacion = function() {
    campañaSeleccionadaId = null;
    campañaSeleccionadaNombre = `la ONG ${ongLocal.nombre}`;
    
    document.getElementById("destino-donacion").textContent = campañaSeleccionadaNombre;
    document.getElementById("btnResetCampana").style.display = "none";
}

function cargarOpcionesDonacion() {
    const container = document.getElementById("donations-container");
    const textoDestino = campañaSeleccionadaId ? campañaSeleccionadaNombre : `la ONG ${ongLocal.nombre}`;
    const displayReset = campañaSeleccionadaId ? 'inline-block' : 'none';

    container.innerHTML = `
        <div class="card" style="text-align:center; padding:30px;">
            <h3>Donación Monetaria</h3>
                      <img src="/img/Logo/DonacionM.gif" style="width: 100px;" >
            <p>Tu aporte va a: <strong id="destino-donacion" style="color:#2c82f6;">${textoDestino}</strong></p>
            
            <button id="btnResetCampana" onclick="resetearDonacion()" 
                    style="display:${displayReset}; background:none; border:none; color:#dc3545; font-size:0.85em; text-decoration:underline; cursor:pointer; margin-bottom:15px;">
                ✕ Cancelar campaña (Donar general)
            </button>

            <div class="montos-rapidos" style="display:flex; gap:10px; justify-content:center; margin:20px 0; flex-wrap:wrap;">
                <button class="btn-money"  onclick="iniciarDonacion(50)">$50</button>
                <button class="btn-money"  onclick="iniciarDonacion(100)">$100</button>
                <button class="btn-money"  onclick="iniciarDonacion(200)">$200</button>
                <button class="btn-money"  onclick="iniciarDonacion(500)">$500</button>
            </div>
            <button id="btnOtroMonto" class="btn-otro-monto">
            💰 Otro monto
            </button>

        </div>
    `;

    document.getElementById("btnOtroMonto").addEventListener("click", () => {
        document.getElementById("modalMonto").style.display = "flex";
    });
}

window.iniciarDonacion = function(monto) {
    const usuarioStr = localStorage.getItem('usuario');
    if (!usuarioStr) {
        if(confirm("Para donar necesitas iniciar sesión. ¿Ir al login?")) {
            window.location.href = "login.html";
        }
        return;
    }
    montoSeleccionado = monto;
    document.getElementById("modalPago").style.display = "flex";
}

// Eventos Modales
const btnConfirmarMonto = document.getElementById("confirmarMonto");
if(btnConfirmarMonto) {
    btnConfirmarMonto.addEventListener("click", () => {
        const val = parseFloat(document.getElementById("montoPersonalizado").value);
        if (val > 0) {
            iniciarDonacion(val);
            document.getElementById("modalMonto").style.display = "none";
        } else {
            alert("Monto inválido");
        }
    });
}
document.getElementById("cancelarMonto").onclick = () => document.getElementById("modalMonto").style.display = "none";
document.querySelectorAll(".cerrar-modal, .close-ticket").forEach(btn => {
    btn.addEventListener("click", () => btn.closest(".modal").style.display = "none");
});

// PROCESAR PAGO
function inicializarEventosPago() {
    const btnProcesar = document.getElementById("procesarPago");
    if(!btnProcesar) return;

    btnProcesar.addEventListener("click", async () => {
        const titular = document.getElementById("titular").value;
        if(!titular) { alert("Ingresa el titular"); return; }

        document.getElementById("loaderOverlay").style.display = "flex";
        const usuario = JSON.parse(localStorage.getItem('usuario'));

        const donacionData = {
            Monto: montoSeleccionado,
            UsuarioId: usuario.id,
            Titular: titular,
            OngId: ONG_ID // Enviamos el ID de la ONG
            
            
        };

        try {
            const res = await fetch(`${SERVER_URL}/api/donaciones/procesar`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(donacionData)
            });

            const result = await res.json();

            setTimeout(() => {
                document.getElementById("loaderOverlay").style.display = "none";
                document.getElementById("modalPago").style.display = "none";

                if (res.ok) {
                    mostrarTicketFinal();
                } else {
                    alert("Error: " + result.message);
                }
            }, 2000);

        } catch (error) {
            console.error(error);
            document.getElementById("loaderOverlay").style.display = "none";
            alert("Error de conexión");
        }
    });
}

function mostrarTicketFinal() {
    const modalTicket = document.getElementById("modalTicket");
    const content = document.getElementById("ticketContent");
    const fecha = new Date().toLocaleDateString();
    
    content.innerHTML = `
        <div style="text-align:center; padding:20px;">
            <p><strong>ONG:</strong> ${ongLocal.nombre}</p>
            <p><strong>Destino:</strong> ${campañaSeleccionadaNombre}</p>
            <p><strong>Monto:</strong> $${montoSeleccionado} MXN</p>
            <p><strong>Fecha:</strong> ${fecha}</p>
            <p style="color:green; font-weight:bold;">✔ Donación Exitosa</p>
        </div>
        <button onclick="document.getElementById('modalTicket').style.display='none'" style="width:100%; padding:10px; background:#007bff; color:white; border:none; border-radius:5px; cursor:pointer;">Cerrar</button>
    `;
    modalTicket.style.display = "flex";
}

// ==========================================
// C. COMENTARIOS Y VALORACIONES
// ==========================================
async function cargarComentariosONG() {
    const container = document.getElementById("comentarios-ong-container");
    if(!container) return;

    try {
        const res = await fetch(`${SERVER_URL}/api/detalle-ong/${ONG_ID}/comentarios`);
        const comentarios = await res.json();
        
        container.innerHTML = '';
        if (comentarios.length === 0) {
            container.innerHTML = '<p style="color:#777; font-style:italic;">No hay opiniones aún. ¡Sé el primero!</p>';
            return;
        }

        comentarios.forEach(c => {
            const div = document.createElement("div");
            div.style.cssText = "background:#f9f9f9; padding:15px; margin-bottom:15px; border-radius:8px; border:1px solid #eee;";
            
            const estrellas = "⭐".repeat(c.valoracion);
            
            div.innerHTML = `
                <div style="display:flex; justify-content:space-between;">
                    <strong style="color:#333;">${c.autor}</strong>
                    <span style="color:#f4b000; font-size:0.9em;">${estrellas}</span>
                </div>
                <p style="margin:10px 0; color:#555;">"${c.texto}"</p>
                <small style="color:#999;">Publicado recientemente</small>
            `;
            container.appendChild(div);
        });
    } catch(e) { console.error("Error comentarios", e); }
}

function inicializarFormularioComentarios() {
    const usuarioStr = localStorage.getItem('usuario');
    const msgLogin = document.getElementById('msgLoginComentario');
    const formWrapper = document.getElementById('formComentarioWrapper');
    const form = document.getElementById('formComentarioOng');

    // Control de Visibilidad
    if (usuarioStr) {
        if(formWrapper) formWrapper.style.display = 'block';
    } else {
        if(msgLogin) msgLogin.style.display = 'block';
    }

    // Envío
    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const txt = document.getElementById('txtComentarioOng').value;
            const rate = document.getElementById('ratingOng').value;
            const btn = document.getElementById('btnEnviarComentario');
            
            if (!txt.trim()) { alert("Escribe algo."); return; }

            const usuario = JSON.parse(usuarioStr);
            btn.disabled = true;
            btn.textContent = "Enviando...";

            const payload = {
                UsuarioId: usuario.id,
                Texto: txt,
                Valoracion: parseInt(rate)
            };

            try {
                const res = await fetch(`${SERVER_URL}/api/detalle-ong/${ONG_ID}/comentarios`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (res.ok) {
                    alert("¡Gracias por tu opinión!");
                    form.reset();
                    cargarComentariosONG(); // Recargar lista
                } else {
                    const err = await res.json();
                    alert("Error: " + (err.message || "Falló"));
                }
            } catch (error) {
                console.error(error);
                alert("Error de conexión");
            } finally {
                btn.disabled = false;
                btn.textContent = "Publicar Opinión";
            }
        });
    }
}