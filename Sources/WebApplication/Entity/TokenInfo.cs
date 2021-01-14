using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities.Common;

namespace WebApplication.Entity
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
