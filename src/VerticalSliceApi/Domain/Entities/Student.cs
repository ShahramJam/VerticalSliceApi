namespace VerticalSliceApi.Domain.Entities;

public class Student
{
    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime EnrolledAtUtc { get; private set; }

    private readonly List<Course> _courses = [];
    public IReadOnlyCollection<Course> Courses => _courses.AsReadOnly();

    private Student() { } // EF Core

    public Student(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        EnrolledAtUtc = DateTime.UtcNow;
    }

    public void EnrollInCourse(Course course)
    {
        if (_courses.All(c => c.Id != course.Id))
            _courses.Add(course);
    }
}
