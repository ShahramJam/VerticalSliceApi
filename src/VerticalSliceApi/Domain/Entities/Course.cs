namespace VerticalSliceApi.Domain.Entities;

public class Course
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public int Credits { get; private set; }

    private readonly List<Student> _students = [];
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();

    private Course() { } // EF Core

    public Course(string code, string title, int credits)
    {
        Code = code.ToUpperInvariant();
        Title = title;
        Credits = credits;
    }
}
