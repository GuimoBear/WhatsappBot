using Newtonsoft.Json;
using ProtoBuf;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatsappSender.Headless.States
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class PageState
    {
        [ProtoMember(1), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> LocalStorage { get; private set; }
        [ProtoMember(2), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Cookie> Cookies { get; private set; }

        [JsonConstructor]
        private PageState() { }

        public PageState(IDictionary<string, string> localStorage, IEnumerable<Cookie> cookies)
        {
            LocalStorage = localStorage;
            Cookies = cookies;
        }

        public async Task RestoreState(Page page)
        {
            await page.EvaluateExpressionAsync("localStorage.clear();");
            if(!(LocalStorage is null))
                foreach (var kvp in LocalStorage)
                    await page.EvaluateExpressionAsync($"localStorage.setItem('{kvp.Key}', '{kvp.Value}');");

            if (!(Cookies is null))
                foreach (var cookie in Cookies)
                    await page.SetCookieAsync((CookieParam)cookie);
            await page.ReloadAsync();
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
        }
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public class Cookie
    {
        [ProtoMember(1)]
        public string Name { get; private set; }
        [ProtoMember(2)]
        public string Value { get; private set; }
        [ProtoMember(3), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Domain { get; private set; }
        [ProtoMember(4), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; private set; }
        [ProtoMember(5), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; private set; }
        [ProtoMember(6), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? Expires { get; private set; }
        [ProtoMember(7), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Size { get; private set; }
        [ProtoMember(8), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HttpOnly { get; private set; }
        [ProtoMember(9), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Secure { get; private set; }
        [ProtoMember(10), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Session { get; private set; }
        [ProtoMember(11), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SameSite? SameSite { get; private set; }

        [JsonConstructor]
        private Cookie() { }

        public Cookie(CookieParam cookie)
        {
            Name = cookie.Name;
            Value = cookie.Value;
            Domain = cookie.Domain;
            Url = cookie.Url;
            Path = cookie.Path;
            Expires = cookie.Expires;
            Size = cookie.Size;
            HttpOnly = cookie.HttpOnly;
            Secure = cookie.Secure;
            Session = cookie.Session;
            SameSite = cookie.SameSite;
        }

        public static explicit operator CookieParam(Cookie cookie)
        {
            return new CookieParam
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Url = cookie.Url,
                Path = cookie.Path,
                Expires = cookie.Expires,
                Size = cookie.Size,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                Session = cookie.Session,
                SameSite = cookie.SameSite
            };
        }
    }
}
