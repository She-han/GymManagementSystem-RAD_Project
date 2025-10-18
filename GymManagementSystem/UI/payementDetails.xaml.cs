using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GymManagementSystem.Models;
using Stripe;

namespace GymManagementSystem.UI
{
    public partial class PaymentDetails : Window
    {


        
        private Member memeber;

        public PaymentDetails(Member member)
        {
            InitializeComponent();
            this.memeber = member;
            txtName.Text = member.FullName;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double amount = 0;
            bool radio = false;
            if (radio = radioBtn1.IsChecked == true)
            {
                amount = 3000.00;
            }
            else if (radio = radioBtn2.IsChecked == true)
            {
                amount = 8000.00;
            } else if (radio = radioBtn3.IsChecked == true) {
                amount = 30000.00;
            }



            Test testWindow = new Test(memeber, amount);
            testWindow.Show();
            this.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back or close window
            this.Close();
            // Or if you need to show a previous window:
            // var previousWindow = new YourPreviousWindow();
            // previousWindow.Show();
            // this.Close();
        }

        private void Pa(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
             payementGateWay payment=  new payementGateWay();
            payment.Show();
            this.Close();
        }
    }
    }