using Microsoft.AspNetCore.Mvc;
using NIC_clinica_project.Models;
using NIC_clinica_project.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NIC_clinica_project.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private ClientsRepository _repository;
        private int _page = 3;
        private int _pageSize = 5;

        public ClientsController (ClientsRepository repository) => _repository = repository;




        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var clients = await _repository.GetAllClientsWithValues();
        //    return Ok(clients);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderParam = "id")
        {
            if (page < 1 || pageSize < 1 || (orderParam!= "id" && orderParam != "firstName" && orderParam != "lastName" && orderParam != "otchestvo" && 
                orderParam != "adress" && orderParam != "birthday" && orderParam != "gender" &&orderParam != "region"))
            {
                return BadRequest();
            }

            var clients = await _repository.GetAllClientsWithValues(page, pageSize, orderParam);
            return Ok(clients);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _repository.GetClientById(id);
            if (client == null)
                return NotFound();

            return Ok(client);

        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientsDtoWithKeys client)
        {
            client.id = await _repository.CreateClient(client);

            return CreatedAtAction(nameof(GetById), new { client.id }, client);

        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ClientsDtoWithKeys client)
        {
            var isUpdated = await _repository.UpdateClient(client);
            if (!isUpdated) return NotFound();

            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _repository.DeleteClient(id);
            if(!isDeleted) return NotFound();

            return NoContent();
        }

    }
}
