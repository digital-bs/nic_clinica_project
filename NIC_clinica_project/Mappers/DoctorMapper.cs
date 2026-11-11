using NIC_clinica_project.Models;

namespace NIC_clinica_project.Mappers
{
    public class DoctorMapper : BaseMapper<DoctorsDtoWithKeys, DoctorsDtoWithValues, Doctor>
    {
        protected override void MappingValuebleFields(Doctor entity, DoctorsDtoWithValues dto)
        {
            dto.cabinetNumber = entity.cabinet.number;
            dto.specializationName = entity.specialization.name;
            dto.regionNumber = entity.region?.number;
        }
    }
}
