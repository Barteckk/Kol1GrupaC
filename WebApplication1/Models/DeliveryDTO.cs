namespace WebApplication1.Models;

public class DeliveryDTO
{
    public DateTime Date { get; set; }
    public CustomerDTO Customer { get; set; }
    public DriverDTO Driver { get; set; }
    public List<ProductsDTO> Products { get; set; }
    
}

public class CustomerDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class DriverDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenceNumber { get; set; }
}

public class ProductsDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
    
}