using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApi.Features.Common;
using VerticalSliceApi.Infrastructure.Persistence;

namespace VerticalSliceApi.Features.Students;

public static class EnrollStudent
{
    public sealed record Command(int StudentId, int CourseId) : IRequest<bool>;

    public sealed class Handler(SchoolDbContext db) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken ct)
        {
            var student = await db.Students
                .Include(s => s.Courses)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            var course = await db.Courses
                .FirstOrDefaultAsync(c => c.Id == request.CourseId, ct);

            if (student is null || course is null)
                return false;

            student.EnrollInCourse(course);
            await db.SaveChangesAsync(ct);
            return true;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/students/{studentId:int}/courses/{courseId:int}",
                async (int studentId, int courseId, ISender sender, CancellationToken ct) =>
                {
                    var success = await sender.Send(new Command(studentId, courseId), ct);
                    return success ? Results.NoContent() : Results.NotFound("Student or Course not found.");
                })
                .WithTags("Students");
        }
    }
}


