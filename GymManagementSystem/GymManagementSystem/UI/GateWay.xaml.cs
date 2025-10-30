using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using GymManagementSystem.Models;

namespace GymManagementSystem.UI
{
    public partial class Test : Window
    { 
        private bool isset = false;
        private Member member;
        private double balance=00.00;
        public Test(Models.Member memeber,double balance)
        {
            InitializeComponent();
            this.member = memeber;
            this.balance = balance;
            
            // Hook up Frame navigation events
            webView.Navigated += WebView_Navigated;
            
            DisplayPayHereForm();
        }

        private void DisplayPayHereForm()
        {
            string merchantId = "1228683";
            string merchantSecret = "MzI1Mjg5MjY5NTIxMjA2OTYxMDUxNDYwNTE2Mjg2MzE0Nzc1MjI0Mw==";
            string orderId = Guid.NewGuid().ToString();
            double amount = balance; // Use the balance passed to the constructor
            string currency = "LKR";

            string hash = CreateHash(merchantId, orderId, amount, currency, merchantSecret);

            string html = $@"
<!DOCTYPE html>
<html>
<body onload='document.forms[""payhereForm""].submit()'>
<form name='payhereForm' method='post' action='https://sandbox.payhere.lk/pay/checkout'>
<input type='hidden' name='merchant_id' value='{merchantId}' />
<input type='hidden' name='return_url' value='http://localhost/return' />
<input type='hidden' name='cancel_url' value='http://localhost/cancel' />
<input type='hidden' name='notify_url' value='http://localhost/notify' />
<input type='hidden' name='order_id' value='{orderId}' />
<input type='hidden' name='items' value='Test Product' />
<input type='hidden' name='currency' value='{currency}' />
<input type='hidden' name='amount' value='{amount.ToString("0.00", CultureInfo.InvariantCulture)}' />
<input type='hidden' name='first_name' value='Numidu' />
<input type='hidden' name='last_name' value='Dulanga' />
<input type='hidden' name='email' value='test@example.com' />
<input type='hidden' name='phone' value='0771234567' />
<input type='hidden' name='address' value='No.123, Test Street' />
<input type='hidden' name='city' value='Colombo' />
<input type='hidden' name='country' value='Sri Lanka' />
<input type='hidden' name='hash' value='{hash}' />
<noscript><button type='submit'>Continue to Payment</button></noscript>
</form>
</body>
</html>";

            // For Frame control, we need to save to a temp file and navigate
            string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "payhere_form.html");
            System.IO.File.WriteAllText(tempFile, html);
            webView.Navigate(new Uri(tempFile));
        }

        private string CreateHash(string merchantId, string orderId, double amount, string currency, string merchantSecret)
        {
            string amountFormatted = amount.ToString("0.00", CultureInfo.InvariantCulture);

            using (MD5 md5 = MD5.Create())
            {
                byte[] secretHash = md5.ComputeHash(Encoding.UTF8.GetBytes(merchantSecret));
                string md5Secret = BitConverter.ToString(secretHash).Replace("-", "").ToUpper();

                string hashString = $"{merchantId}{orderId}{amountFormatted}{currency}{md5Secret}";
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(hashString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
        }

        private void WebView_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri == null) return;
            
            string currentUrl = e.Uri.ToString();

            if (!isset && currentUrl.Contains("http://localhost/return"))
            {
                StoreData();
                isset = true;
                payementGateWay payment = new payementGateWay();
                payment.Show();
                this.Close();
            }
            else if (currentUrl.Contains("http://localhost/cancel"))
            {
                MessageBox.Show("Payment was cancelled.", "Payment Cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StoreData() { 
           Payment payment = new Payment
            {
                PaymentId = Guid.NewGuid().ToString(),
                MemberId = member.Id,
                Amount = balance,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Status = "Paid"
            };
            Services.InsertData.AddPayment(payment);
            
            MessageBox.Show(
                "Member payment has been recorded successfully.",
                "Payment Successful 🎉",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}