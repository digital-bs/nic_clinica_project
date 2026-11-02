using Microsoft.Data.SqlClient;
using NIC_clinica_project.Models;
using System.Numerics;

namespace NIC_clinica_project.Repositories
{
    public class DoctorsRepository
    {
        private string _connectionString;
        public DoctorsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaDb");

        }

        public async Task<List<DoctorsDtoWithValues>> GetAllDoctorsWithValues(int page, int pageSize, string orderParam)
        {
            var doctors = new List<DoctorsDtoWithValues>();
            var sortColumn = orderParam.ToLowerInvariant() switch
            {
                "id" => "d.id",
                "fio" => "d.fio",
                "cabinet" => "c.number",
                "spec" => "s.name",
                "region" => "r.number"
            };
            string sql = $"""
                select d.id,d.fio, c.number as cabinetNumber,s.name as specializationName, r.number as regionNumber
                from dbo.Doctors as d
                join dbo.Cabinets as c on c.id = d.cabinetID
                join dbo.Specializations as s on s.id = d.specializationID
                left join dbo.Regions as r on r.id = d.regionID
                order by {sortColumn}
                offset {(page - 1) * pageSize} rows
                fetch next {pageSize} rows only;

                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int region = reader.GetOrdinal("regionNumber");
                            var doctor = new DoctorsDtoWithValues()
                            {
                                id = (int)reader["id"],
                                fio = reader["fio"]?.ToString(),
                                cabinetNumber = (int)reader["cabinetNumber"],
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
        public async Task<DoctorsDtoWithKeys?> GetDoctorById(int id)
        {

            string sql = $"""
                select * from dbo.Doctors
                where id = {id};
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
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

        public async Task<int> CreateDoctor(DoctorsDtoWithKeys doctor)
        {
            //string sql = $"""
            //            insert into dbo.Clients
            //            output inserted.id
            //            values
            //            (N'{client.firstName}', N'{client.lastName}', N'{client.otchestvo}', N'{client.adress}', '{client.birthday}', N'{client.gender}', {client.regionId});
            //            """;

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

        public async Task<bool> UpdateDoctor(DoctorsDtoWithKeys doctor)
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

        public async Task<bool> DeleteDoctor(int id)
        {
            string sql = $"""
                delete from dbo.Doctors 
                where id = {id};
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    var rowsDeleted = await command.ExecuteNonQueryAsync();
                    return rowsDeleted > 0;
                }

            }
        }

    }
}
