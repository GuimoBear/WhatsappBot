using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WhatsappSender.Headless.Awaiters.Args;
using WhatsappSender.Headless.Managers;
using WhatsappSender.Headless.States;
using WhatsappSender.Headless.Utils;

namespace WhatsappSender.Headless
{
    public class HeadlessBrowserManager : IDisposable
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
        private static readonly ViewPortOptions VIEW_PORT = new ViewPortOptions { Width = 1366, Height = 768 };

        private Browser browser;

        private IDictionary<string, BrowserContextManager> managers;

        public BrowserContextManager this[string token]
        {
            get
            {
                if (managers.TryGetValue(token, out var manager))
                    return manager;
                throw new KeyNotFoundException("Não existe uma instancia do Whatsapp aberta para este token");
            }
        }

        public HeadlessBrowserManager()
        {
            InitializePuppeteer().GetAwaiter().GetResult();
            managers = CreateManagers();
        }

        private async Task InitializePuppeteer()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                LogProcess = false
            });
        }

        private IDictionary<string, BrowserContextManager> CreateManagers()
        {
            var managers = new Dictionary<string, BrowserContextManager>();
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "state.tmp")))
            {
                using (var fileStream = File.Open(Path.Combine(AppContext.BaseDirectory, "state.tmp"), FileMode.Open))
                {
                    var states = ProtoSerialize.Deserialize<IDictionary<string, PageState>>(fileStream);
                    foreach (var state in states)
                    {
                        var browserContextManager = new BrowserContextManager(browser, state.Value);
                        managers.Add(state.Key, browserContextManager);
                    }
                }
            }
            return managers;
        }      

        public bool TryGetManager(string token, out BrowserContextManager browserContextManager)
            => managers.TryGetValue(token, out browserContextManager);

        public BrowserContextManager CreateManager(string token, PageState state = null)
        {
            if (managers.ContainsKey(token))
                throw new ArgumentException("O token informado já existe");
            var browserContextManager = state is null ? new BrowserContextManager(browser) : new BrowserContextManager(browser, state);
            managers.Add(token, browserContextManager);
            return browserContextManager;
        }

        public void Dispose()
        {
            if(managers.Count > 0)
            {
                IDictionary<string, PageState> states = new Dictionary<string, PageState>();
                foreach (var kvp in managers)
                {
                    states.Add(kvp.Key, kvp.Value.GetState().GetAwaiter().GetResult());
                    //kvp.Value.Logoff(); //Comentar esta linha caso queira que o estado da sessão seja salvo
                    kvp.Value.Dispose();
                }
                using (var file = File.Create(Path.Combine(AppContext.BaseDirectory, "state.tmp"), 4096, FileOptions.None))
                {                    
                    ProtoSerialize.Serialize(states, file);
                    file.Flush();
                }
            }
            else if (File.Exists(Path.Combine(AppContext.BaseDirectory, "state.tmp")))
                File.Delete(Path.Combine(AppContext.BaseDirectory, "state.tmp"));
            try
            {
                browser.CloseAsync().GetAwaiter().GetResult();
            }
            catch { }
            try
            {
                browser.Dispose();
            }
            catch { }
        }
    }
}
