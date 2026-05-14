using System;
using System.Text;

namespace CyberSecurityGUI
{
    public class ChatbotModel
    {
        public string UserName { get; set; }
        public string UserInterest { get; set; }
        public string Topic { get; set; }
        public string Message { get; set; }
        public string Tips { get; set; }
        public string Sentiment { get; set; }
        public DateTime Timestamp { get; set; }

        public ChatbotModel()
        {
            Timestamp = DateTime.Now;
        }
    }
}