using Microsoft.EntityFrameworkCore;
using VerticalSliceApi.Domain.Entities;

namespace VerticalSliceApi.Infrastructure.Persistence;

public class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
            b.Property(s => s.LastName).HasMaxLength(100).IsRequired();
            b.Property(s => s.Email).HasMaxLength(200).IsRequired();
            b.HasIndex(s => s.Email).IsUnique();

            b.HasMany(s => s.Courses)
             .WithMany(c => c.Students)
             .UsingEntity(j => j.ToTable("StudentCourses"));
        });

        modelBuilder.Entity<Course>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(c => c.Code).IsUnique();
            b.Property(c => c.Title).HasMaxLength(200).IsRequired();
            b.Property(c => c.Credits).IsRequired();
        });
    }
}  

