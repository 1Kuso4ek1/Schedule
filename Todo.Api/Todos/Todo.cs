using System.ComponentModel.DataAnnotations;

public class Schedule
{
    public int Id { get; set; }
    [Required]
    public string Group { get; set; } = default!;
    public int FirstLesson { get; set; } = 1;
    public List<List<LessonItem>> LessonsByDays { get; set; } = default!;

    [Required]
    public string OwnerId { get; set; } = default!;
}

// The DTO that excludes the OwnerId (we don't want that exposed to clients)
public class ScheduleItem
{
    public int Id { get; set; }
    [Required]
    public string Group { get; set; } = default!;
    public int FirstLesson { get; set; } = 1;
    public List<List<LessonItem>> LessonsByDays { get; set; } = default!;
}

public class LessonItem
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = default!;
    [Required]
    public string Teacher { get; set; } = default!;
}

public static class ScheduleMappingExtensions
{
    public static ScheduleItem AsScheduleItem(this Schedule schedule)
    {
        return new()
        {
            Id = schedule.Id,
            Group = schedule.Group,
            LessonsByDays = schedule.LessonsByDays
        };
    }
}