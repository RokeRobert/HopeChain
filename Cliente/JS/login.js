// Configuración
const baseUrl = (typeof SERVER_BASE_URL !== 'undefined') ? SERVER_BASE_URL : 'http://localhost:5001';
const API_LOGIN = '/api/login';

document.addEventListener('DOMContentLoaded', () => {
    
    // =========================================================
    // 0. GUARDIA DE SEGURIDAD
    // =========================================================

    const sesionActiva = localStorage.getItem('usuario');
    
    if (sesionActiva) {
        const usuario = JSON.parse(sesionActiva);
        if (usuario.tipo === 'ONG') {
            window.location.replace("PanelONG.html"); 
        } else if (usuario.rolId === 2) {
            window.location.replace("PanelAdmin.html");
        } else {
            window.location.replace("index.html"); 
        }
        return; 
    }

    // =========================================================
    // 1. LÓGICA DE "MOSTRAR CONTRASEÑA" 
    // =========================================================
    const checkMostrar = document.getElementById('mostrarPassword');
    const inputPass = document.getElementById('contrasenaLogin');

    if (checkMostrar && inputPass) {
        checkMostrar.addEventListener('change', function() {
            if (this.checked) {
                // Si está marcado, cambiamos a texto visible
                inputPass.type = 'text';
            } else {
                // Si no, lo regresamos a password (oculto)
                inputPass.type = 'password';
            }
        });
    }
    // =========================================================

    const form = document.getElementById('formLogin');
    const mensajeError = document.getElementById('mensajeErrorLogin'); 
    const btnSubmit = document.querySelector('.btn-registrar'); 

    if (typeof iniciarAnimacionCorazones === 'function') {
        iniciarAnimacionCorazones();
    }

    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault(); 

            if (mensajeError) {
                mensajeError.textContent = "";
                mensajeError.style.display = 'none';
            }
            
            if (btnSubmit) {
                btnSubmit.disabled = true;
                btnSubmit.textContent = "Verificando...";
            }

            const correo = document.getElementById('correoLogin').value.trim();
            const password = document.getElementById('contrasenaLogin').value;

            if (!correo || !password) {
                mostrarError("Por favor, ingresa correo y contraseña.");
                resetBtn();
                return;
            }

            const credenciales = {
                correo: correo,
                contrasena: password
            };

            try {
                const response = await fetch(baseUrl + API_LOGIN, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(credenciales)
                });

                let data;
                try { data = await response.json(); } catch (err) { data = {}; }

                if (response.status === 403 && data.bloqueado) {
                    alert(data.message + "\n\nTe redirigiremos a la página de contacto.");
                    window.location.href = "Contacto.html"; 
                    return; 
                }

                if (response.status === 401) {
                    throw new Error("Correo o contraseña incorrectos.");
                }

                if (!response.ok) {
                    throw new Error(data.message || `Error ${response.status}: No se pudo iniciar sesión.`);
                }

                // --- ÉXITO ---
                console.log("Login exitoso:", data);
                
                const sessionData = {
                    id: data.usuarioId,
                    nombre: data.nombre,
                    tipo: data.tipo,
                    rolId: data.rolId      
                };
                
                // SIMPLIFICADO: Guardamos siempre en localStorage (persistencia estándar)
                localStorage.setItem('usuario', JSON.stringify(sessionData));
                
                // Redirección
                if (data.tipo === 'ONG') {
                    window.location.href = "PanelONG.html";
                } else if (data.rolId === 2) { 
                    window.location.href = "PanelAdmin.html"; 
                } else {
                    window.location.href = "index.html"; 
                }

            } catch (error) {
                console.error("Error de login:", error);
                mostrarError(error.message);
            } finally {
                resetBtn();
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

    function resetBtn() {
        if (btnSubmit) {
            btnSubmit.disabled = false;
            btnSubmit.textContent = "Ingresar"; 
        }
    }
});