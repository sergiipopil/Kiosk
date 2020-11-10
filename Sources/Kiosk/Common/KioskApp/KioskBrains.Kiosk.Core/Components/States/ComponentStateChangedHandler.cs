using System.Threading.Tasks;

namespace KioskBrains.Kiosk.Core.Components.States
{
    public delegate Task ComponentStateChangedHandler(string propertyName);
}