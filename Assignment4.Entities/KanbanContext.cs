

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Assignment4.Core;
using Assignment4.Entities;

namespace Assignment4.Entities
{
    public class KanbanContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        public KanbanContext(DbContextOptions<KanbanContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Task>()
                .Property(t => t.State)
                .HasConversion(new EnumToStringConverter<State>());
            
            // modelBuilder
            //     .Entity<Task>()
            //     .Property(t => t.Tags)
            //     .HasConversion(t => Tag.ToString())

            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();

            modelBuilder.Entity<Tag>()
                        .HasIndex(t => t.Name)
                        .IsUnique();

        }
    }
}
