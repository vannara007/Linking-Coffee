using System.Windows;

namespace CoffeeShopPro
{
    public partial class LoginWindow : Window
    {
        public StaffMember? AuthenticatedStaff { get; private set; }
        
        public LoginWindow()
        {
            InitializeComponent();
            txtPinCode.Focus();
        }
        
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var pinCode = txtPinCode.Password;
            
            if (string.IsNullOrEmpty(pinCode))
            {
                ShowError("Please enter PIN code");
                return;
            }
            
            var staff = DatabaseHelper.AuthenticateStaff(pinCode);
            if (staff != null)
            {
                AuthenticatedStaff = staff;
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError("Invalid PIN code. Please try again.");
                txtPinCode.Password = "";
                txtPinCode.Focus();
            }
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}
