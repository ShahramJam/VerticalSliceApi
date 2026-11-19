using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceApi.Features.Common;
using VerticalSliceApi.Infrastructure.Persistence;

namespace VerticalSliceApi.Features.Courses;

public static class GetCourses
{
    public sealed record Query : IRequest<List<Response>>;
    public sealed record Response(int Id, string Code, string Title, int Credits, int EnrolledCount);

    public sealed class Handler(SchoolDbContext db) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query request, CancellationToken ct)
        {
            return await db.Courses
                .AsNoTracking()
                .Select(c => new Response(c.Id, c.Code, c.Title, c.Credits, c.Students.Count))
                .ToListAsync(ct);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/courses", async (ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new Query(), ct)))
                .WithTags("Courses");
        }
    }
}

