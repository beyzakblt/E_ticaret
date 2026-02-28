namespace E_ticaret.Modellerim
{
    using System.Net;
    using System.Net.Mail;

    public static class MailHelper
    {
        public static void SendCode(string targetEmail, string code)
        {
            var fromAddress = new MailAddress("beyzakblt@gmail.com", "E-Ticaret Destek");
            var toAddress = new MailAddress(targetEmail);
            const string fromPassword = "eqvm lqjb ubnm hdok"; // Gmail'den alınan 16 haneli uygulama şifresi
            const string subject = "E-Ticaret Kayıt Onay Kodu";
            string body = $"Merhaba, kayıt işlemini tamamlamak için onay kodunuz: <b>{code}</b>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
}
