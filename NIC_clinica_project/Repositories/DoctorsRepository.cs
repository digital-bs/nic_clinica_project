using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NIC_clinica_project.Mappers;
using NIC_clinica_project.Models;
using System.Data;
using System.Numerics;

namespace NIC_clinica_project.Repositories
{
    public class DoctorsRepository : ICrudRepository<DoctorsDtoWithKeys, DoctorsDtoWithValues>
    {
        private readonly ClinicaDbContext _context;
        const int defaultPageSize = 10;
        const int maxPageSize = 100;
        private static readonly IReadOnlyDictionary<string, Func<IQueryable<Doctor>, IOrderedQueryable<Doctor>>> DoctorOrderings =
            new Dictionary<string, Func<IQueryable<Doctor>, IOrderedQueryable<Doctor>>>
            {
                ["id"] = query => query.OrderBy(d => d.id),
                ["fio"] = query => query.OrderBy(d => d.fio),
                ["region"] = query => query.OrderBy(d => d.region.number),
                ["spec"] = query => query.OrderBy(d => d.specialization.name),
                ["cabinet"] = query => query.OrderBy(d => d.cabinet.number),
            };


        public DoctorsRepository(ClinicaDbContext context) 
        {
            _context = context;

        }

        public bool IsSortable(string? orderParam)
        {
            return !string.IsNullOrEmpty(orderParam) && DoctorOrderings.ContainsKey(orderParam);

        }
        public async Task<bool> IsValid(DoctorsDtoWithKeys doctor)
        {
            if (string.IsNullOrWhiteSpace(doctor.fio) || doctor.fio.Length > 200)
                return false;

            var isCabinetValid = await _context.Cabinets.AnyAsync(c => c.id == doctor.cabinetId);

            if (!isCabinetValid) return false;
            var isSpecValid = await _context.Specializations.AnyAsync(c=> c.id == doctor.specializationId);
            
            if (!isSpecValid) return false;

            if (doctor.regionId is int) 
                return await _context.Regions.AnyAsync(c => c.id == doctor.regionId); 


            return true;

            
        }
        
        public async Task<List<DoctorsDtoWithValues>> GetAll(int page, int pageSize, string orderParam)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = defaultPageSize;
            else if (pageSize > maxPageSize)
                pageSize = maxPageSize;


            if (!IsSortable(orderParam))
                throw new ArgumentException("Некорректное поле сортмровки.", nameof(orderParam));

            var query = DoctorOrderings[orderParam](_context.Doctors.AsNoTracking());
            var doctors = await query.Include(d=>d.cabinet)
                .Include(d => d.specialization)
                .Include(d => d.region)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var doctorsDto = doctors.Select(d => d.ConvertToDtoWithValues()).ToList();

            return doctorsDto;

        }
        public async Task<DoctorsDtoWithKeys?> GetById(int id)
        {
            var doctor =await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d=>d.id ==id);
            if (doctor is null) return null;

            return doctor.ConvertToDtoWithKeys();
        }

        public async Task<int> Create(DoctorsDtoWithKeys doctor)
        {
            var newDoctor = doctor.ToEFEntity();
            _context.Doctors.Add(newDoctor);
            await _context.SaveChangesAsync();
            return newDoctor.id;

        }

        public async Task<bool> Update(DoctorsDtoWithKeys doctor)
        {
            var doctorForUpdate = await _context.Doctors.FindAsync(doctor.id);
            if (doctorForUpdate is null) return false;
            doctor.ConvertToEFEntity(doctorForUpdate);

            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> Delete(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor is null) return false;
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
