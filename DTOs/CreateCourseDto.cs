namespace grad.DTOs
{
    public class CreateCourseDto
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Introduction { get; set; }
        public string VideoUrl { get; set; }
        public string Schedule { get; set; }
        public string ClassType { get; set; }
        public decimal MonthlyPrice { get; set; }
    }
}