using System;

namespace KioskApp.Controls
{
    public class MenuPointClickedEventArgs : EventArgs
    {
        public string Id { get; }

        public MenuPointClickedEventArgs(string id)
        {
            Id = id;
        }
    }
}