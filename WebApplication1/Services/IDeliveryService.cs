using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IDeliveryService
{
    Task<bool> DoesDeliveryExist(int id);
    Task<bool> DoesClientExist(int id);
    Task<bool> DoesDriverExist(string licenseNumber);
    Task<bool> DoesProductExist(string name);
    Task<DeliveryDTO> GetDelivery(int id);
    Task<int> AddDelivery(NewDeliveryDTO delivery);
}