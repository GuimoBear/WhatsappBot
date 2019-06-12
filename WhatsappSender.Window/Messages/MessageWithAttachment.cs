using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace WhatsappSender.Window.Messages
{
    public class Message : IMessage
    {
        const string IDENTIFIER_TEXT = "ashfowhmeinyiwyuiqwyvbitybunrciqncyitvgniym";

        private const string ATTACHMENT_BUTTON = "span[data-icon=\"clip\"]";
        private const string SEND_ATTACHMENT = "span[data-icon=\"send-light\"]";
        private const string IMAGE_DESCRIPTION_DIV = "div.copyable-text.selectable-text[contenteditable=\"true\"]";

        private ICollection<KeyValuePair<string, string>> phones;
        private ICollection<string> messages;
        private ICollection<KeyValuePair<string, string>> attachments;

        protected void AddReceiver(string phone, string description = null)
        {
            phone = ApenasNumeros(phone);
            if (string.IsNullOrEmpty(phone))
                return;
            if (phone.Length == 12 && !phone.StartsWith("55"))
                phone = "55" + phone;
            if (phone.Length < 13)
                return;
            phones = phones ?? new List<KeyValuePair<string, string>>();
            phones.Add(new KeyValuePair<string, string>(phone, description));
        }

        protected void AddMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            messages = messages ?? new List<string>();
            messages.Add(message);
        }

        protected void AddAttachment(string attachment, string description = "")
        {
            if (string.IsNullOrEmpty(attachment))
                return;
            if(File.Exists(attachment))
            {
                attachments = attachments ?? new List<KeyValuePair<string, string>>();
                attachments.Add(new KeyValuePair<string, string>(attachment, description));
            }
        }

        private IWebElement OpenContactWindow(IWebDriver driver, string phone)
        {
            phone = WebUtility.HtmlEncode(phone);
            var url = $"https://api.whatsapp.com/send?phone={phone}&text={IDENTIFIER_TEXT}";
            var waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            driver.Navigate().GoToUrl(url);
            //Thread.Sleep(TimeSpan.FromSeconds(2));
            var btnConfirSendMessage = waiter.Until(_ =>
            {
                try
                {
                    return driver.FindElement(By.CssSelector("div#action-button-container a#action-button"));
                }
                catch
                {
                    return null;
                }
            });
            btnConfirSendMessage.Click();
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
                    return null;
                    /*
                    try
                    {
                        var sendButtomElement = driver.FindElement(By.CssSelector("div#action-button-container a#action-button"));
                        sendButtomElement.Click();
                    }
                    catch
                    {

                    }
                    return null;
                    */
                }
            });
            textElement.Clear();
            return textElement;
        }

        private void SendAttachment(IWebDriver driver, IWebElement textElement, string file, string description)
        {
            driver.FindElement(By.CssSelector(ATTACHMENT_BUTTON)).Click();
            Thread.Sleep(500);
            var input = driver.FindElement(By.CssSelector("input[type=file]"));
            input.SendKeys(file);
            Thread.Sleep(1000);
            try
            {
                var attachmentUpload = driver.FindElement(By.CssSelector(SEND_ATTACHMENT));
                attachmentUpload.Click();
                Thread.Sleep(2000);
            }
            catch
            {

            }
            if (string.IsNullOrEmpty(description))
                return;
            textElement.SendKeys(description);
            textElement.SendKeys(Keys.Enter);
            textElement.Clear();
        }

        private void SendMessage(IWebElement textElement, string message)
        {
            textElement.SendKeys(message);
            textElement.SendKeys(Keys.Enter);
            Thread.Sleep(500);
        }

        public void Send(IWebDriver driver)
        {
            if (phones is null)
                return;
            var hasMessages = !(messages is null) && messages.Count > 0;
            if (!hasMessages)
                return;
            var hasAttachment = !(attachments is null) && attachments.Count > 0;
            foreach (var phone in phones)
            {
                var textElement = OpenContactWindow(driver, phone.Key);
                if (!string.IsNullOrEmpty(phone.Value))
                    SendMessage(textElement, "Ola " + phone.Value);
                if(hasAttachment)
                {
                    foreach(var attachment in attachments)
                    {
                        SendAttachment(driver, textElement, attachment.Key, attachment.Value);
                    }
                }
                foreach(var message in messages)
                {
                    SendMessage(textElement, message);
                }
            }
        }

        public void Dispose()
        {

        }

        private string ApenasNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;
            var result = "";
            foreach (var c in valor)
            {
                if (c >= '0' && c <= '9')
                    result += c;
            }
            return result;
        }
    }

    public class MessageBuilder : Message
    {
        private MessageBuilder() : base() { }

        public MessageBuilder WithReceiver(string phone, string name)
        {
            AddReceiver(phone, name);
            return this;
        }

        public MessageBuilder WithAttachment(string filename, string description)
        {
            AddAttachment(filename, description);
            return this;
        }

        public MessageBuilder WithMessage(string message)
        {
            AddMessage(message);
            return this;
        }

        public Message Build()
            => this;

        public static MessageBuilder Make()
            => new MessageBuilder();
    }
}
