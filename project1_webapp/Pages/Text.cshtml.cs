using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Reflection.Metadata;
using System.Reflection;

// TODO
// create class for text stuff

namespace project1_webapp.Pages {
    public class TextModel : PageModel {

        [BindProperty]
        public List<string?> keyPhrases { get; set; }

        [BindProperty]
        public string sentiment { get; set; }

        [BindProperty]
        public double positiveScore { get; set; }

        [BindProperty]
        public double negativeScore { get; set; }

        [BindProperty]
        public double neutralScore { get; set; }

        [BindProperty]
        public string summary { get; set; }

        [BindProperty]
        public string userText { get; set; }

        private readonly IConfiguration _configuration;
        private readonly ILogger<TextModel> _logger;

        public TextModel(ILogger<TextModel> logger, IConfiguration configuration) {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet() {


        }

        public async Task<IActionResult> OnPost() {
            
            if (userText != null) {

                // accessing language service
                AzureKeyCredential credentials = new AzureKeyCredential(_configuration["AIKey"]);
                Uri endpoint = new Uri(_configuration["AIEndpoint"]);
                var aiClient = new TextAnalyticsClient(endpoint, credentials);

                // all analysis tasks that will be performed
                TextAnalyticsActions actions = new TextAnalyticsActions() {
                    AbstractiveSummarizeActions = new List<AbstractiveSummarizeAction>() { new AbstractiveSummarizeAction() },
                    AnalyzeSentimentActions = new List<AnalyzeSentimentAction>() { new AnalyzeSentimentAction() { IncludeOpinionMining = true } },
                    ExtractKeyPhrasesActions  = new List<ExtractKeyPhrasesAction>() { new ExtractKeyPhrasesAction() }
                };

                // get results from language service, forces batch for summary results per documentation
                var operation = await aiClient.StartAnalyzeActionsAsync(new List<string>() { userText }, actions);
                await operation.WaitForCompletionAsync();
                var summaryResult = operation.GetValues();
                
                // assigns all the variables to their results
                foreach (var page in summaryResult.AsPages()) {
                    foreach (var allResults in page.Values) {

                        // adds all key phrases
                        foreach (var result in allResults.ExtractKeyPhrasesResults) {
                            foreach (var documentResult in result.DocumentsResults) {
                                foreach (var keyPhrase in documentResult.KeyPhrases) {
                                    keyPhrases.Add(keyPhrase);
                                }
                            }
                        }

                        // assigns sentiment variables
                        foreach (var result in allResults.AnalyzeSentimentResults) {
                            foreach (var documentResult in result.DocumentsResults) {
                                sentiment = documentResult.DocumentSentiment.Sentiment.ToString();
                                positiveScore = documentResult.DocumentSentiment.ConfidenceScores.Positive;
                                neutralScore = documentResult.DocumentSentiment.ConfidenceScores.Neutral;
                                negativeScore = documentResult.DocumentSentiment.ConfidenceScores.Negative;
                            }
                        }

                        // assigns summary variable
                        foreach (var result in allResults.AbstractiveSummarizeResults) {
                            foreach (var documentResult in result.DocumentsResults) {
                                foreach (var sum in documentResult.Summaries) {
                                    summary = sum.Text;
                                }
                            }
                        }
                    }
                }
                
            }

            return Page();
        }
    }
}