using Microsoft.EntityFrameworkCore;
using Formify.Models;

namespace Formify.Data
{
    public class FormifyDbContext : DbContext
    {
        public FormifyDbContext(DbContextOptions<FormifyDbContext> options)
            : base(options) { }

        // Na start testowa tabela Użytkowników
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<WaterIntake> WaterIntakes { get; set; }
        public DbSet<WeightEntry> WeightEntries { get; set; }
        public DbSet<UserContextChange> UserContextChanges { get; set; }

    }

}
