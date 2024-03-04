using project.Domain.DTOs;
using project.Domain.Entities;

namespace project.Domain.Interfaces
{
    public interface IAdmin
    {
        Admin? Login(LoginDTO loginDTO);

        Admin PostAdmin(Admin admin);
            
        List<Admin> GetAdmins(int? page);

        Admin GetAdmin(int id);
    }
}
