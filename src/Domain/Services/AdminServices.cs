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
        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = _dataContext.Admins.FirstOrDefault(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password);

            return adm;
        }
    }
}
