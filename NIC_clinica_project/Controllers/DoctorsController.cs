using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NIC_clinica_project.Models;
using NIC_clinica_project.Repositories;

namespace NIC_clinica_project.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private DoctorsRepository _repository;

        public DoctorsController(DoctorsRepository repository) => _repository = repository;


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderParam = "id")
        {
            if (page < 1 || pageSize < 1 || (orderParam != "id" && orderParam != "fio" && orderParam != "cabinet" && orderParam != "spec" &&  orderParam != "region"))
            {
                return BadRequest();
            }

            var doctors = await _repository.GetAllDoctorsWithValues(page, pageSize, orderParam);
            return Ok(doctors);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _repository.GetDoctorById(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);

        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorsDtoWithKeys doctor)
        {
            doctor.id = await _repository.CreateDoctor(doctor);

            return CreatedAtAction(nameof(GetById), new { doctor.id }, doctor);

        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] DoctorsDtoWithKeys doctor)
        {
            var isUpdated = await _repository.UpdateDoctor(doctor);
            if (!isUpdated) return NotFound();

            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _repository.DeleteDoctor(id);
            if (!isDeleted) return NotFound();

            return NoContent();
        }

    }
}
