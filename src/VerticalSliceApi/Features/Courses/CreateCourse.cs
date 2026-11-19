using FluentValidation;
using MediatR;
using VerticalSliceApi.Domain.Entities;
using VerticalSliceApi.Features.Common;
using VerticalSliceApi.Infrastructure.Persistence;

namespace VerticalSliceApi.Features.Courses;

public static class CreateCourse
{
    public sealed record Command(string Code, string Title, int Credits) : IRequest<int>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Credits).InclusiveBetween(1, 60);
        }
    }

    public sealed class Handler(SchoolDbContext db) : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken ct)
        {
            var course = new Course(request.Code, request.Title, request.Credits);
            db.Courses.Add(course);
            await db.SaveChangesAsync(ct);
            return course.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/courses", async (Command command, ISender sender, CancellationToken ct) =>
            {
                var id = await sender.Send(command, ct);
                return Results.Created($"/api/courses/{id}", new { Id = id });
            })
            .WithTags("Courses");
        }
    }
}

