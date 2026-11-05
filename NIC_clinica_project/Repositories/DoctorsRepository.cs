using Microsoft.Data.SqlClient;
using NIC_clinica_project.Models;
using System.Data;
using System.Numerics;

namespace NIC_clinica_project.Repositories
{
    public class DoctorsRepository : SortableRepository, ICrudRepository<DoctorsDtoWithKeys, DoctorsDtoWithValues>
    {
        private string _connectionString;
        const int defaultPageSize = 10;
        const int maxPageSize = 100;
        private static readonly IReadOnlyDictionary<string, string> DoctorOrderByValues = new Dictionary<string, string>()
        {
            ["id"] = "d.id",
            ["fio"] = "d.fio",
            ["cabinet"] = "c.number",
            ["spec"] = "s.name",
            ["region"] = "r.number"
        };
        
        public DoctorsRepository(IConfiguration configuration) : base(DoctorOrderByValues)
        {
            _connectionString = configuration.GetConnectionString("ClinicaDb");

        }

        public async Task<bool> IsValid(DoctorsDtoWithKeys doctor)
        {
            if (doctor is null || string.IsNullOrWhiteSpace(doctor.fio) || doctor.fio.Length > 200 || doctor.cabinetId <= 0 || doctor.specializationId <= 0 || (doctor.regionId is not null && doctor.regionId <= 0))
                return false;

            const string sql = """
                    select convert(bit,
                        case
                            when exists (select 1 from dbo.Cabinets where id = @cabinetId)
                             and EXISTS (SELECT 1 from dbo.Specializations where id = @specializationId)
                             and (@regionId is null or exists (SELECT 1 from dbo.Regions where id = @regionId))
                            then 1
                            else 0
                        end
                    );
        """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@cabinetId", SqlDbType.Int).Value = doctor.cabinetId;
                    command.Parameters.Add("@specializationId", SqlDbType.Int).Value = doctor.specializationId;
                    var regionParameter = command.Parameters.Add("@regionId", SqlDbType.Int);
                    regionParameter.Value = doctor.regionId.HasValue ? doctor.regionId.Value : DBNull.Value;
                    var result = await command.ExecuteScalarAsync();

                    return result is true;
                }
            }
            
        }
        
        public async Task<List<DoctorsDtoWithValues>> GetAll(int page, int pageSize, string orderParam)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = defaultPageSize;
            else if (pageSize > maxPageSize)
                pageSize = maxPageSize;


            var doctors = new List<DoctorsDtoWithValues>();
            var orderByValue = GetOrderByValue(orderParam);

            string sql = $"""
                select d.id,d.fio, c.number as cabinetNumber,s.name as specializationName, r.number as regionNumber
                from dbo.Doctors as d
                join dbo.Cabinets as c on c.id = d.cabinetID
                join dbo.Specializations as s on s.id = d.specializationID
                left join dbo.Regions as r on r.id = d.regionID
                order by {orderByValue}
                offset @offset rows
                fetch next @pageSize rows only;
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@offset", SqlDbType.Int).Value = (page - 1) * pageSize;
                    command.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize;
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int region = reader.GetOrdinal("regionNumber");
                            var doctor = new DoctorsDtoWithValues()
                            {
                                id = (int)reader["id"],
                                fio = reader["fio"]?.ToString(),
                                cabinetNumber = reader["cabinetNumber"]?.ToString() ?? string.Empty,
                                specializationName = reader["specializationName"]?.ToString(),
                                regionNumber = reader.IsDBNull(region) ? null : reader.GetInt32(region)
                            };
                            doctors.Add(doctor);
                        }

                    }

                }

                return doctors;
            }
        }
        public async Task<DoctorsDtoWithKeys?> GetById(int id)
        {

            string sql = $"""
                select * from dbo.Doctors
                where id = @id;
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                            return null;

                        int region = reader.GetOrdinal("regionId");
                        var doctor = new DoctorsDtoWithKeys()
                        {
                            id = (int)reader["id"],
                            fio = reader["fio"]?.ToString(),
                            cabinetId = (int)reader["cabinetID"],
                            specializationId = (int)reader["specializationID"],
                            regionId = reader.IsDBNull(region) ? null : reader.GetInt32(region)
                        };
                        return doctor;

                    }

                }

;
            }
        }

        public async Task<int> Create(DoctorsDtoWithKeys doctor)
        {
            string sql = """
                    insert into dbo.Doctors
                            output inserted.id
                            values
                            (@fio, @cabinetID, @specializationID, @regionId);
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("fio", doctor.fio));
                    command.Parameters.Add(new SqlParameter("cabinetId", doctor.cabinetId));
                    command.Parameters.Add(new SqlParameter("specializationId", doctor.specializationId));
                    var param = command.Parameters.Add(new SqlParameter("regionId", doctor.regionId));
                    param.Value = doctor.regionId.HasValue ? doctor.regionId.Value : DBNull.Value;
                    var result = await command.ExecuteScalarAsync();

                    return (int)result;
                }
            }
        }

        public async Task<bool> Update(DoctorsDtoWithKeys doctor)
        {
            string sql = """
            update dbo.Doctors set fio = @fio, cabinetId = @cabinetId, specializationId = @specializationId,regionId = @regionId
            where id=@id;
            """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("fio", doctor.fio));
                    command.Parameters.Add(new SqlParameter("cabinetId", doctor.cabinetId));
                    command.Parameters.Add(new SqlParameter("specializationId", doctor.specializationId));
                    var param = command.Parameters.Add(new SqlParameter("regionId", doctor.regionId));
                    param.Value = doctor.regionId.HasValue ? doctor.regionId.Value : DBNull.Value;
                    command.Parameters.Add(new SqlParameter("id", doctor.id));

                    var rowsUpdated = await command.ExecuteNonQueryAsync();
                    return rowsUpdated > 0;
                }

            }

        }

        public async Task<bool> Delete(int id)
        {
            string sql = $"""
                delete from dbo.Doctors 
                where id = @id;
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    var rowsDeleted = await command.ExecuteNonQueryAsync();
                    return rowsDeleted > 0;
                }

            }
        }

    }
}
