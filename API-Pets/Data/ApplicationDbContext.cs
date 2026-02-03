using API_Pets.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace API_Pets.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Pets> Pets { get; set; }
    }
}
