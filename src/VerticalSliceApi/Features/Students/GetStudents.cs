using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApi.Features.Common;
using VerticalSliceApi.Infrastructure.Persistence;

namespace VerticalSliceApi.Features.Students;

public static class GetStudents
{
    public sealed record Query : IRequest<List<Response>>;
    public sealed record Response(int Id, string FullName, string Email, List<string> Courses);

    public sealed class Handler(SchoolDbContext db) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query request, CancellationToken ct)
        {
            return await db.Students
                .AsNoTracking()
                .Select(s => new Response(
                    s.Id,
                    $"{s.FirstName} {s.LastName}",
                    s.Email,
                    s.Courses.Select(c => $"{c.Code} - {c.Title}").ToList()))
                .ToListAsync(ct);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/students", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new Query(), ct)))
                .WithTags("Students");
        }
    }
}


