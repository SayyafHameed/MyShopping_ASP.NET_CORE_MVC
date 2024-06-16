using MyShoping.Entities.Models;
using MyShoping.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShoping.DataAccess.Implementation
{
    public class AppUserRepository : GenericRepository<ApplicationUser>, IAppUserRepository
    {
        private readonly AppDbContext _context;
        public AppUserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
