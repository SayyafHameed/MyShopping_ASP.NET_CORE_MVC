using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShoping.DataAccess.DbInitialize;
using MyShoping.Entities.Models;
using MyShoping.Entities.Repository;
using MyShopping.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShoping.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;


        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public void Initialize()
        {
            if (!_roleManager.RoleExistsAsync(SD.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.EditorRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole)).GetAwaiter().GetResult();
            }

            // Migration
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception)
            {

                throw;
            }

            // Roles
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@dotnet.com",
                Email = "admin@dotnet.com",
                Name = "Sayyaf Hameed",
                PhoneNumber = "776745614",
                Address = "Sanaa",
                City = "Sanaa"
            }, "Admin45614@").GetAwaiter().GetResult();

            //User
            ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@dotnet.com");
            _userManager.AddToRoleAsync(user, SD.AdminRole).GetAwaiter().GetResult();


        }
    }
}
