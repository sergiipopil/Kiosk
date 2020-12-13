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
        public bool? IsInitial { get; set; }
        public bool? IsUsedForName { get; set; }
        public bool? IsUsedForDescription { get; set; }
        public bool? IsUsedForParameter { get; set; }

        public string TermPart { get; set; }
        public string TranslatePart { get; set; }
        public int? Length { get; set; }
    }
}
