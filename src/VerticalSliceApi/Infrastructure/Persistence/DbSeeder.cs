using Microsoft.EntityFrameworkCore;
using VerticalSliceApi.Domain.Entities;

namespace VerticalSliceApi.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(DbContext context, bool _, CancellationToken ct)
    {
        // Prevent duplicate seeding if data already exists
        if (await context.Set<Course>().AnyAsync(ct) || await context.Set<Student>().AnyAsync(ct))
            return;

        // 1. Seed 4 Courses
        var csharpCourse = new Course("CS101", "C# & .NET 10 Fundamentals", 15);
        var efCoreCourse = new Course("DB202", "Entity Framework Core & SQL Server", 10);
        var webApiCourse = new Course("WEB303", "ASP.NET Core Web API Architecture", 15);
        var cloudCourse = new Course("CLD404", "Cloud Deployment & Docker", 10);

        context.Set<Course>().AddRange(csharpCourse, efCoreCourse, webApiCourse, cloudCourse);

        // 2. Seed 5 Students
        var alice = new Student("Alice", "Lindqvist", "alice.lindqvist@example.se");
        var erik = new Student("Erik", "Bergström", "erik.bergstrom@example.se");
        var maja = new Student("Maja", "Svensson", "maja.svensson@example.se");
        var oskar = new Student("Oskar", "Nilsson", "oskar.nilsson@example.se");
        var sara = new Student("Sara", "Holm", "sara.holm@example.se");

        // 3. Enroll Students in Courses (populates StudentCourses join table)
        alice.EnrollInCourse(csharpCourse);
        alice.EnrollInCourse(webApiCourse);

        erik.EnrollInCourse(csharpCourse);
        erik.EnrollInCourse(efCoreCourse);

        maja.EnrollInCourse(webApiCourse);
        maja.EnrollInCourse(cloudCourse);

        oskar.EnrollInCourse(efCoreCourse);
        oskar.EnrollInCourse(cloudCourse);

        sara.EnrollInCourse(csharpCourse);
        sara.EnrollInCourse(efCoreCourse);
        sara.EnrollInCourse(webApiCourse);

        context.Set<Student>().AddRange(alice, erik, maja, oskar, sara);

        await context.SaveChangesAsync(ct);
    }
}

