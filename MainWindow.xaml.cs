using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace CyberSecurityGUI
{
    public partial class MainWindow : Window
    {
        private readonly Chatbot _bot = new Chatbot();

        private static readonly SolidColorBrush BrushBotBubble = new SolidColorBrush(Color.FromRgb(0x1A, 0x00, 0x00));
        private static readonly SolidColorBrush BrushUserBubble = new SolidColorBrush(Color.FromRgb(0x0D, 0x0D, 0x0D));
        private static readonly SolidColorBrush BrushRed = new SolidColorBrush(Color.FromRgb(0xFF, 0x22, 0x22));
        private static readonly SolidColorBrush BrushOrange = new SolidColorBrush(Color.FromRgb(0xFF, 0x66, 0x00));

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => ShowWelcome();
        }

        private void ShowWelcome()
        {
            PlayGreeting("CyberSecurityGreeting.wav");
            AppendBotMessage(
                "[ SYSTEM INITIALISED ]\n\n" +
                "  ██████╗██╗   ██╗██████╗ ███████╗██████╗ \n" +
                "  ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗\n" +
                "  ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝\n" +
                "  ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗\n" +
                "  ╚██████╗   ██║   ██████╔╝███████╗██║  ██║\n" +
                "   ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝\n\n" +
                "Cybersecurity Intelligence Assistant — ONLINE\n\n" +
                "Available threat modules:\n" +
                "> PASSWORDS   > PHISHING   > SCAMS\n" +
                "> PRIVACY     > MALWARE    > VPN    > 2FA\n\n" +
                "Type a topic or command to begin."
            );
        }

        private void PlayGreeting(string fileName)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                var player = new System.Media.SoundPlayer(path);
                player.Play();
            }
            catch { }
        }

        private void SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendUserMessage(text);
            InputBox.Clear();
            InputBox.Focus();

            string response = _bot.GetResponse(text);
            AppendBotMessage(response);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                InputBox.Text = btn.Tag.ToString();
                SendMessage();
            }
        }

        private void AppendBotMessage(string message) => AddMessageRow(message, BrushBotBubble, BrushRed, HorizontalAlignment.Left, true);
        private void AppendUserMessage(string message) => AddMessageRow(message, BrushUserBubble, BrushOrange, HorizontalAlignment.Right, false);

        private void AddMessageRow(string message, Brush background, Brush labelColor,
            HorizontalAlignment align, bool isBot)
        {
            var row = new Grid
            {
                Margin = new Thickness(0, 4, 0, 4),
                Opacity = 0
            };

            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition());

            var bubble = BuildBubble(message, background, labelColor, align, isBot);
            Grid.SetColumn(bubble, isBot ? 0 : 1);
            row.Children.Add(bubble);

            MessagePanel.Children.Add(row);

            Dispatcher.InvokeAsync(() =>
            {
                ChatScroll.UpdateLayout();
                ChatScroll.ScrollToEnd();
            });

            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            row.BeginAnimation(OpacityProperty, anim);
        } 

        private Border BuildBubble(string message, Brush background,
            Brush labelColor, HorizontalAlignment align, bool isBot)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = isBot ? "⚠ CYBERBOT" : "// YOU",
                Foreground = labelColor, 
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            });

            stack.Children.Add(new TextBlock 
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8)),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap
            });

            return new Border
            {
                Background = background,
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(14),
                Margin = new Thickness(5),
                MaxWidth = 540,
                HorizontalAlignment = align,
                Child = stack,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x22, 0x22)),
                BorderThickness = new Thickness(1, 0, 0, 0),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0xFF, 0x00, 0x00),
                    BlurRadius = 8,
                    Opacity = 0.15,
                    ShadowDepth = 0
                }
            };
        }
    }
}