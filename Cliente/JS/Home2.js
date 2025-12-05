// ==========================================
// CONFIGURACIÓN GLOBAL
// ==========================================
const SERVER_URL = 'http://localhost:5001'; 

document.addEventListener("DOMContentLoaded", () => {
    // Cargas iniciales
    cargarEstadisticas();
    cargarCampanasCarrusel();
    inicializarDonaciones();
    cargarComentariosPlataforma(); 
    inicializarFormularioComentarios(); 
});

// ==========================================
// A. ESTADÍSTICAS (Versión Directa y Segura)
// ==========================================
async function cargarEstadisticas() {
    try {
        const res = await fetch(`${SERVER_URL}/api/home/stats`);
        
        if(res.ok) {
            const stats = await res.json();
            
            // Animamos los contadores
            actualizarContador('totalONG', stats.ongs);
            actualizarContador('totalDonaciones', stats.donaciones);
            actualizarContador('paises', stats.paises);
            actualizarContador('reportes', stats.reportes);
            
            // Hacemos visibles las tarjetas por si el CSS las tenía ocultas
            document.querySelectorAll('.stat').forEach(el => el.classList.add('show'));
        }
    } catch (error) {
        console.error("Error cargando stats", error);
    }
}

function actualizarContador(id, valorFinal) {
    const elemento = document.getElementById(id);
    if (!elemento) return;
    
    // Si el valor es 0 o nulo, ponemos 0
    const target = valorFinal || 0;
    
    // Animación simple
    let start = 0;
    const duration = 2000; 
    const step = Math.ceil(target / (duration / 20)); 

    const timer = setInterval(() => {
        start += step;
        if (start >= target) {
            start = target;
            clearInterval(timer);
        }
        elemento.textContent = start;
    }, 20);
}

// ==========================================
// B. CARRUSEL CAMPAÑAS (Redirige a la ONG dueña)
// ==========================================
async function cargarCampanasCarrusel() {
    const track = document.getElementById('campaignsContainer');
    if(!track) return;

    try {
        const res = await fetch(`${SERVER_URL}/api/home/campanas-destacadas`);
        const campanas = await res.json();

        track.innerHTML = ''; 

        if(campanas.length === 0) {
            track.innerHTML = '<p style="text-align:center; width:100%; padding:20px;">No hay campañas activas.</p>';
            return;
        }

        campanas.forEach(camp => {
            let imgUrl = 'https://via.placeholder.com/300x200?text=Campaña';

            if(camp.imagen) {
                imgUrl = camp.imagen.startsWith('http') ? camp.imagen : `${SERVER_URL}/${camp.imagen}`;
            }

            const div = document.createElement("div");
            div.classList.add("campaign");

            // --- CORRECCIÓN: Usamos camp.ongId en lugar de camp.id ---
            div.innerHTML = `
                <img src="${imgUrl}" alt="${camp.nombre}">
                <div class="campaign-info">
                    <h3>${camp.nombre}</h3>
                    <p>${camp.descripcion ? camp.descripcion.substring(0, 80) + '...' : 'Sin descripción.'}</p>
                    <button class="btn-secundario" onclick="verPerfilOng(${camp.ongId})">Ver Detalles</button>
                </div>
            `;

            track.appendChild(div);
        });

        if(campanas.length > 0) iniciarSlider();

    } catch (error) {
        console.error("Error campañas", error);
    }
}

// --- NUEVA FUNCIÓN DE REDIRECCIÓN ---
function verPerfilOng(idOng) {
    if(!idOng) {
        console.error("No se recibió el ID de la ONG");
        return;
    }
    // Redirige al perfil de la ONG dueña
    window.location.href = `detalleONG.html?id=${idOng}`;
}

// (La función iniciarSlider se mantiene igual...)
function iniciarSlider() {
    const track = document.querySelector('.carousel-track');
    const nextBtn = document.querySelector('.carousel-btn.next');
    const prevBtn = document.querySelector('.carousel-btn.prev');
    
    if(!track || !nextBtn || !track.firstElementChild) return;

    track.style.display = "flex";
    track.style.overflowX = "auto"; 
    track.style.scrollBehavior = "smooth"; 
    track.style.scrollbarWidth = "none"; 
    
    const cardStyle = window.getComputedStyle(track.firstElementChild);
    const cardWidth = track.firstElementChild.offsetWidth + 
                      parseInt(cardStyle.marginRight) + 
                      parseInt(cardStyle.marginLeft);

    nextBtn.addEventListener('click', () => {
        track.scrollBy({ left: cardWidth, behavior: 'smooth' });
    });

    prevBtn.addEventListener('click', () => {
        track.scrollBy({ left: -cardWidth, behavior: 'smooth' });
    });
}

// ==========================================
// C. DONACIONES (LÓGICA DE PAGO + SEGURIDAD)
// ==========================================
function inicializarDonaciones() {
    const btnDonar = document.getElementById("btnDonar");
    const btnProcesar = document.getElementById("procesarPago");
    const btnMoney = document.querySelectorAll(".btn-money");
    const ingrMoney = document.getElementById("ingrMoney");
    const modalPago = document.getElementById("modalPago");
    const modalTicket = document.getElementById("modalTicket");
    let cantidad = 0;

    // Selección de monto predefinido
    btnMoney.forEach(btn => {
        btn.addEventListener('click', () => {
            btnMoney.forEach(b => b.classList.remove("active-money")); 
            btn.style.background = "#e0f0ff"; 
            btn.style.borderColor = "#2c82f6";
            
            cantidad = parseInt(btn.dataset.val);
            if(ingrMoney) ingrMoney.value = "";
        });
    });

    // Entrada manual de monto
    if(ingrMoney) {
        ingrMoney.addEventListener('input', () => {
            cantidad = parseInt(ingrMoney.value) || 0;
            // Limpiar selección de botones si escribe manual
            btnMoney.forEach(b => {
                b.style.background = ""; 
                b.style.borderColor = "";
            });
        });
    }

    // ABRIR MODAL (CON VALIDACIÓN DE LOGIN)
    if(btnDonar) {
        btnDonar.addEventListener('click', () => {
            
            // 1. VERIFICAR SESIÓN
            const usuarioStr = localStorage.getItem('usuario');
            if (!usuarioStr) {
                const irLogin = confirm("Para realizar una donación segura, necesitas iniciar sesión.\n\n¿Quieres ir al login ahora?");
                if (irLogin) {
                    window.location.href = "login.html";
                }
                return; 
            }

            // 2. VERIFICAR MONTO
            if(cantidad <= 0) {
                alert("Por favor selecciona un monto para donar.");
                return;
            }

            // 3. ABRIR MODAL
            if(modalPago) modalPago.style.display = "flex";
        });
    }

    // CERRAR MODAL
    document.getElementById("cerrarModal")?.addEventListener('click', () => {
        modalPago.style.display = "none";
    });

    // PROCESAR PAGO (FETCH AL SERVIDOR)
    if(btnProcesar) {
        btnProcesar.addEventListener('click', async () => {
            const titular = document.getElementById("titular").value;
            if(!titular) { alert("Ingresa el nombre del titular"); return; }

            // Loader visual
            const loader = document.getElementById("loaderOverlay");
            if(loader) {
                loader.classList.remove("hidden");
                document.getElementById("loaderProgress").style.width = "100%";
            }

            const usuario = JSON.parse(localStorage.getItem('usuario'));

            const donacionData = {
                Monto: cantidad,
                UsuarioId: usuario.id,
                Titular: titular,
                OngId: 999 // ID genérico para donación a plataforma
            };

            try {
                const res = await fetch(`${SERVER_URL}/api/donaciones/procesar`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(donacionData)
                });

                const result = await res.json();

                setTimeout(() => {
                    if(loader) loader.classList.add("hidden");
                    modalPago.style.display = "none";

                    if(res.ok) {
                        modalTicket.style.display = "flex";
                        renderTicket(cantidad, titular);
                        cargarEstadisticas(); // Actualizar contadores globales
                    } else {
                        alert("Error: " + result.message);
                    }
                }, 2000);

            } catch (error) {
                console.error(error);
                if(loader) loader.classList.add("hidden");
                alert("Error de conexión con el servidor de pagos.");
            }
        });
    }
}

// RENDERIZAR TICKET
function renderTicket(monto, titular) {
    const content = document.getElementById("ticketContent");
    content.innerHTML = `
        <div style="text-align:center; padding:20px;">
            <p style="font-size:1.2rem;"><strong>Monto:</strong> $${monto} MXN</p>
            <p><strong>Donante:</strong> ${titular}</p>
            <p><strong>Fecha:</strong> ${new Date().toLocaleDateString()}</p>
            <p style="color:green; font-weight:bold; margin-top:10px;">✔ Transacción Exitosa</p>
        </div>
        <button id="btnDescargarPDF" class="btn-acento" style="width:100%; margin-top:10px;">Descargar Comprobante</button>
    `;

    document.getElementById("btnDescargarPDF").addEventListener('click', () => {
        generarPDF(monto, titular);
    });
    
    document.getElementById("cerrarTicket").onclick = () => {
        document.getElementById("modalTicket").style.display = "none";
    };
}

// GENERAR PDF (Requiere jsPDF)
function generarPDF(monto, nombre) {
    if (!window.jspdf) { alert("Librería PDF no cargada"); return; }
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    
    doc.setFontSize(22);
    doc.setTextColor(44, 130, 246); // Azul HopeChain
    doc.text("HopeChain - Comprobante", 20, 30);
    
    doc.setFontSize(12);
    doc.setTextColor(0,0,0);
    doc.text(`Donante: ${nombre}`, 20, 50);
    doc.text(`Monto: $${monto} MXN`, 20, 60);
    doc.text(`Fecha: ${new Date().toLocaleString()}`, 20, 70);
    doc.text(`ID Transacción: #${Math.floor(Math.random()*1000000)}`, 20, 80);
    
    doc.save("Recibo_HopeChain.pdf");
}

// ==========================================
// D. COMENTARIOS PLATAFORMA (REALES)
// ==========================================
async function cargarComentariosPlataforma() {
    const container = document.getElementById("testimonialsContainer");
    if(!container) return;

    try {
        const res = await fetch(`${SERVER_URL}/api/comentarios/plataforma`);
        const comentarios = await res.json();

        container.innerHTML = '';

        if(comentarios.length === 0) {
            container.innerHTML = '<p style="text-align:center; width:100%; color:#888;">Sé el primero en opinar.</p>';
            return;
        }

        comentarios.forEach(c => {
            const estrellas = "⭐".repeat(c.valoracion);
            const avatarUrl = `https://ui-avatars.com/api/?name=${c.autor}&background=random`;

            const card = document.createElement("div");
            card.classList.add("testimonial-card");

            card.innerHTML = `
                <div class="rating">
                    <span>${estrellas}</span>
                </div>

                <p>"${c.texto}"</p>

                <div class="user">
                    <img src="${avatarUrl}">
                    <div class="user-info">
                        <h4>${c.autor}</h4>
                        <span>Usuario verificado</span>
                    </div>
                </div>
            `;

            container.appendChild(card);
        });

    } catch (error) {
        console.error("Error comentarios:", error);
    }
}

// ==========================================
// E. FORMULARIO DE COMENTARIOS (Auth)
// ==========================================
function inicializarFormularioComentarios() {
    const usuarioStr = localStorage.getItem('usuario');
    const divLogin = document.getElementById('mensajeLoginComentario');
    const divForm = document.getElementById('formComentarioContainer');
    
    // 1. Mostrar u ocultar según sesión
    if (usuarioStr) {
        const usuario = JSON.parse(usuarioStr);
        // Si hay usuario logueado, mostramos el formulario
        if(divForm) {
            divForm.style.display = 'block';
            document.getElementById('nombreComentador').textContent = usuario.nombre;
        }
    } else {
        // Si no hay sesión, mostramos invitación a login
        if(divLogin) divLogin.style.display = 'block';
    }

    // 2. Manejar el envío
    const form = document.getElementById('formComentarioPlataforma');
    if(form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const btn = document.getElementById('btnEnviarComentario');
            const txt = document.getElementById('txtComentario').value;
            const val = document.getElementById('selValoracion').value;
            const usuario = JSON.parse(localStorage.getItem('usuario'));

            if(!txt || !val) return;

            btn.disabled = true;
            btn.textContent = "Enviando...";

            try {
                const res = await fetch(`${SERVER_URL}/api/comentarios/plataforma`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        UsuarioId: usuario.id,
                        Texto: txt,
                        Valoracion: parseInt(val)
                    })
                });

                if(res.ok) {
                    alert("¡Gracias por tu comentario!");
                    form.reset();
                    cargarComentariosPlataforma(); // Recargar lista al instante
                } else {
                    alert("Error al enviar comentario.");
                }
            } catch (error) {
                console.error(error);
                alert("Error de conexión");
            } finally {
                btn.disabled = false;
                btn.textContent = "Enviar comentario";
            }
        });
    }
}