using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using Stripe;
using Stripe.Billing;
using static System.Net.Mime.MediaTypeNames;

namespace GymManagementSystem.UI
{
    /// <summary>
    /// Interaction logic for payementGateWay.xaml
    /// </summary>
    public partial class payementGateWay : Window
    {


        public payementGateWay()
        {
            InitializeComponent();

            DisplayCards();
        }



        public void DisplayCards()
        {
            var memberList = MemberService.GetAllMembers();
            var paymentList = PaymentService.GetAllPament();

            foreach (var member in memberList)
            {
                // Create beautiful card border with gradient and smooth shadow
                Border cardBorder = new Border
                {
                    CornerRadius = new CornerRadius(12),
                    BorderThickness = new Thickness(0),
                    Background = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 1),
                        GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(255, 255, 255), 0),
            new GradientStop(Color.FromRgb(250, 251, 255), 1)
        }
                    },
                    Margin = new Thickness(10, 8, 10, 8),
                    Padding = new Thickness(20, 18, 20, 18),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 15,
                        ShadowDepth = 3,
                        Opacity = 0.12,
                        Color = Color.FromRgb(100, 100, 150)
                    }
                };

                // Add subtle hover effect trigger
                cardBorder.MouseEnter += (s, e) =>
                {
                    cardBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 20,
                        ShadowDepth = 5,
                        Opacity = 0.18,
                        Color = Color.FromRgb(100, 100, 150)
                    };
                };

                cardBorder.MouseLeave += (s, e) =>
                {
                    cardBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 15,
                        ShadowDepth = 3,
                        Opacity = 0.12,
                        Color = Color.FromRgb(100, 100, 150)
                    };
                };

                // Grid layout (text on left, button on right)
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Member name with elegant styling
                TextBlock textBlock = new TextBlock
                {
                    Text = $"{member.FullName}",
                    FontSize = 17,
                    FontWeight = FontWeights.SemiBold,
                    FontFamily = new FontFamily("Segoe UI"),
                    Foreground = new SolidColorBrush(Color.FromRgb(45, 55, 72)),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                grid.Children.Add(textBlock);

                // Stylish action button
                Button actionButton = new Button
                {
                    Padding = new Thickness(24, 10, 24, 10),
                    Margin = new Thickness(15, 0, 0, 0),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 8,
                        ShadowDepth = 2,
                        Opacity = 0.25
                    }
                };

                // Button style template for rounded corners
                var buttonStyle = new Style(typeof(Border));
                buttonStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(8)));
                actionButton.Resources.Add(typeof(Border), buttonStyle);

                Grid.SetColumn(actionButton, 1);

                // Check payment status and style accordingly
                var payment = paymentList.FirstOrDefault(p => p.MemberId == member.Id);

                if (payment != null && payment.Status == "Paid")
                {
                    actionButton.Content = "✓ Paid";
                    actionButton.Click += (s, e) =>
                    {
                        MessageBox.Show(
                            "Member payment has been recorded successfully.",
                            "Payment Successful 🎉",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    };
                    actionButton.Background = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 0),
                        GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(52, 211, 153), 0),
            new GradientStop(Color.FromRgb(34, 197, 94), 1)
        }
                    };
                  
                    actionButton.Opacity = 0.85;
                }
                else
                {
                    actionButton.Content = "Pay Now";
                    actionButton.Background = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 0),
                        GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(59, 130, 246), 0),
            new GradientStop(Color.FromRgb(37, 99, 235), 1)
        }
                    };

                    // Hover effect for Pay button
                    actionButton.MouseEnter += (s, e) =>
                    {
                        actionButton.Background = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(37, 99, 235), 0),
                new GradientStop(Color.FromRgb(29, 78, 216), 1)
            }
                        };
                    };

                    actionButton.MouseLeave += (s, e) =>
                    {
                        actionButton.Background = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(59, 130, 246), 0),
                new GradientStop(Color.FromRgb(37, 99, 235), 1)
            }
                        };
                    };

                    actionButton.Click += (s, e) =>
                    {
                        PaymentDetails paymentDetails = new PaymentDetails(member);
                        paymentDetails.Show();
                        this.Close();
                    };
                }

                grid.Children.Add(actionButton);
                cardBorder.Child = grid;
                spanel.Children.Add(cardBorder);
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Members_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Trainers_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Equipments_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Payments_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}





