using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine
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
