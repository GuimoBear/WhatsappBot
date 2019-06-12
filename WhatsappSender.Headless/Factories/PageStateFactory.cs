using Newtonsoft.Json;
using PuppeteerSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhatsappSender.Headless.States;

namespace WhatsappSender.Headless.Factories
{
    public class PageStateFactory
    {
        public static async Task<PageState> Create(Page page)
        {
            var localStorage = await page.EvaluateExpressionAsync<IDictionary<string, string>>("(() => { var storage = {}; for(var i = 0; i < localStorage.length; i++) { storage[localStorage.key(i)] = localStorage.getItem(localStorage.key(i)); } return storage; })()");
            var cookies = (await page.GetCookiesAsync("https://web.whatsapp.com")).Select(cp => new Cookie(cp)).ToList();
            return new PageState(localStorage, cookies);
        }
    }
}
