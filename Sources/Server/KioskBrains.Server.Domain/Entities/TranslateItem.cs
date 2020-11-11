using KioskBrains.Server.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace KioskBrains.Server.Domain.Entities
{
    public class TranslateItem : BaseEntityManualId<string> 
    {
        public string TextRu { get; set; }
        public bool Translated { get; set; }
    }
}
