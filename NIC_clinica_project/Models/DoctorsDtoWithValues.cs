namespace NIC_clinica_project.Models
{
    public class DoctorsDtoWithValues
    {
        public int id { get; set; }
        public string fio { get; set; } = string.Empty;
        public int cabinetNumber { get; set; }
        public string specializationName { get; set; } = string.Empty;
        public int? regionNumber { get; set; }
    }
}
