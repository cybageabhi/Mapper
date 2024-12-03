using Microsoft.EntityFrameworkCore;
using Server.Models;
namespace Server.Data
{
    public class SecretServerDbContext : DbContext
    {
        public SecretServerDbContext(DbContextOptions<SecretServerDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
