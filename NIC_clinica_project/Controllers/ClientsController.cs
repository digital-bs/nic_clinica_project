using Microsoft.AspNetCore.Mvc;
using NIC_clinica_project.Models;
using NIC_clinica_project.Repositories;

namespace NIC_clinica_project.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : CrudControllerBase<ClientsDtoWithKeys, ClientsDtoWithValues>
    {

        public ClientsController(ICrudRepository<ClientsDtoWithKeys, ClientsDtoWithValues> repository) : base(repository)
        { 
        
        }
        protected override string ErrorDataMessage => "Данные пациента некорректны";



    }
}
