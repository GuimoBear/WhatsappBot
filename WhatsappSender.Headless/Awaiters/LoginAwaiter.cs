using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhatsappSender.Headless.Awaiters.Args;

namespace WhatsappSender.Headless.Awaiters
{
    public sealed class LoginAwaiter : BaseAwaiter
    {
        private const string QR_CODE_IMAGE = "img[alt=\"Scan me!\"]";
        private const string REMEMBER_ME_CHECKBOK = "input[name=\"rememberMe\"][type=\"checkbox\"]";
        private const string REFRESH_IMAGE = "span[data-icon=\"refresh-l-light\"]";

        public LoginAwaiter(Page page, TimeSpan timeout) : base(page, timeout) { }

        public event EventHandler<QrCodeChangedArgs> OnQRCodeChange;

        protected override async Task RunAsync()
        {
            byte[] oldBase64QRCode = new byte[0];
            var startDate = DateTime.Now;
            var rememberMeCheckbox = await page.QuerySelectorAsync(REMEMBER_ME_CHECKBOK);
            if (rememberMeCheckbox is null)
                return;
            var isChecked = (bool)(await (await rememberMeCheckbox.GetPropertyAsync("checked")).JsonValueAsync());
            if (!isChecked)
                await rememberMeCheckbox.ClickAsync();
            do
            {
                var qrCodeImage = await page.QuerySelectorAsync(QR_CODE_IMAGE);
                if (qrCodeImage is null)
                {
                    var refreshImage = await page.QuerySelectorAsync(REFRESH_IMAGE);
                    if (!(refreshImage is null))
                        await refreshImage.ClickAsync();
                    else
                        break;
                    continue;
                }
                var srcQrCode = await qrCodeImage.ScreenshotDataAsync();
                bool isEqual = Enumerable.SequenceEqual(srcQrCode, oldBase64QRCode);
                oldBase64QRCode = srcQrCode;
                if (!isEqual)
                {
                    OnQRCodeChange.Invoke(this, new QrCodeChangedArgs(srcQrCode));
                }
            }
            while (DateTime.Now.Subtract(startDate) < timeout);
            Thread.Sleep(1000);
        }
    }
}
