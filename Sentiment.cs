using System;

namespace CyberSecurityGUI
{
    // ===== DELEGATES =====
    public delegate string SentimentResponseHandler(string sentiment, string input);
    public delegate void SentimentDetectedEventHandler(string sentiment, string input);

    public class Sentiment
    {
        // ===== EVENT =====
        public event SentimentDetectedEventHandler OnSentimentDetected;

        // ===== DETECT SENTIMENT =====
        public string GetSentiment(string input)
        {
            input = input.ToLower();

            if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious"))
            {
                return ProcessSentiment("worried", input,
                    (sentiment, userInput) =>
                    $"It sounds like you're {sentiment}. Don't worry — I'll help you stay safe online step by step.");
            }

            if (input.Contains("frustrated") || input.Contains("angry") || input.Contains("annoyed"))
            {
                return ProcessSentiment("frustrated", input,
                    (sentiment, userInput) =>
                    $"I hear you — it can be a lot to take in. Let's slow down.");
            }

            if (input.Contains("confused") || input.Contains("don't understand") || input.Contains("dont understand"))
            {
                return ProcessSentiment("confused", input,
                    (sentiment, userInput) =>
                    $"No stress — I'll break it down simply for you.");
            }

            if (input.Contains("curious") || input.Contains("interesting"))
            {
                return ProcessSentiment("curious", input,
                    (sentiment, userInput) =>
                    $"Great mindset! Curiosity is the first step to staying cyber-safe. 🔍");
            }

            return "";
        }

        // ===== PROCESS SENTIMENT =====
        private string ProcessSentiment(string sentiment, string input, SentimentResponseHandler handler)
        {
            OnSentimentDetected?.Invoke(sentiment, input);
            return handler(sentiment, input);
        }
    }
}