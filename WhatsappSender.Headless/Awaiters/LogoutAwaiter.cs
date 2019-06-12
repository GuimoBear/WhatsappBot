using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WhatsappSender.Headless.Awaiters
{
    public class LogoutAwaiter : BaseAwaiter
    {
        private const string MENU_IMAGE = "div[title=\"Menu\"]>span[data-icon=\"menu\"]";
        private const string LOGOUT_IMAGE = "div[title=\"Sair\"]";

        public LogoutAwaiter(Page page, TimeSpan timeout) : base(page, timeout) { }

        protected override async Task RunAsync()
        {
            var startDate = DateTime.Now;
            do
            {
                var menuElement = await page.QuerySelectorAsync(MENU_IMAGE);
                if(!(menuElement is null))
                {
                    await menuElement.ClickAsync();
                    var logoutElement = await page.QuerySelectorAsync(LOGOUT_IMAGE);
                    if (!(logoutElement is null))
                    {
                        await logoutElement.ClickAsync();
                        break;
                    }
                }
            }
            while (DateTime.Now.Subtract(startDate) < timeout);
            Thread.Sleep(1000);
        }
    }
}
