using CaffeeCoApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CaffeeCoApp.Services
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>    
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}
