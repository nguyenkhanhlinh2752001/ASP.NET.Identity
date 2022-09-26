using Identity.WebApi.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.WebApi.Data
{
    public class DataContext:IdentityDbContext
    {
        public DataContext(DbContextOptions options):base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
