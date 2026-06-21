using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using CyberSecurityGUI.Data;
using CyberSecurityGUI.Services;

namespace CyberSecurityGUI
{
    public partial class MainWindow : Window 
    {
        private readonly Chatbot _bot = new Chatbot();
        private readonly TaskRepository _taskRepo = new TaskRepository();
        private readonly TaskAssistant _taskAssistant = new TaskAssistant(); 
        private readonly QuizEngine _quiz = new QuizEngine();

        private bool _quizAnswered = false;

        private static readonly SolidColorBrush BrushBotBubble = new SolidColorBrush(Color.FromRgb(0x1A, 0x00, 0x00));
        private static readonly SolidColorBrush BrushUserBubble = new SolidColorBrush(Color.FromRgb(0x0D, 0x0D, 0x0D));
        private static readonly SolidColorBrush BrushRed = new SolidColorBrush(Color.FromRgb(0xFF, 0x22, 0x22));
        private static readonly SolidColorBrush BrushOrange = new SolidColorBrush(Color.FromRgb(0xFF, 0x66, 0x00));

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                ShowWelcome();
                DatabaseHelper.EnsureDatabaseExists();
                RefreshTaskList();
                RefreshActivityLog();
            };
        }

        private void ShowWelcome()
        {
            PlayGreeting("greeting.wav");
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
                "New in this release: \ud83d\udcdd Task Assistant, \ud83c\udfae Quiz, \ud83d\udccb Activity Log — see the tabs above,\n" +
                "or just ask me things like \"Add task - enable 2FA\" or \"show activity log\".\n\n" +
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

        // ===================================================================
        // CHAT TAB
        // ===================================================================
        private void SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendUserMessage(text);
            InputBox.Clear();
            InputBox.Focus();

            // NLP task/reminder/log commands get first refusal (Task 3 + Task 4).
            // Anything it doesn't recognise falls through to the general chatbot.
            string response;
            if (_taskAssistant.TryHandle(text, out string nlpResponse))
                response = nlpResponse;
            else
                response = _bot.GetResponse(text);

            AppendBotMessage(response);

            // Keep the Tasks/Log tabs in sync if the chat was used to add a task or view the log.
            RefreshTaskList();
            RefreshActivityLog();
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

        // ===================================================================
        // TAB SWITCH — refresh data whenever the user lands on a tab
        // ===================================================================
        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            RefreshTaskList();
            RefreshActivityLog();
        }

        // ===================================================================
        // TASKS TAB (Database-backed Task Assistant)
        // ===================================================================
        private void RefreshTaskList()
        {
            try
            {
                TaskListItems.ItemsSource = _taskRepo.GetAllTasks();
            }
            catch (Exception ex)
            {
                TaskStatusText.Text = $"⚠ Could not load tasks from the database: {ex.Message}";
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text.Trim();
            string description = TaskDescBox.Text.Trim();
            DateTime? reminder = TaskReminderPicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(title))
            {
                TaskStatusText.Text = "⚠ Please enter a task title.";
                return;
            }

            try
            {
                _taskRepo.AddTask(title, description, reminder);
                ActivityLogger.Log($"Task added: '{title}'" + (reminder.HasValue ? $" (reminder set for {reminder:dd MMM yyyy})" : " (no reminder set)"));
                TaskStatusText.Text = $"✔ Task \"{title}\" added.";

                TaskTitleBox.Clear();
                TaskDescBox.Clear();
                TaskReminderPicker.SelectedDate = null;

                RefreshTaskList();
                RefreshActivityLog();
            }
            catch (Exception ex)
            {
                TaskStatusText.Text = $"⚠ Could not save task: {ex.Message}";
            }
        }

        private void RefreshTasksButton_Click(object sender, RoutedEventArgs e) => RefreshTaskList();

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                try
                {
                    _taskRepo.MarkCompleted(id);
                    ActivityLogger.Log($"Task #{id} marked as completed");
                    RefreshTaskList();
                    RefreshActivityLog();
                }
                catch (Exception ex)
                {
                    TaskStatusText.Text = $"⚠ {ex.Message}";
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                try
                {
                    _taskRepo.DeleteTask(id);
                    ActivityLogger.Log($"Task #{id} deleted");
                    RefreshTaskList();
                    RefreshActivityLog();
                }
                catch (Exception ex)
                {
                    TaskStatusText.Text = $"⚠ {ex.Message}";
                }
            }
        }

        // ===================================================================
        // QUIZ TAB (Mini-game)
        // ===================================================================
        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            var q = _quiz.StartQuiz(10);
            QuizFeedbackText.Text = "";
            NextQuestionButton.IsEnabled = false;
            StartQuizButton.Content = "RESTART QUIZ";
            RenderQuizQuestion(q);
            RefreshActivityLog();
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var q = _quiz.CurrentQuestion;
            if (q == null)
            {
                // Quiz finished
                QuizQuestionNumberText.Text = "Quiz complete!";
                QuizQuestionText.Text = _quiz.GetFinalFeedback();
                QuizOptionsPanel.ItemsSource = null;
                QuizFeedbackText.Text = "";
                NextQuestionButton.IsEnabled = false;
                RefreshActivityLog();
                return;
            }
            RenderQuizQuestion(q);
        }

        private void RenderQuizQuestion(Models.QuizQuestion q)
        {
            _quizAnswered = false;
            QuizQuestionNumberText.Text = $"Question {_quiz.CurrentNumber} of {_quiz.TotalQuestions}";
            QuizQuestionText.Text = q.QuestionText;
            QuizOptionsPanel.ItemsSource = q.Options;
            QuizFeedbackText.Text = "";
            QuizScoreText.Text = $"Score: {_quiz.Score} / {_quiz.TotalAnswered}";
            NextQuestionButton.IsEnabled = false;
        }

        private void QuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (_quizAnswered) return; // ignore double-clicks on an already-answered question
            if (sender is not Button btn || _quiz.CurrentQuestion == null) return;

            var q = _quiz.CurrentQuestion;
            int selectedIndex = Array.IndexOf(q.Options, btn.Tag?.ToString());
            var (isCorrect, explanation) = _quiz.SubmitAnswer(selectedIndex);

            _quizAnswered = true;
            QuizFeedbackText.Text = (isCorrect ? "✔ Correct! " : "✘ Not quite. ") + explanation;
            QuizScoreText.Text = $"Score: {_quiz.Score} / {_quiz.TotalAnswered}";
            NextQuestionButton.IsEnabled = true;
            NextQuestionButton.Content = _quiz.IsActive ? "NEXT QUESTION" : "SEE RESULTS";
        }

        // ===================================================================
        // ACTIVITY LOG TAB
        // ===================================================================
        private void RefreshActivityLog()
        {
            ActivityLogItems.ItemsSource = ActivityLogger.GetRecent(8);
        }

        private void ShowMoreLogButton_Click(object sender, RoutedEventArgs e)
        {
            ActivityLogItems.ItemsSource = ActivityLogger.GetAll();
        }

        private void RefreshLogButton_Click(object sender, RoutedEventArgs e) => RefreshActivityLog();
    }
}
