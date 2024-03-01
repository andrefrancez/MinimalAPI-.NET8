using project.Domain.Entities;
using project.Domain.Interfaces;
using project.Infra.Db;

namespace project.Domain.Services
{
    public class VehicleServices : IVehicle
    {
        private readonly DataContext _context;

        public VehicleServices(DataContext dataContext)
        {
            _context = dataContext;
        }
        public void DeleteVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
            _context.SaveChanges();

        }

        public Vehicle GetVehicle(int id)
        {
            return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
        }

        public List<Vehicle> GetVehicles(int? page = 1, string? name = null, string? make = null)
        {
            IQueryable<Vehicle> query = _context.Vehicles;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(v => v.Name.ToLower().Contains(name));
            }

            if (!string.IsNullOrEmpty(make))
            {
                query = query.Where(v => v.Make.ToLower().Contains(make));
            }

            int pageSize = 10;
            int pageNumber = page ?? 1; 
            int skip = (pageNumber - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            List<Vehicle> vehicles = query.ToList();
            return vehicles;
        }

        public void PostVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
        }
    }
}
