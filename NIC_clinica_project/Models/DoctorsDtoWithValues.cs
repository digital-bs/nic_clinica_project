namespace NIC_clinica_project.Models
{
    public class DoctorsDtoWithValues
    {
        public int id { get; set; }
        public string fio { get; set; } = string.Empty;
        public string cabinetNumber { get; set; } = string.Empty;
        public string specializationName { get; set; } = string.Empty;
        public int? regionNumber { get; set; }
    }
}
