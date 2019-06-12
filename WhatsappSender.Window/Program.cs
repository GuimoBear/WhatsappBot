using System;
using System.IO;
using WhatsappSender.Window.Messages;

namespace WhatsappSender.Window
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var service = new BrowserWindowManager())
            {
                var message = MessageBuilder.Make()
                                    .WithReceiver("5584981691098", "José Angelo")
                                    //.WithReceiver("5584988111414", "Gabiru")
                                    //.WithAttachment("D:\\backup setor.csv", "Arquivo CSV 1")
                                    //.WithAttachment("D:\\backup.csv", "Arquivo CSV 2")
                                    .WithMessage("Teste de BOT Do Whatsapp")
                                    .Build();
                service.AguardarAutenticacao();
                service.Enviar(message);
                //service.EnviarImagem("84996005239", "Arquivo CSV", );
                //service.EnviarMensagem("84996005239", "Testando envio de mensagem");
                Console.ReadKey();
                service.Logoff();
            }
        }
    }
}
