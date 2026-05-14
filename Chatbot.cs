using System;
using System.Collections.Generic;

namespace CyberSecurityGUI
{
    // ===== DELEGATE =====
    public delegate string ResponseHandler(string input);

    public class Chatbot
    {
        private string userName = "";
        private string userInterest = "";
        private string lastTopic = "";
        private Random rand = new Random();
        private Sentiment sentiment = new Sentiment();

        // ===== DELEGATE USAGE =====
        private ResponseHandler responseHandler;

        public Chatbot()
        {
            responseHandler = ProcessResponse;

            // ===== SUBSCRIBE TO SENTIMENT EVENT =====
            sentiment.OnSentimentDetected += (s, input) =>
                Console.WriteLine($"[Sentiment detected: {s}]");
        }

        public string GetResponse(string input)
        {
            return responseHandler(input);
        }

        private string ProcessResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message!";

            string lower = input.ToLower().Trim();

            // ================= NAME =================
            if (lower.Contains("my name is"))
            {
                userName = input.ToLower().Replace("my name is", "").Trim();
                return $"Nice to meet you, {userName}! Ask me anything about cybersecurity.";
            }

            // ================= GREETINGS =================
            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey") || lower.Contains("howzit"))
            {
                string name = string.IsNullOrEmpty(userName) ? "" : $", {userName}";
                return $"Hey{name}! 👋 What cybersecurity topic can I help you with today?\n\n• Passwords\n• Phishing\n• Safe Browsing\n• Privacy";
            }

            // ================= SENTIMENT (via Sentiment class) =================
            string sentimentResponse = sentiment.GetSentiment(lower);
            if (!string.IsNullOrEmpty(sentimentResponse))
            {
                if (lower.Contains("worried") || lower.Contains("scared") || lower.Contains("anxious"))
                {
                    string tip = GetTopicResponse("phishing").Message;
                    return $"{sentimentResponse} 💙\n\nHere's something useful:\n\n{tip}";
                }

                if (lower.Contains("confused") || lower.Contains("don't understand") || lower.Contains("dont understand"))
                {
                    if (!string.IsNullOrEmpty(lastTopic))
                        return $"{sentimentResponse}\n\n{GetDetailedInfo(lastTopic).Tips}";
                    return $"{sentimentResponse}\n\nWhat topic is confusing you?\n• Passwords  • Phishing  • Privacy  • Scams";
                }

                if (lower.Contains("frustrated") || lower.Contains("angry") || lower.Contains("annoyed"))
                    return $"{sentimentResponse}\n\nWhat topic would you like help with?\n• Passwords  • Phishing  • Privacy  • Scams";

                if (lower.Contains("curious") || lower.Contains("interesting"))
                    return $"{sentimentResponse}\n\nWhat would you like to explore?\n• Passwords  • Phishing  • Privacy  • Scams  • Malware  • VPN  • 2FA";

                return sentimentResponse;
            }

            // ================= INTEREST MEMORY =================
            if (lower.Contains("interested in") || lower.Contains("i like") || lower.Contains("i care about"))
            {
                foreach (var key in topicResponses.Keys)
                {
                    if (lower.Contains(key))
                    {
                        userInterest = key;
                        return $"Got it! I'll remember that you're interested in {key}. It's a crucial part of staying safe online.\n\n{GetTopicResponse(key).Message}";
                    }
                }
            }

            // ================= FOLLOW UP =================
            if (lower.Contains("tell me more") || lower.Contains("explain more") || lower.Contains("more info") || lower.Contains("another tip") || lower.Contains("give me more"))
            {
                if (!string.IsNullOrEmpty(lastTopic))
                    return GetDetailedInfo(lastTopic).Tips;
                return "Sure! What topic would you like more on?\n• Passwords  • Phishing  • Safe Browsing  • Privacy";
            }

            // ================= HELP =================
            if (lower.Contains("help") || lower.Contains("what can you"))
            {
                return "I can help you with:\n\n🔑 Password safety\n🎣 Phishing & scams\n🌐 Safe browsing\n🔒 Privacy protection\n🦠 Malware\n🌍 VPN\n🔐 2FA\n\nJust ask about any of these!";
            }

            // ================= KEYWORD MATCHING =================
            foreach (var key in topicResponses.Keys)
            {
                if (lower.Contains(key))
                {
                    lastTopic = key;
                    if (!string.IsNullOrEmpty(userInterest) && userInterest == key)
                        return $"As someone interested in {key}, here's a tip:\n\n{GetTopicResponse(key).Message}";
                    return GetTopicResponse(key).Message;
                }
            }

            // ================= SAFE BROWSING =================
            if (lower.Contains("brows") || lower.Contains("website") || lower.Contains("internet") || lower.Contains("online safety") || lower.Contains("safe browsing"))
            {
                lastTopic = "safe browsing";
                string[] responses =
                {
                    "🌐 Always check for HTTPS and the padlock icon in the address bar before entering any personal info.",
                    "🌐 Avoid downloading software from unknown websites — stick to official sources or verified app stores.",
                    "🌐 Keep your browser and extensions updated. Outdated browsers have security vulnerabilities attackers can exploit."
                };
                return responses[rand.Next(responses.Length)];
            }

            // ================= THANK YOU =================
            if (lower.Contains("thank") || lower.Contains("thanks") || lower.Contains("appreciate"))
            {
                string[] responses =
                {
                    "You're welcome! Stay safe out there. 🛡",
                    "Happy to help! Remember — cybersecurity is everyone's responsibility.",
                    "Anytime! Keep your accounts locked down tight. 🔒"
                };
                return responses[rand.Next(responses.Length)];
            }

            // ================= GOODBYE =================
            if (lower.Contains("bye") || lower.Contains("goodbye") || lower.Contains("exit") || lower.Contains("quit"))
            {
                string name = string.IsNullOrEmpty(userName) ? "" : $", {userName}";
                return $"Stay safe online{name}! 👋 Come back anytime you have cybersecurity questions.";
            }

            // ================= DEFAULT =================
            string[] fallback =
            {
                "I didn't quite catch that. Try asking about:\n• Passwords\n• Phishing\n• Safe browsing\n• Privacy",
                "I specialise in cybersecurity topics. Ask me about passwords, scams, or online safety!",
                "Not sure about that one. Rephrase it or pick a topic: passwords, phishing, privacy, or safe browsing."
            };
            return fallback[rand.Next(fallback.Length)];
        }

        // ================= TOPIC DICTIONARY =================
        private Dictionary<string, List<string>> topicResponses = new Dictionary<string, List<string>>
        {
            {
                "password", new List<string>
                {
                    "🔑 Strong passwords use uppercase, lowercase, numbers, and symbols.\nExample: Cyber@2026!\n\nNever reuse passwords across different accounts.",
                    "🔑 Try using a passphrase — a string of random words like: Blue$Coffee*Moon2026\n\nIt's long, memorable, and hard to crack.",
                    "🔑 Use a password manager like Bitwarden or 1Password to generate and store strong passwords securely.\n\nNever write passwords on sticky notes!"
                }
            },
            {
                "phishing", new List<string>
                {
                    "🎣 Phishing is when attackers pretend to be trusted companies to steal your info.\n\nAlways check the sender's email address carefully before clicking anything.",
                    "🎣 Watch out for urgent language like 'Your account will be closed!' — scammers use panic to trick you.\n\nLegitimate companies won't ask for passwords via email.",
                    "🎣 Hover over links before clicking to see the real URL.\n\nIf the domain looks off (e.g. paypa1.com), don't click it — it's a scam."
                }
            },
            {
                "scam", new List<string>
                {
                    "🚨 If it sounds too good to be true, it is.\n\nNever send money or personal info to unverified sources.",
                    "🚨 Romance scams are on the rise.\n\nBe cautious of online relationships that quickly ask for financial help.",
                    "🚨 Tech support scams often use pop-ups.\n\nMicrosoft and Apple will NEVER call you unsolicited about your computer."
                }
            },
            {
                "privacy", new List<string>
                {
                    "🔒 Review app permissions regularly. Many apps request access to your camera, contacts, or location — only allow what's necessary.",
                    "🔒 Use a VPN on public Wi-Fi to encrypt your traffic and stop others from snooping on your data.",
                    "🔒 Enable two-factor authentication (2FA) on all important accounts — email, banking, and social media especially."
                }
            },
            {
                "malware", new List<string>
                {
                    "🦠 Never download software from unknown websites.\n\nAlways use official sources or verified app stores.",
                    "🦠 Keep your operating system updated.\n\nUpdates patch security vulnerabilities that malware exploits.",
                    "🦠 Ransomware can encrypt all your files.\n\nBack up your data regularly to an offline or cloud storage."
                }
            },
            {
                "vpn", new List<string>
                {
                    "🌍 A VPN encrypts your internet traffic.\n\nIt hides your IP address and protects your data on public networks.\n\nRecommended: ProtonVPN, Mullvad, NordVPN.",
                    "🌍 Not all VPNs are trustworthy.\n\nAvoid free VPNs — they often sell your data to third parties."
                }
            },
            {
                "2fa", new List<string>
                {
                    "🔐 Two-factor authentication adds a second layer of security.\n\nEven if your password is stolen, attackers can't access your account without your second factor.",
                    "🔐 Use an authenticator app like Google Authenticator or Authy.\n\nSMS-based 2FA can be intercepted via SIM swapping — an app is safer."
                }
            }
        };

        // ================= HELPERS =================
        private ChatbotModel GetTopicResponse(string topic)
        {
            foreach (var key in topicResponses.Keys)
            {
                if (topic.Contains(key))
                {
                    lastTopic = key;
                    var list = topicResponses[key];
                    return new ChatbotModel { Topic = key, Message = list[rand.Next(list.Count)] };
                }
            }
            return new ChatbotModel { Message = "I didn't catch that topic. Try asking about passwords, phishing, or privacy." };
        }

        private ChatbotModel GetDetailedInfo(string topic)
        {
            switch (topic)
            {
                case "password":
                    return new ChatbotModel { Topic = topic, Tips = "🔑 More on passwords:\n\n• Minimum 12 characters\n• Mix letters, numbers, symbols\n• Use a password manager\n• Enable 2FA everywhere\n• Change passwords after any breach" };
                case "phishing":
                    return new ChatbotModel { Topic = topic, Tips = "🎣 More on phishing:\n\n• Check sender email addresses carefully\n• Don't click links — go directly to the website\n• Report phishing emails to your IT/provider\n• Use email filters and spam detection" };
                case "scam":
                    return new ChatbotModel { Topic = topic, Tips = "🚨 More on scams:\n\n• Never send money to unverified contacts\n• Verify identities through official channels\n• Be suspicious of unsolicited contact\n• Report scams to local authorities" };
                case "privacy":
                    return new ChatbotModel { Topic = topic, Tips = "🔒 More on privacy:\n\n• Audit app permissions monthly\n• Use encrypted messaging (Signal)\n• Opt out of data tracking where possible\n• Use a VPN on public networks" };
                case "malware":
                    return new ChatbotModel { Topic = topic, Tips = "🦠 More on malware:\n\n• Keep OS and software updated\n• Use reputable antivirus software\n• Never open attachments from unknown senders\n• Back up data regularly" };
                case "safe browsing":
                    return new ChatbotModel { Topic = topic, Tips = "🌐 More on safe browsing:\n\n• Use a privacy-focused browser (Firefox, Brave)\n• Install uBlock Origin to block malicious ads\n• Clear cookies and cache regularly\n• Avoid public Wi-Fi without a VPN" };
                default:
                    return new ChatbotModel { Topic = topic, Tips = $"Let's go deeper into {topic}. Always double-check sources and stay alert online. Ask me something specific!" };
            }
        }
    }
}