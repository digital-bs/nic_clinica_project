namespace NIC_clinica_project.Models
{
    public class DoctorsDtoWithKeys : DoctorDto
    {
        public int cabinetID { get; set; }
        public int specializationID { get; set; }
        public int? regionId { get; set; }
    }
}
