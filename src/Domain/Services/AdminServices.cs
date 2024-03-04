using project.Domain.DTOs;
using project.Domain.Entities;
using project.Domain.Interfaces;
using project.Infra.Db;

namespace project.Domain.Services
{
    public class AdminServices : IAdmin
    {
        private readonly DataContext _dataContext;
        public AdminServices(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public Admin GetAdmin(int id)
        {
            return _dataContext.Admins.Where(a => a.Id == id).FirstOrDefault();
        }

        public List<Admin> GetAdmins(int? page)
        {
            var query = _dataContext.Admins.AsQueryable();

            int pageSize = 10;
            int pageNumber = page ?? 1;
            int skip = (pageNumber - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            return query.ToList();
        }

        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = _dataContext.Admins.FirstOrDefault(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password);

            return adm;
        }

        public Admin PostAdmin(Admin admin)
        {
            _dataContext.Admins.Add(admin);
            _dataContext.SaveChanges();

            return admin;
        }
    }
}
