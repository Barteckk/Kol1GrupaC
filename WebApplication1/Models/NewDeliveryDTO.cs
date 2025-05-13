namespace WebApplication1.Models;

public class NewDeliveryDTO
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber { get; set; }
    public List<AddProductsDTO> Products { get; set; }
}

public class AddProductsDTO
{
    public string Name { get; set; }
    public int Amount { get; set; }
}
