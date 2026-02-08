namespace grad.DTOs
{
    public class HomeResponseDto
    {
        public string StudentName { get; set; } = string.Empty; // غيرنا UserName لـ StudentName
        public StatisticsDto Statistics { get; set; } = new();
        public List<LessonDto> PopularLessons { get; set; } = new();
        public List<EventDto> TodayEvents { get; set; } = new();
    }

    public class StatisticsDto
    {
        public int Absence { get; set; }
        public int Tasks { get; set; } // غيرنا الاسم لـ Tasks بس
        public int Quiz { get; set; }
    }

    public class LessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // خانة جديدة
        public int LessonCount { get; set; } // خانة جديدة
        public string Duration { get; set; } = "0h 0m"; // خليناها string عشان تقبل "6h 30min"
        public decimal Rating { get; set; }
        public string ImageUrl { get; set; } = string.Empty; // غيرنا ThumbnailUrl لـ ImageUrl
    }

    public class EventDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; } // ضفنا التاريخ عشان نعرضه
    }
}