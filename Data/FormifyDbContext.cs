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
    }
}
