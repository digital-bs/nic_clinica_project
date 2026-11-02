namespace NIC_clinica_project.Models
{
    public class ClientsDtoWithValues
    {
        public int id { get; set; }
        public string? firstName { get; set; } = string.Empty;
        public string? lastName { get; set; } = string.Empty;
        public string? otchestvo { get; set; } = string.Empty;
        public string? adress { get; set; } = string.Empty;
        public DateTime birthday { get; set; }
        public string? gender { get; set; } = string.Empty;
        public int regionNumber { get; set; }

    }
}
