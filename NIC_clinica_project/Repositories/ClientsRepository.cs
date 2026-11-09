using Microsoft.Data.SqlClient;
using NIC_clinica_project.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using NIC_clinica_project.Mappers;

namespace NIC_clinica_project.Repositories
{
    public class ClientsRepository : ICrudRepository<ClientsDtoWithKeys, ClientsDtoWithValues>
    {
        private readonly ClinicaDbContext _context;
        const int defaultPageSize = 10;
        const int maxPageSize = 100;
        private static readonly IReadOnlyDictionary<string, Func<IQueryable<Client>, IOrderedQueryable<Client>>> ClientOrderings =
            new Dictionary<string, Func<IQueryable<Client>, IOrderedQueryable<Client>>>
            {
                ["id"] = query => query.OrderBy(c => c.id),
                ["firstName"] = query => query.OrderBy(c => c.firstName),
                ["lastName"] = query => query.OrderBy(c => c.lastName),
                ["otchestvo"] = query => query.OrderBy(c => c.otchestvo),
                ["adress"] = query => query.OrderBy(c => c.adress),
                ["birthday"] = query => query.OrderBy(c => c.birthday),
                ["gender"] = query => query.OrderBy(c => c.gender),
                ["region"] = query => query.OrderBy(c => c.region.number)
            };

        

        public ClientsRepository(ClinicaDbContext context) 
        {
            _context = context;
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
                || client.birthday > DateOnly.FromDateTime(DateTime.Today)
                || client.regionId <= 0)
            {
                return false;
            }
            return await _context.Regions.AnyAsync(r => r.id == client.regionId);
        }
        public bool IsSortable(string? orderParam)
        {
            return !string.IsNullOrWhiteSpace(orderParam) && ClientOrderings.ContainsKey(orderParam);
        }
        public async Task<List<ClientsDtoWithValues>> GetAll(int page, int pageSize, string orderParam)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = defaultPageSize;
            else if (pageSize > maxPageSize)
                pageSize = maxPageSize;



            if (!IsSortable(orderParam))
                throw new ArgumentException("Некорректное поле сортмровки.", nameof(orderParam));

            var query = ClientOrderings[orderParam](_context.Clients.AsNoTracking());

            var clients = await query
                .Include(c => c.region)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return clients.Select(c => c.ConvertToDtoWithValues()).ToList();

        }

        public async Task<ClientsDtoWithKeys?> GetById(int id)
        {
            var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.id == id);
            if (client is null) return null;


            return client.ConvertToDtoWithKeys();
        }

        public async Task<int> Create(ClientsDtoWithKeys client)
        {
            var newClient = client.ToEfEntity();
            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();

            return newClient.id;
        }

        public async Task<bool> Update(ClientsDtoWithKeys client)
        {
            var clientForUpdate = await _context.Clients.FindAsync(client.id);
            if (clientForUpdate is null)
                return false;
            client.ConvertToEfEntity(clientForUpdate);

            await _context.SaveChangesAsync();

            return true;

        }
        public async Task<bool> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client is null)
                return false;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }




    }
}
