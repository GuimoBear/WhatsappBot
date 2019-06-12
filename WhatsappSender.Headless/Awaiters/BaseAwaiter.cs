using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WhatsappSender.Headless.Awaiters
{
    public abstract class BaseAwaiter
    {
        protected Page page;
        protected TimeSpan timeout;

        public BaseAwaiter(Page page, TimeSpan timeout)
        {
            this.page = page;
            this.timeout = timeout;
        }

        protected abstract Task RunAsync();

        public async Task Wait()
            => await RunAsync();

    }
}
