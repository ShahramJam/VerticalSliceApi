using FluentValidation;
using MediatR;
using VerticalSliceApi.Domain.Entities;
using VerticalSliceApi.Features.Common;
using VerticalSliceApi.Infrastructure.Persistence;

namespace VerticalSliceApi.Features.Students;

public static class CreateStudent
{
    public sealed record Command(string FirstName, string LastName, string Email) : IRequest<int>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        }
    }

    public sealed class Handler(SchoolDbContext db) : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken ct)
        {
            var student = new Student(request.FirstName, request.LastName, request.Email);
            db.Students.Add(student);
            await db.SaveChangesAsync(ct);
            return student.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/students", async (Command command, ISender sender, CancellationToken ct) =>
            {
                var id = await sender.Send(command, ct);
                return Results.Created($"/api/students/{id}", new { Id = id });
            })
            .WithTags("Students");
        }
    }
}

