using NIC_clinica_project.Models;

namespace NIC_clinica_project.Mappers
{
    public class ClientMapper :  BaseMapper<ClientsDtoWithKeys, ClientsDtoWithValues, Client>
    {
        protected override void MappingValuebleFields(Client entity, ClientsDtoWithValues dto)
        {
            dto.regionNumber = entity.region.number;
        }

    }
}
