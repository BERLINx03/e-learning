namespace ELearning.Services.Dtos;
public class LessonResponseDto
{
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public int Order { get; set; }
        public string? VideoUrl { get; set; }
        public string? DocumentUrl { get; set; }
        public bool IsQuiz { get; set; }
}
