using KioskBrains.Server.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KioskBrains.Server.Domain.Entities
{
    public class TokenInfo : BaseEntity<int>
    {
        public string Token { get; set; }
        public TokenType TokenType { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public enum TokenType
    {
        Allegro, Yandex
    }
}
