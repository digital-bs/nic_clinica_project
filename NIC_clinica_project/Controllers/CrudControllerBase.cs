using Microsoft.AspNetCore.Mvc;
using NIC_clinica_project.Models;
using NIC_clinica_project.Repositories;

namespace NIC_clinica_project.Controllers
{
    public abstract class CrudControllerBase<TEditDto, TListDto> : ControllerBase
        where TEditDto : class
        where TListDto : class
    {
        private readonly ICrudRepository<TEditDto, TListDto> _repository;

        protected virtual string ErrorDataMessage => "Данные некорректны .";

        protected CrudControllerBase(ICrudRepository<TEditDto, TListDto> repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderParam = "id")
        {
            if (!_repository.IsSortable(orderParam))
            {
                return BadRequest(new { message = "Недопустимое поле сортировки" });
            }
            var items = await _repository.GetAll(page, pageSize, orderParam);
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetById(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TEditDto item)
        {
            if (!await _repository.IsValid(item))
            {
                return BadRequest(new { message = ErrorDataMessage });
            }

            var id = await _repository.Create(item);
            var createdItem = await _repository.GetById(id);
            return CreatedAtAction(nameof(GetById), new { id }, createdItem);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TEditDto item)
        {
            if (!await _repository.IsValid(item))
            {
                return BadRequest(new { message = ErrorDataMessage });
            }
            var isUpdated = await _repository.Update(item);
            if (!isUpdated) return NotFound();

            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _repository.Delete(id);
            if (!isDeleted) return NotFound();

            return NoContent();
        }

    }
}
