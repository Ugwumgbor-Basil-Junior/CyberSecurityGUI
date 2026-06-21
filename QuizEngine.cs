using System;
using System.Collections.Generic;
using CyberSecurityGUI.Models;
using CyberSecurityGUI.Services;

namespace CyberSecurityGUI.Services
{
    // ===== CYBERSECURITY MINI-GAME / QUIZ ENGINE (Task 2) =====
    public class QuizEngine
    {
        private readonly List<QuizQuestion> _bank;
        private readonly Random _rand = new();
        private List<QuizQuestion> _session = new();
        private int _currentIndex = -1;
        private int _score;

        public bool IsActive => _currentIndex >= 0 && _currentIndex < _session.Count;
        public int Score => _score; 
        public int TotalAnswered { get; private set; }
        public int TotalQuestions => _session.Count; 
        public int CurrentNumber => _currentIndex + 1;

        public QuizEngine()
        {
            _bank = BuildQuestionBank();
        }

        // Starts a new randomised round of `count` questions.
        public QuizQuestion StartQuiz(int count = 10)
        {
            var shuffled = new List<QuizQuestion>(_bank);
            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = _rand.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            _session = shuffled.GetRange(0, Math.Min(count, shuffled.Count));
            _currentIndex = 0;
            _score = 0;
            TotalAnswered = 0;

            ActivityLogger.Log($"Quiz started - {_session.Count} questions");
            return _session[_currentIndex];
        }

        public QuizQuestion? CurrentQuestion => IsActive ? _session[_currentIndex] : null;

        // Returns (isCorrect, explanation)
        public (bool isCorrect, string explanation) SubmitAnswer(int selectedIndex)
        {
            if (!IsActive) return (false, "No active question.");

            var q = _session[_currentIndex];
            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) _score++;
            TotalAnswered++;
            _currentIndex++;

            if (_currentIndex >= _session.Count)
                ActivityLogger.Log($"Quiz completed - scored {_score}/{_session.Count}");

            return (correct, q.Explanation);
        }

        public string GetFinalFeedback()
        {
            double pct = _session.Count == 0 ? 0 : (double)_score / _session.Count * 100;
            string verdict = pct >= 80 ? "Great job! You're a cybersecurity pro! 🛡️"
                            : pct >= 50 ? "Good effort! A bit more practice and you'll be a pro."
                            : "Keep learning to stay safe online! 📚";
            return $"Quiz complete! You scored {_score}/{_session.Count} ({pct:0}%).\n{verdict}";
        }

        private List<QuizQuestion> BuildQuestionBank() => new()
        {
            new QuizQuestion { QuestionText = "What should you do if you receive an email asking for your password?",
                Options = new[]{"Reply with your password","Delete the email","Report the email as phishing","Ignore it"},
                CorrectIndex = 2, Explanation = "Correct! Reporting phishing emails helps prevent scams and protects others." },

            new QuizQuestion { QuestionText = "True or False: Using the same password across multiple accounts is safe.",
                Options = new[]{"True","False"}, IsTrueFalse = true, CorrectIndex = 1,
                Explanation = "False. Reusing passwords means one breach can compromise all your accounts." },

            new QuizQuestion { QuestionText = "Which of these is the strongest password?",
                Options = new[]{"password123","Cyber@2026!Secure","123456","qwerty"},
                CorrectIndex = 1, Explanation = "Strong passwords mix uppercase, lowercase, numbers and symbols, and avoid common words." },

            new QuizQuestion { QuestionText = "What does 2FA stand for?",
                Options = new[]{"Two-Factor Authentication","Two-File Access","Final Account Authorisation","Two-Factor Access"},
                CorrectIndex = 0, Explanation = "2FA (Two-Factor Authentication) adds a second verification step beyond your password." },

            new QuizQuestion { QuestionText = "True or False: A VPN hides your IP address and encrypts your traffic.",
                Options = new[]{"True","False"}, IsTrueFalse = true, CorrectIndex = 0,
                Explanation = "True. A VPN routes and encrypts your traffic, masking your real IP address." },

            new QuizQuestion { QuestionText = "Which is a common sign of a phishing email?",
                Options = new[]{"Urgent threatening language","Personalised birthday message from a friend","An invoice you were expecting","A newsletter you subscribed to"},
                CorrectIndex = 0, Explanation = "Phishing emails often create urgency or fear to pressure you into acting without thinking." },

            new QuizQuestion { QuestionText = "True or False: Public Wi-Fi is always safe for online banking.",
                Options = new[]{"True","False"}, IsTrueFalse = true, CorrectIndex = 1,
                Explanation = "False. Public Wi-Fi can be intercepted; always use a VPN or mobile data for sensitive transactions." },

            new QuizQuestion { QuestionText = "What is 'social engineering' in cybersecurity?",
                Options = new[]{"Designing social media apps","Manipulating people into revealing confidential info","A type of firewall","A networking protocol"},
                CorrectIndex = 1, Explanation = "Social engineering exploits human trust rather than technical flaws to gain access to information." },

            new QuizQuestion { QuestionText = "What should you do before clicking a link in an unexpected email?",
                Options = new[]{"Click it immediately","Hover over it to preview the real URL","Forward it to a friend","Reply asking if it's safe"},
                CorrectIndex = 1, Explanation = "Hovering reveals the real destination URL, helping you spot disguised or malicious links." },

            new QuizQuestion { QuestionText = "True or False: Keeping software updated helps protect against malware.",
                Options = new[]{"True","False"}, IsTrueFalse = true, CorrectIndex = 0,
                Explanation = "True. Updates patch known security vulnerabilities that malware commonly exploits." },

            new QuizQuestion { QuestionText = "Which of these is the safest way to store passwords?",
                Options = new[]{"Sticky note on your monitor","A password manager","A plain text file named 'passwords'","Memorise one password for everything"},
                CorrectIndex = 1, Explanation = "Password managers generate and securely store strong, unique passwords for every account." },

            new QuizQuestion { QuestionText = "What is ransomware?",
                Options = new[]{"Software that speeds up your PC","Malware that encrypts your files and demands payment","A type of antivirus","A browser extension"},
                CorrectIndex = 1, Explanation = "Ransomware locks or encrypts your files until a ransom is paid — regular backups are your best defence." },
        };
    }
}
