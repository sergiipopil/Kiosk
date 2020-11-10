using System;

namespace KioskApp.Clients.NovaPoshtaUkraine
{
    public class NovaPoshtaUkraineClientException : Exception
    {
        public NovaPoshtaUkraineClientException(string message)
            : base(message)
        {
        }

        public NovaPoshtaUkraineClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}