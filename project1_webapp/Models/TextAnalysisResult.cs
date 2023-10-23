using Microsoft.AspNetCore.Mvc;

namespace project1_webapp.Models {
    public class TextAnalysisResult {
        public List<string?> KeyPhrases { get; set; } = new List<string?>();
        public string? Sentiment { get; set; }
        public double? PositiveScore { get; set; }
        public double? NegativeScore { get; set; }
        public double? NeutralScore { get; set; }
        public string? Summary { get; set; }
        public string? UserText { get; set; }
    }
}
