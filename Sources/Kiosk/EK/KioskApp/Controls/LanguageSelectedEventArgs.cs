using System;
using Windows.Globalization;

namespace KioskApp.Controls
{
    public class LanguageSelectedEventArgs : EventArgs
    {
        public Language Language { get; }

        public LanguageSelectedEventArgs(Language language)
        {
            Language = language;
        }
    }
}