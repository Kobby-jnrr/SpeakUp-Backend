using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
    }
}