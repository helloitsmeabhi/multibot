using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AutoGen.Core;
using static Google.Cloud.AIPlatform.V1.PublisherModel.Types.CallToAction.Types;
using AutoGen.Gemini;
using Microsoft.Toolkit.Parsers.Markdown;
using Markdig;
namespace multibot
{

       
        public partial class MainWindow : Window
       {
        private bool _isProcessing = false;
        private bool isPanelCollapsed = false;

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.RegisterAttached(
                "IsCollapsed",
                typeof(bool),
                typeof(MainWindow),
                new PropertyMetadata(false));
        public MainWindow()
        {
            InitializeComponent();
            SendButton.Click += SendButton_Click;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = "";
                //Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (apiKey is null)
            {
                Console.WriteLine("Please set GOOGLE_GEMINI_API_KEY environment variable.");
                return;
            }

            var geminiAgent = new GeminiChatAgent(
                    name: "gemini",
                    model: "gemini-1.5-flash-001",
                    apiKey: apiKey,
                    systemMessage: "You are a helpful C# engineer, put your code between ```csharp and ```, don't explain the code")
                .RegisterMessageConnector()
                .RegisterPrintMessage();
            if (_isProcessing || string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            try
            {
                _isProcessing = true;
                SendButton.IsEnabled = false;
                MessageTextBox.IsEnabled = false;

                string userMessage = MessageTextBox.Text;
                AddMessageToChat("User", userMessage);
                MessageTextBox.Clear();

                var response = await geminiAgent.SendAsync(userMessage);
                
                AddMessageToChat("Bot", response.FormatMessage());
            }
            catch (Exception ex)
            {
                AddMessageToChat("System", $"Error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
                SendButton.IsEnabled = true;
                MessageTextBox.IsEnabled = true;
                MessageTextBox.Focus();
            }
        }

        private void AddMessageToChat(string sender, string message)
        {
            var md = Markdown.ToPlainText(message);
            var messagePanel = new Border
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                Background = sender == "User" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCF8C6"))
                                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E8E8"))
            };

            var messageBlock = new TextBlock
            {
                Text = $"{sender}: {md}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0)
            };

            messagePanel.Child = messageBlock;

            // Add to the left or right depending on sender
            var containerPanel = new StackPanel
            {
                Margin = new Thickness(5),
                HorizontalAlignment = sender == "User" ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 300
            };

            containerPanel.Children.Add(messagePanel);
            ChatMessagesPanel.Children.Add(containerPanel);

            // Auto-scroll to bottom
            if (ChatMessagesPanel.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToEnd();
            }
        }


        // Add Enter key support for sending messages
        private void MessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !e.KeyboardDevice.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                e.Handled = true;
                if (!_isProcessing && !string.IsNullOrWhiteSpace(MessageTextBox.Text))
                {
                    SendButton_Click(sender, e);
                }
            }
        }

        private void TogglePanel_Click(object sender, RoutedEventArgs e)
        {
            isPanelCollapsed = !isPanelCollapsed;
            SidePanel.SetValue(IsCollapsedProperty, isPanelCollapsed);

            var animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase()
            };

            if (isPanelCollapsed)
            {
                animation.To = -75;  // Match your ColumnDefinition Width
                ToggleButton.Content = "☰";
            }
            else
            {
                animation.To = 0;     // Slide back in
                ToggleButton.Content = "☰";
            }

            SidePanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings clicked!");
        }

    }
}