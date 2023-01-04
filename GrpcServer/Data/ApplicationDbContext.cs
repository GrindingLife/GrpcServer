using BHFunctioning.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace GrpcServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<HealthData> HealthData { get; set; }

    }
}