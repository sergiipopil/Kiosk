using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Server.Common.Services;

namespace KioskBrains.Server.EK.Integration.Managers
{
    public interface IEkIntegrationManager
    {
        Task<EmptyData> ApplyUpdatesAsync(Update[] updates, IIntegrationLogManager integrationLogManager);

        Task<Kiosk> GetKioskAsync(int kioskId);

        Task<Order> GetOrderAsync(int orderId);
    }
}