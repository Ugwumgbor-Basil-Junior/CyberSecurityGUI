namespace CyberSecurityGUI.Models
{
    // ===== QUIZ QUESTION MODEL =====
    public class QuizQuestion
    {
        public string QuestionText { get; set; } = "";
        public string[] Options { get; set; } = System.Array.Empty<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
        public bool IsTrueFalse { get; set; }
    }
}
 