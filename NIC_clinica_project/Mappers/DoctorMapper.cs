using NIC_clinica_project.Models;

namespace NIC_clinica_project.Mappers
{
    public static class DoctorMapper
    {
        private static TDto BaseMapping<TDto> (Doctor doctorFrom, TDto doctorTo)
            where TDto : DoctorDto
        {
            doctorTo.id = doctorFrom.id;
            doctorTo.fio = doctorFrom.fio;
            return doctorTo;

        }

        public static DoctorsDtoWithKeys ConvertToDtoWithKeys(this Doctor doctor)
        {
            var newDto = BaseMapping(doctor, new DoctorsDtoWithKeys());
            newDto.cabinetId = doctor.cabinetID;
            newDto.specializationId = doctor.specializationID;
            newDto.regionId = doctor.regionId;
            return newDto;

        }

        public static DoctorsDtoWithValues ConvertToDtoWithValues(this Doctor doctor)
        {
             var newDto = BaseMapping(doctor, new DoctorsDtoWithValues());
            newDto.specializationName = doctor.specialization.name;
            newDto.cabinetNumber = doctor.cabinet.number;
            newDto.regionNumber = doctor.region?.number;

            return newDto;

        }

        public static void ConvertToEFEntity(this DoctorsDtoWithKeys dto, Doctor doctor)
        {

            doctor.fio = dto.fio;
            doctor.cabinetID = dto.cabinetId;

            doctor.specializationID = dto.specializationId;
            doctor.regionId = dto.regionId;
        }

        public static Doctor ToEFEntity(this DoctorsDtoWithKeys dto)
        {
            var doctor = new Doctor();
            dto.ConvertToEFEntity(doctor);
            return doctor;
        }
    }
}
