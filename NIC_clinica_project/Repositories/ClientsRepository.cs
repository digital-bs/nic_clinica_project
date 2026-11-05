using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using NIC_clinica_project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace NIC_clinica_project.Repositories
{
    public class ClientsRepository : SortableRepository, ICrudRepository<ClientsDtoWithKeys, ClientsDtoWithValues>
    {
        private string _connectionString;
        const int defaultPageSize = 10;
        const int maxPageSize = 100;
        private static readonly IReadOnlyDictionary<string, string> ClientOrderByValues = new Dictionary<string, string>()
        {
            ["id"] = "c.id",
            ["firstName"] = "c.firstName",
            ["lastName"] = "c.lastName",
            ["otchestvo"] = "c.otchestvo",
            ["adress"] = "c.adress",
            ["birthday"] = "c.birthday",
            ["gender"] = "c.gender",
            ["region"] = "r.number"
        };

        public ClientsRepository(IConfiguration configuration) : base(ClientOrderByValues)
        {
            _connectionString = configuration.GetConnectionString("ClinicaDb");
        }

        public async Task<bool> IsValid(ClientsDtoWithKeys client)
        {
            if (client is null
                || string.IsNullOrWhiteSpace(client.firstName)
                || client.firstName.Length > 50
                || string.IsNullOrWhiteSpace(client.lastName)
                || client.lastName.Length > 50
                || string.IsNullOrWhiteSpace(client.otchestvo)
                || client.otchestvo.Length > 50
                || string.IsNullOrWhiteSpace(client.adress)
                || client.adress.Length > 100
                || string.IsNullOrWhiteSpace(client.gender)
                || client.gender.Length > 10
                || client.birthday.Date > DateTime.Today
                || client.regionId <= 0)
            {
                return false;
            }
            const string sql = """
                    select convert(bit,
                        case
                            when exists (select 1 from dbo.Regions where id = @regionId)
                            then 1
                            else 0
                        end);
                    """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@regionId", SqlDbType.Int).Value = client.regionId;

                    var result = await command.ExecuteScalarAsync();
                    return result is true;
                }
            }
        }
        public async Task<List<ClientsDtoWithValues>> GetAll(int page, int pageSize, string orderParam)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = defaultPageSize;
            else if (pageSize > maxPageSize)
                pageSize = maxPageSize;

            var clients = new List<ClientsDtoWithValues>();

            var orderByValue = GetOrderByValue(orderParam);

            string sql = $"""
                    select c.id, c.firstName,c.lastName, c.otchestvo, c.adress, c.birthday, c.gender, r.number AS regionNumber
                            from dbo.Clients as c
                            join dbo.Regions as r ON r.id = c.regionId
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
                            var client = new ClientsDtoWithValues()
                            {
                                id = (int)reader["id"],
                                firstName = reader["firstName"]?.ToString(),
                                lastName = reader["lastName"]?.ToString(),
                                otchestvo = reader["otchestvo"]?.ToString(),
                                adress = reader["adress"]?.ToString(),
                                birthday = (DateTime)reader["birthday"],
                                gender = reader["gender"]?.ToString(),
                                regionNumber = (int)reader["regionNumber"]
                            };
                            clients.Add(client);
                        }

                    }

                }

                return clients;
            }
        }

        public async Task<ClientsDtoWithKeys?> GetById(int id)
        {
            string sql = $"""
                        select id, firstName,lastName, otchestvo, adress, birthday, gender,regionId 
                        from dbo.Clients
                        where id = @id
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
                        var client = new ClientsDtoWithKeys()
                        {
                            id = (int)reader["id"],
                            firstName = reader["firstName"]?.ToString(),
                            lastName = reader["lastName"]?.ToString(),
                            otchestvo = reader["otchestvo"]?.ToString(),
                            adress = reader["adress"]?.ToString(),
                            birthday = (DateTime)reader["birthday"],
                            gender = reader["gender"]?.ToString(),
                            regionId = (int)reader["regionId"]
                        };
                        return client;


                    }
                }
            }
        }

        public async Task<int> Create(ClientsDtoWithKeys client)
        {
            string sql = """
                    insert into dbo.Clients
                            output inserted.id
                            values
                            (@firstName, @lastName, @otchestvo, @adress, @birthday, @gender, @regionId);
                """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("firstName", client.firstName));
                    command.Parameters.Add(new SqlParameter("lastName", client.lastName));
                    command.Parameters.Add(new SqlParameter("otchestvo", client.otchestvo));
                    command.Parameters.Add(new SqlParameter("adress", client.adress));
                    command.Parameters.Add(new SqlParameter("birthday", client.birthday));
                    command.Parameters.Add(new SqlParameter("gender", client.gender));
                    command.Parameters.Add(new SqlParameter("regionId", client.regionId));
                    var result = await command.ExecuteScalarAsync();


                    return (int)result;
                }
            }
        }

        public async Task<bool> Update(ClientsDtoWithKeys client)
        {
            string sql = """
            update dbo.Clients set firstName = @firstName, lastName = @lastName, otchestvo = @otchestvo, adress = @adress, 
            birthday = @birthday,gender = @gender,regionId = @regionId
            where id=@id;
            """;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("firstName", client.firstName));
                    command.Parameters.Add(new SqlParameter("lastName", client.lastName));
                    command.Parameters.Add(new SqlParameter("otchestvo", client.otchestvo));
                    command.Parameters.Add(new SqlParameter("adress", client.adress));
                    command.Parameters.Add(new SqlParameter("birthday", client.birthday));
                    command.Parameters.Add(new SqlParameter("gender", client.gender));
                    command.Parameters.Add(new SqlParameter("regionId", client.regionId));
                    command.Parameters.Add(new SqlParameter("id", client.id));

                    var rowsUpdated = await command.ExecuteNonQueryAsync();
                    return rowsUpdated > 0;
                }

            }

        }
        public async Task<bool> Delete(int id)
        {
            string sql = $"""
                delete from dbo.Clients 
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
