using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsappSender.Window.Messages
{
    public interface IMessage : IDisposable
    {
        void Send(IWebDriver driver);
    }
}
