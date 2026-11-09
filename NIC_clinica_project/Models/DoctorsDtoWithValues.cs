namespace NIC_clinica_project.Models
{
    public class DoctorsDtoWithValues : DoctorDto
    {

        public string cabinetNumber { get; set; } = string.Empty;
        public string specializationName { get; set; } = string.Empty;
        public int? regionNumber { get; set; }
    }
}
