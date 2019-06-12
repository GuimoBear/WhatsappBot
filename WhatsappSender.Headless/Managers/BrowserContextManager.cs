using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsappSender.Headless.Awaiters;
using WhatsappSender.Headless.Awaiters.Args;
using WhatsappSender.Headless.Factories;
using WhatsappSender.Headless.States;

namespace WhatsappSender.Headless.Managers
{
    public class BrowserContextManager : IDisposable
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
        private static readonly ViewPortOptions VIEW_PORT = new ViewPortOptions { Width = 1366, Height = 768 };

        private BrowserContext context;
        private Page whatsapp;

        private bool disposed = false;

        public BrowserContextManager(Browser browser)
        {
            context = NewContext(browser).GetAwaiter().GetResult();
            whatsapp = NewWhatsappPage(context).GetAwaiter().GetResult();
            var devTools = whatsapp.Target.CreateCDPSessionAsync().GetAwaiter().GetResult();
            devTools.SendAsync("Network.enable").GetAwaiter().GetResult();
        }

        public BrowserContextManager(Browser browser, PageState state) : this(browser)
        {
            state.RestoreState(whatsapp).GetAwaiter().GetResult();
        }

        private async Task<BrowserContext> NewContext(Browser browser)
            => await browser.CreateIncognitoBrowserContextAsync();

        private async Task<Page> NewWhatsappPage(BrowserContext context)
        {
            var page = await context.NewPageAsync();
            await page.SetUserAgentAsync(USER_AGENT);
            await page.SetViewportAsync(VIEW_PORT);
            await page.GoToAsync("https://web.whatsapp.com/");
            
            await page.ExposeFunctionAsync("messageReceived", (string text) =>
            {
                Console.WriteLine(text);
                return Task.CompletedTask;
            });

            await page.EvaluateFunctionAsync(@"
() => { 
    var observer = new MutationObserver((mutations) => {
        for(var mutation of mutations) {
            if(mutation.addedNodes.length) {
                for(var node of mutation.addedNodes) {
                    var childIn = node.querySelector('div.message-in');
                    var childOut = node.querySelector('div.message-out');
                    if(childIn) {
                        var messageContainer = childIn.querySelector('div.copyable-text');
                        if(messageContainer) {
                            var nome = '';
                            if(messageContainer.getAttribute('data-pre-plain-text')) {
                                nome = messageContainer.getAttribute('data-pre-plain-text');
                            }
                            var texto = messageContainer.querySelector('span.selectable-text.copyable-text').innerText;
                            messageReceived(nome + texto);
                        }
                    } else if(childOut) {
                        var messageContainer = childOut.querySelector('div.copyable-text');
                       if(messageContainer) {
                            var nome = '';
                            if(messageContainer.getAttribute('data-pre-plain-text')) {
                                nome = messageContainer.getAttribute('data-pre-plain-text');
                            }
                            var texto = messageContainer.querySelector('span.selectable-text.copyable-text').innerText;
                            messageReceived(nome + texto);
                        }
                    }
                }
            }
        }
    });
    for(var node of document.querySelectorAll('div.copyable-area>div[tabindex=\'0\']:nth-child(3)>div:nth-child(3)')) {
        observer.observe(node, { childList: true, attributes: true });
    }
}
");
            /*
            @"
            () => { 
                var observer = new MutationObserver((mutations) => {
                    for(var mutation of mutations) {
                        if(mutation.addedNodes.length) {
                            for(var node in mutation.addedNodes) {
                                if(node.tagName.toUpperCase() === 'DIV' && node.classList.contains('message-in') && node.classList.contains('tail')) {
                                    messageReceived(node.innerHTML);
                                }
                            }
                        }
                    }
                });
                observer.observe(document.querySelector('div#app'), { childList: true, attributes: true });
            }
            " 
            */

            return page;
        }

        private void OnQRCodeChanged(object sender, QrCodeChangedArgs args)
        {
            using (var file = File.Create(Path.Combine(AppContext.BaseDirectory, "qr-code.png")))
            {
                file.Write(args.Image, 0, args.Image.Length);
                file.Flush();
            }
        }

        public void Logon()
        {
            var loginAwaiter = new LoginAwaiter(whatsapp, TimeSpan.FromMinutes(10));
            loginAwaiter.OnQRCodeChange += OnQRCodeChanged;
            loginAwaiter.Wait().GetAwaiter().GetResult();
        }

        public void Screenshot(string file)
            => whatsapp.ScreenshotAsync(file).GetAwaiter().GetResult();

        public void Logoff()
        {
            var logoutAwaiter = new LogoutAwaiter(whatsapp, TimeSpan.FromMinutes(10));
            logoutAwaiter.Wait().GetAwaiter().GetResult();
        }

        public async Task<PageState> GetState()
            => await PageStateFactory.Create(whatsapp);

        public async void Dispose()
        {
            if (disposed)
                return;
            try { await whatsapp.CloseAsync(); } catch { }
            try { whatsapp.Dispose(); } catch { }
            try { await context.CloseAsync(); } catch { }
            disposed = true;
        }
    }
}
