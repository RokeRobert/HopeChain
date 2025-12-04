// Configuración
const baseUrl = (typeof SERVER_BASE_URL !== 'undefined') ? SERVER_BASE_URL : 'http://localhost:5001';
const API_CONTACTO = '/api/contacto';

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('formContacto');
    const btnEnviar = document.getElementById('btnEnviar');
    const mensajeEstado = document.getElementById('mensajeEstado');

    if (form) {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            // UI Loading
            btnEnviar.disabled = true;
            btnEnviar.textContent = "Enviando...";
            mensajeEstado.style.display = 'none';
            mensajeEstado.textContent = "";

            // Capturar datos
            const datos = {
                nombre: document.getElementById('nombre').value,
                empresa: document.getElementById('empresa').value,
                telefono: document.getElementById('telefono').value,
                email: document.getElementById('email').value,
                asunto: document.getElementById('asunto').value,
                mensaje: document.getElementById('mensaje').value
            };

            try {
                const response = await fetch(baseUrl + API_CONTACTO, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datos)
                });

                if (!response.ok) throw new Error("Error en el envío.");

                // Éxito
                mensajeEstado.style.color = "green";
                mensajeEstado.textContent = "¡Mensaje enviado correctamente! Nos pondremos en contacto pronto.";
                mensajeEstado.style.display = 'block';
                form.reset();

            } catch (error) {
                console.error("Error:", error);
                mensajeEstado.style.color = "red";
                mensajeEstado.textContent = "Hubo un problema al enviar el mensaje. Intenta más tarde.";
                mensajeEstado.style.display = 'block';
            } finally {
                btnEnviar.disabled = false;
                btnEnviar.textContent = "Enviar";
            }
        });
    }
});