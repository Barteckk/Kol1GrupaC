using WebApplication1.Models;

namespace WebApplication1.Services;
using Microsoft.Data.SqlClient;

public class DeliveryService : IDeliveryService
{
    private readonly IConfiguration _configuration;

    public DeliveryService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesDeliveryExist(int id)
    {
        var query = "SELECT 1 FROM Delivery WHERE delivery_id = @id";
        
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        
        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }
    
    public async Task<bool> DoesClientExist(int id)
    {
        var query = "SELECT 1 FROM Customer WHERE customer_id = @id";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }

    public async Task<bool> DoesDriverExist(string licence)
    {
        var query = "SELECT 1 FROM Driver WHERE licence_number = @licence";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@licence", licence);

        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }

    public async Task<bool> DoesProductExist(string name)
    {
        var query = "SELECT 1 FROM Product WHERE name = @name";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@name", name);

        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }

    public async Task<DeliveryDTO> GetDelivery(int id)
    {
        var query = @"SELECT 
                       a.date AS DeliveryDate, 
                       c.first_name AS CustomerFirstName,
                       c.last_name AS CustomerLastName,
                       c.date_of_birth AS CustomerDOB,
                       d.first_name AS DriverFirstName,
                       d.last_name AS DriverLastName,
                       d.licence_number AS DriverLicenseNumber,
                       p.name AS ProductName,
                       p.price AS ProductPrice,
                       ap.amount AS ProductAmount
                       FROM Delivery a
                       JOIN Customer c ON a.customer_id = c.customer_id
                       JOIN Driver d ON a.driver_id = d.driver_id
                       JOIN Product_Delivery ap ON a.delivery_id = ap.delivery_id
                       JOIN Product p ON ap.product_id = p.product_id
                       WHERE a.delivery_id = @id";
        
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        
        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        
        DeliveryDTO? dto = null;

        while (await reader.ReadAsync())
        {
            if (dto is null)
            {
                dto = new DeliveryDTO()
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                    Customer = new CustomerDTO()
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("CustomerFirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("CustomerLastName")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("CustomerDOB"))
                    },
                    Driver = new DriverDTO()
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("DriverFirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("DriverLastName")),
                        LicenceNumber = reader.GetString(reader.GetOrdinal("DriverLicenseNumber")),
                    },
                    Products = new List<ProductsDTO>()
                };
            }

            dto.Products.Add(new ProductsDTO()
            {
                Name = reader.GetString(reader.GetOrdinal("ProductName")),
                Price = reader.GetDecimal(reader.GetOrdinal("ProductPrice")),
                Amount = reader.GetInt32(reader.GetOrdinal("ProductAmount"))
            });
        }

        if (dto is null)
            throw new Exception("Delivery not found");
        
        return dto;
    }
    
    public async Task<int> AddDelivery(NewDeliveryDTO newDelivery)
    {
        string command = "Select 1 from Appointment where delivery_id = @DeliveryId";
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@DeliveryId", newDelivery.DeliveryId);

            conn.Open();
            var id = cmd.ExecuteScalar();
            if (id is not null)
                return -1;
        }

        command = "Select 1 from Customer where customer_id = @CustomerId";
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@CustomerId", newDelivery.CustomerId);

            conn.Open();
            var id = cmd.ExecuteScalar();
            if (id is null)
                return -2;
        }
        
        command = "Select 1 from Driver where licence_number = @LicenceNamber";
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@LicenceNamber", newDelivery.LicenceNumber);

            conn.Open();
            var id = cmd.ExecuteScalar();
            if (id is null)
                return -2;
        }
        
        command = "Select name from Product where product_id = @ProductId";
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            int good = 0;

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (newDelivery.Products.Select(x=>x.Name).Contains(reader.GetString(0)))
                    {
                        good++;
                    }

                    if (newDelivery.Products.Select(x=>x.Name).Contains(reader.GetString(1)))
                    {
                        good++;
                    }
                }
            }
            if (good != newDelivery.Products.Count)
                return -3;
        }
        
        command = "Insert into Delivery (delivery_id, customer_id, licence_number) values (@DeliveryId, @CustomerId, @LicenceNumber, (SELECT licence_number FROM Driver WHERE licence_number = @LicenceNumber))";
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@DeliveryId", newDelivery.DeliveryId);
            cmd.Parameters.AddWithValue("@CustomerId", newDelivery.CustomerId);
            cmd.Parameters.AddWithValue("@LicenceNumber", newDelivery.LicenceNumber);

            conn.Open();
            var id = cmd.ExecuteScalar();
            if (id is not null)
                return -4;
        }

        return 1;
    }
}