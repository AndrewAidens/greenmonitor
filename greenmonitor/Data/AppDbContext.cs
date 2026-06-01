using greenmonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace greenmonitor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
