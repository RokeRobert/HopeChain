using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using TransparencyServer.Models;

namespace TransparencyServer.Services
{
    // Interfaz para la inyección de dependencias
    public interface IEmailService
    {
        Task<bool> SendContactForm(ContactoDto contacto);
    }
    
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendContactForm(ContactoDto contacto)
        {
            try
            {
                string body = $@"
                    <h1>Nuevo Mensaje de Contacto</h1>
                    <p><strong>Nombre:</strong> {contacto.Nombre}</p>
                    <p><strong>Email:</strong> {contacto.Email}</p>
                    <p><strong>Teléfono:</strong> {contacto.Telefono}</p>
                    <p><strong>Empresa:</strong> {contacto.Empresa}</p>
                    <hr/>
                    <h3>Asunto: {contacto.Asunto}</h3>
                    <p>{contacto.Mensaje}</p>
                ";

                using (var message = new MailMessage())
                {
                    message.Subject = $"[WEB] {contacto.Asunto}";
                    message.Body = body;
                    message.IsBodyHtml = true;

                    // Remitente y Destinatario
                    message.From = new MailAddress(_emailSettings.SenderEmail, "Formulario Web HopeChain");
                    message.To.Add(_emailSettings.ReceiverEmail);
                    
                    message.ReplyToList.Add(new MailAddress(contacto.Email));
                    
                    // Configurar el cliente SMTP (Gmail)
                    using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port);
                    client.EnableSsl = true; 
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(
                        _emailSettings.SenderEmail, 
                        _emailSettings.SenderPassword
                    );

                    await client.SendMailAsync(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                return false;
            }
        }
    }
}