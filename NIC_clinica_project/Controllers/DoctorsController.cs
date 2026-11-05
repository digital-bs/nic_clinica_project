using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NIC_clinica_project.Models;
using NIC_clinica_project.Repositories;

namespace NIC_clinica_project.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    public class DoctorsController : CrudControllerBase<DoctorsDtoWithKeys, DoctorsDtoWithValues>
    {

        public DoctorsController(ICrudRepository<DoctorsDtoWithKeys, DoctorsDtoWithValues> repository) : base(repository)
        {

        }

        protected override string ErrorDataMessage => " Данные Врача некорректны.";


    }
}
