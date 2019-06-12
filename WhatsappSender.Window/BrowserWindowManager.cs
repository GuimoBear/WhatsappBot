using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading;
using WhatsappSender.Window.Messages;

namespace WhatsappSender.Window
{
    public class BrowserWindowManager : IDisposable
    {
        const string IDENTIFIER_TEXT = "ashfowhmeinyiwyuiqwyvbitybunrciqncyitvgniym";

        private const string QR_CODE_IMAGE = "img[alt=\"Scan me!\"]";
        private const string REFRESH_IMAGE = "span[data-icon=\"refresh-l-light\"]";

        private const string SEND_BUTTON = "span[data-icon=\"send\"]";
        private const string SEND_ATTACHMENT = "span[data-icon=\"send-light\"]";

        private const string ATTACHMENT_BUTTON = "span[data-icon=\"clip\"]";
        private const string IMAGE_DESCRIPTION_DIV = "div.copyable-text.selectable-text[contenteditable=\"true\"]";

        private const string MENU_IMAGE = "div[title=\"Menu\"]>span[data-icon=\"menu\"]";
        private const string SAIR_IMAGE = "div[title=\"Sair\"]";

        private const string NOVA_CONVERSA = "div[title='Nova conversa']";

        private ChromeDriver driver;

        public BrowserWindowManager()
        {
            var options = new ChromeOptions();
            options.BinaryLocation = Path.Combine(AppContext.BaseDirectory, "chromedriver.exe");
            options.AddArgument("--headless");
            driver = new ChromeDriver(AppContext.BaseDirectory, options);
            driver.Navigate().GoToUrl("https://web.whatsapp.com/");
            var version = driver.Capabilities.GetCapability("version");
        }

        public void AguardarAutenticacao()
        {
            var waiter = new WebDriverWait(driver, TimeSpan.FromMinutes(10));
            string oldBase64Image = null;
            var content = waiter.Until(d =>
            {
                try
                {
                    var src = driver.PageSource;
                    var qrCodeImg = driver.FindElement(By.CssSelector(QR_CODE_IMAGE));
                    var base64Image = qrCodeImg.GetAttribute("src");
                    base64Image = base64Image.Remove(0, base64Image.IndexOf(',') + 1);
                    if (!base64Image.Equals(oldBase64Image))
                    {
                        var qrCodeImage = Convert.FromBase64String(base64Image);
                        using (var file = File.Create(Path.Combine(AppContext.BaseDirectory, "qr-code.png")))
                        {
                            file.Write(qrCodeImage, 0, qrCodeImage.Length);
                            file.FlushAsync();
                        }
                    }
                    oldBase64Image = base64Image;
                    return null;
                }
                catch(Exception ex)
                {
                    try
                    {
                        var refreshImg = driver.FindElement(By.CssSelector(REFRESH_IMAGE));
                        refreshImg.Click();
                        return "TIMEOUT";
                    }
                    catch
                    {
                        return null;
                    }
                }
            });
            Thread.Sleep(5000);
        }

        public void Enviar(IMessage message)
            => message.Send(driver);

        public void EnviarImagem(string numero, string descricao, string imagem)
        {
            numero = WebUtility.HtmlEncode("55" + numero);
            var url = $"https://api.whatsapp.com/send?phone={numero}&text={IDENTIFIER_TEXT}";
            var waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            var element = driver.FindElement(By.CssSelector("div#action-button-container a#action-button"));
            element.Click();
            var textElement = waiter.Until(d =>
            {
                try
                {
                    var sendTextMessageBoxElement = driver.FindElement(By.XPath("//*[@id=\"main\"]/footer/div[1]/div[2]/div/div[2]"));
                    var text = sendTextMessageBoxElement.Text;
                    if (text.Equals(IDENTIFIER_TEXT))
                        return sendTextMessageBoxElement;
                    else
                        return null;
                }
                catch
                {
                    try
                    {
                        var sendButtomElement = driver.FindElement(By.CssSelector("div#action-button-container a#action-button"));
                        sendButtomElement.Click();
                    }
                    catch
                    {

                    }
                    return null;
                }
            });
            textElement.Clear();
            driver.FindElement(By.CssSelector(ATTACHMENT_BUTTON)).Click();
            Thread.Sleep(500);
            var input = driver.FindElement(By.CssSelector("input[type=file]"));
            input.SendKeys(imagem);
            Thread.Sleep(1000);
            try
            {
                var attachmentUpload = driver.FindElement(By.CssSelector(SEND_ATTACHMENT));
                attachmentUpload.Click();
                Thread.Sleep(2000);
            }
            catch (NoSuchElementException)
            {
                
            }
            try
            {
                var imageDescriptionText = driver.FindElement(By.CssSelector(IMAGE_DESCRIPTION_DIV));
                imageDescriptionText.SendKeys(descricao);
                imageDescriptionText.SendKeys(Keys.Enter);
                imageDescriptionText.Clear();

            }
            catch (NoSuchElementException)
            {

            }
        }

        public void EnviarMensagem(string numero, string mensagem)
        {
            numero = WebUtility.HtmlEncode("55" + numero);
            var url = $"https://api.whatsapp.com/send?phone={numero}&text={IDENTIFIER_TEXT}";
            var waiter = new WebDriverWait(driver, TimeSpan.FromMinutes(10));
            //driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            var element = driver.FindElement(By.CssSelector("div#action-button-container a#action-button"));
            element.Click();
            var textElement = waiter.Until(d =>
            {
                try
                {
                    var sendTextMessageBoxElement = driver.FindElement(By.XPath("//*[@id=\"main\"]/footer/div[1]/div[2]/div/div[2]"));
                    var text = sendTextMessageBoxElement.Text;
                    if (text.Equals(IDENTIFIER_TEXT))
                        return sendTextMessageBoxElement;
                    else
                        return null;
                }
                catch
                {
                    try
                    {
                        var sendButtomElement = driver.FindElement(By.CssSelector("div#action-button-container a#action-button"));
                        sendButtomElement.Click();
                    }
                    catch
                    {

                    }
                    return null;
                }
            });
            textElement.Clear();
            textElement.SendKeys(mensagem);
            textElement.SendKeys(Keys.Enter);
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public void Logoff()
        {
            var element = driver.FindElement(By.CssSelector(MENU_IMAGE));
            element.Click();
            Thread.Sleep(500);
            element = driver.FindElement(By.CssSelector(SAIR_IMAGE));
            element.Click();
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }

        public void Dispose()
        {
            driver.Close();
            driver.Dispose();
        }
    }
}
