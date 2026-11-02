using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using NIC_clinica_project.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NIC_clinica_project.Repositories
{
    public class ClientsRepository
    {
        private string _connectionString;
        public ClientsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ClinicaDb");
            //Console.WriteLine(_connectionString);


        }

        public async Task<List<ClientsDtoWithValues>> GetAllClientsWithValues()
        {
            var clients = new List<ClientsDtoWithValues>();

            string sql = """
                        select c.id, c.firstName,c.lastName, c.otchestvo, c.adress, c.birthday, c.gender, r.number AS regionNumber
                        from dbo.Clients AS c
                        join dbo.Regions AS r ON r.id = c.regionId
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
                            var client = new ClientsDtoWithValues()
                            {
                                id = (int)reader["id"],
                                firstName = reader["firstName"]?.ToString(),
                                lastName = reader["lastName"]?.ToString(),
                                otchestvo = reader["otchestvo"].ToString(),
                                adress = reader["adress"].ToString(),
                                birthday = (DateTime)reader["birthday"],
                                gender = reader["gender"].ToString(),
                                regionNumber = (int)reader["regionNumber"]
                            };
                            clients.Add(client);
                        }

                    }

                }

                return clients;
            }
        }

        public async Task<List<ClientsDtoWithValues>> GetAllClientsWithValues(int page, int pageSize, string orderParam)
        {
            var clients = new List<ClientsDtoWithValues>();

            var sortColumn = orderParam.ToLowerInvariant() switch
            {
                "id" => "c.id",
                "firstname" => "c.firstName",
                "lastname" => "c.lastName",
                "otchestvo" => "c.otchestvo",
                "adress" => "c.adress",
                "birthday" => "c.birthday",
                "gender" => "c.gender",
                "region" => "r.number"
            };
            string sql = $"""
                    select c.id, c.firstName,c.lastName, c.otchestvo, c.adress, c.birthday, c.gender, r.number AS regionNumber
                            from dbo.Clients as c
                            join dbo.Regions as r ON r.id = c.regionId
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

        public async Task<ClientsDtoWithKeys?> GetClientById(int id)
        {
            string sql = $"""
                        select id, firstName,lastName, otchestvo, adress, birthday, gender,regionId 
                        from dbo.Clients
                        where id = {id}
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

        public async Task<int> CreateClient(ClientsDtoWithKeys client)
        {
            //string sql = $"""
            //            insert into dbo.Clients
            //            output inserted.id
            //            values
            //            (N'{client.firstName}', N'{client.lastName}', N'{client.otchestvo}', N'{client.adress}', '{client.birthday}', N'{client.gender}', {client.regionId});
            //            """;

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

        public async Task<bool> UpdateClient(ClientsDtoWithKeys client)
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
        public async Task<bool> DeleteClient(int id)
        {
            string sql = $"""
                delete from dbo.Clients 
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
