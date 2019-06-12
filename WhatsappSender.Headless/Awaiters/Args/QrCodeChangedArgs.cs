using System;

namespace WhatsappSender.Headless.Awaiters.Args
{
    public class QrCodeChangedArgs : EventArgs
    {
        public byte[] Image { get; }

        public QrCodeChangedArgs(byte[] image)
        {
            Image = image;
        }
    }
}
