using project.Domain.Entities;

namespace project.Domain.Interfaces
{
    public interface IVehicle
    {
        List<Vehicle> GetVehicles(int page = 1, string? name = null, string? make = null);

        Vehicle GetVehicle(int id);

        void PostVehicle(Vehicle vehicle);

        void UpdateVehicle(Vehicle vehicle);

        void DeleteVehicle(Vehicle vehicle);
    }
}
