using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveryController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }
        
        [HttpGet("{id}")]
        public IActionResult GetDelivery(int id)
        {
            if (!_deliveryService.DoesDeliveryExist(id).Result)
                return NotFound($"Delivery with id {id} not found");
            var res = _deliveryService.GetDelivery(id);
            return Ok(res.Result);
        }
        
        [HttpPost]
        public IActionResult AddDelivery([FromBody] NewDeliveryDTO? newDelivery)
        {
            if (newDelivery is null || newDelivery.DeliveryId == null)
                return BadRequest("Invalid delivery data.");


            var createdAppointment = _deliveryService.AddDelivery(newDelivery);
            if (createdAppointment.Result == -1)
                return BadRequest("Delivery already exists.");
            else if (createdAppointment.Result == -2)
                return NotFound("Customer or driver not found.");
            else if (createdAppointment.Result == -3)
                return NotFound("Product not found.");
            else if (createdAppointment.Result == -4)
                return BadRequest();
            
            if (createdAppointment.Result == 1)
                return Created(nameof(GetDelivery), "Created appointment");

            return BadRequest("No");
        }
    }
}
