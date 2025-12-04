// --- CONFIGURACIÓN ---
const baseUrl = (typeof SERVER_BASE_URL !== 'undefined') ? SERVER_BASE_URL : 'http://localhost:5001';
const API_REGISTRO = '/api/registro';

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('formRegistro');
    const mensajeError = document.getElementById('mensajeError');
    const btnSubmit = document.querySelector('.btn-registrar');

    // Animación de corazones
    iniciarAnimacionCorazones();

    

    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault(); 

            if(mensajeError) {
                mensajeError.textContent = "";
                mensajeError.style.display = 'none';
            }
            btnSubmit.disabled = true;
            btnSubmit.textContent = "Registrando...";

            // 1. Capturar datos
            const nombres = document.getElementById('nombres').value.trim();
            const apellidoP = document.getElementById('apellidoP').value.trim();
            const apellidoM = document.getElementById('apellidoM').value.trim();
            const correo = document.getElementById('correo').value.trim();
            const password = document.getElementById('contrasena').value;
            const confirmPassword = document.getElementById('confirmarContrasena').value;
            
            const paisId = parseInt(document.getElementById('pais').value);
           
            const tipoCuentaId = 2; // 2 = Usuario (Valor por defecto)
            
            const rolId = document.getElementById('rol') ? parseInt(document.getElementById('rol').value) : 1;

            // 2. Validaciones
            if (password !== confirmPassword) {
                mostrarError("Las contraseñas no coinciden.");
                btnSubmit.disabled = false;
                btnSubmit.textContent = "Registrarse";
                return;
            }
            if (password.length < 6) {
                mostrarError("La contraseña debe tener al menos 6 caracteres.");
                btnSubmit.disabled = false;
                btnSubmit.textContent = "Registrarse";
                return;
            }

            // 3. DTO para enviar al servidor
            const nuevoUsuario = {
                nombres: nombres,
                apellidoPaterno: apellidoP,
                apellidoMaterno: apellidoM,
                correo: correo,
                contrasena: password, 
                paisId: paisId,
                tipoCuentaId: tipoCuentaId
                
            };

            // 4. Enviar
            try {
                const response = await fetch(baseUrl + API_REGISTRO, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(nuevoUsuario)
                });

                let data;
                try { data = await response.json(); } catch (err) { data = {}; }

                if (!response.ok) {
                    throw new Error(data.message || `Error ${response.status}: No se pudo registrar.`);
                }

                alert("¡Registro exitoso! Bienvenido a HopeChain.");
                window.location.href = "login.html";

            } catch (error) {
                console.error("Error:", error);
                mostrarError(error.message);
            } finally {
                btnSubmit.disabled = false;
                btnSubmit.textContent = "Registrarse";
            }
        });
    }

    function mostrarError(msg) {
        if(mensajeError) {
            mensajeError.textContent = msg;
            mensajeError.style.display = 'block';
        } else {
            alert(msg);
        }
    }
});

// --- ANIMACIÓN (Igual que antes) ---
function iniciarAnimacionCorazones() {
    const svgContainer = document.getElementById('svg-hearts');
    if(!svgContainer) return;
    
    const colors = ['#f4b000', '#2c82f6', '#45c4b0', '#ffffff'];

    function createCurvedHeart() {
        const svgNS = "http://www.w3.org/2000/svg";
        const svg = document.createElementNS(svgNS, "svg");
        const path = document.createElementNS(svgNS, "path");

        svg.setAttribute("viewBox", "0 0 24 24");
        svg.classList.add("svg-heart");

        const color = colors[Math.floor(Math.random() * colors.length)];
        path.setAttribute("fill", color);
        path.setAttribute(
          "d",
          "M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41 0.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
        );
        svg.appendChild(path);
        svg.style.left = Math.random() * 100 + "vw";
        svg.style.animationDuration = Math.random() * 3 + 6 + "s";
        svg.style.transform = `scale(${Math.random() + 0.5})`;
        svg.style.setProperty("--xOffset", Math.random() * 100 - 50 + "px");
        svgContainer.appendChild(svg);
        setTimeout(() => svg.remove(), 9000);
    }
    setInterval(createCurvedHeart, 400);
}