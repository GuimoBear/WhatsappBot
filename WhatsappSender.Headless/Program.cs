using System;
using System.IO;
using System.Threading;
using WhatsappSender.Headless.Managers;

namespace WhatsappSender.Headless
{
    class Program
    {
        private const string TOKEN = "6fe604a5fa6e463393488867318f69c9";
        static void Main(string[] args)
        {
            using (var manager = new HeadlessBrowserManager())
            {
                BrowserContextManager contextManager = null;
                if (!manager.TryGetManager(TOKEN, out contextManager))
                    contextManager = manager.CreateManager(TOKEN);

                contextManager.Screenshot(Path.Combine(AppContext.BaseDirectory, "before login.png"));
                contextManager.Logon();
                Thread.Sleep(1000);
                contextManager.Screenshot(Path.Combine(AppContext.BaseDirectory, "after login.png"));
                //contextManager.Logoff();
                //Thread.Sleep(1000);
                contextManager.Screenshot(Path.Combine(AppContext.BaseDirectory, "after logout.png"));
                Console.ReadKey();
            }
        }
    }
}
