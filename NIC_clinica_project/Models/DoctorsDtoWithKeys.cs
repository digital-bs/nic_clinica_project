namespace NIC_clinica_project.Models
{
    public class DoctorsDtoWithKeys : DoctorDto
    {
        public int cabinetId { get; set; }
        public int specializationId { get; set; }
        public int? regionId { get; set; }
    }
}
