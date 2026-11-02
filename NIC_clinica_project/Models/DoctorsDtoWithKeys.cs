namespace NIC_clinica_project.Models
{
    public class DoctorsDtoWithKeys
    {
        public int id { get; set; }
        public string fio {  get; set; } = string.Empty;
        public int cabinetId { get; set; }
        public int specializationId { get; set; }
        public int? regionId { get; set; }
    }
}
