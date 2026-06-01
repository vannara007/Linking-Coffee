using System;
using System.Linq;
using System.Windows;
using System.Drawing.Printing;

namespace CoffeeShopPro
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            LoadPrinters();
            LoadStaff();
        }
        
        private void LoadSettings()
        {
            txtShopName.Text = DatabaseHelper.GetSetting("ShopName", "Coffee Shop Pro");
            txtShopAddress.Text = DatabaseHelper.GetSetting("ShopAddress", "123 Main Street");
            txtShopPhone.Text = DatabaseHelper.GetSetting("ShopPhone", "(555) 123-4567");
            
            var taxRate = DatabaseHelper.GetSetting("TaxRate", "0.08");
            if (double.TryParse(taxRate, out var rate))
            {
                txtTaxRate.Text = (rate * 100).ToString("F2");
            }
            else
            {
                txtTaxRate.Text = "8.00";
            }
        }
        
        private void LoadPrinters()
        {
            try
            {
                var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
                cmbPrinters.ItemsSource = printers;
                
                var currentPrinter = DatabaseHelper.GetSetting("PrinterName");
                if (!string.IsNullOrEmpty(currentPrinter) && printers.Contains(currentPrinter))
                {
                    cmbPrinters.SelectedItem = currentPrinter;
                    txtSelectedPrinter.Text = currentPrinter;
                }
                else
                {
                    txtSelectedPrinter.Text = "No printer selected";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading printers: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadStaff()
        {
            var staff = DatabaseHelper.GetActiveStaff();
            dgStaff.ItemsSource = staff;
        }
        
        private void cmbPrinters_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbPrinters.SelectedItem is string printer)
            {
                txtSelectedPrinter.Text = printer;
            }
            else
            {
                txtSelectedPrinter.Text = "No printer selected";
            }
        }
        
        private void btnTestPrint_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPrinters.SelectedItem is not string printer)
            {
                MessageBox.Show("Please select a printer first.", "No Printer Selected", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                var testOrder = new Order
                {
                    Id = 999,
                    OrderDate = DateTime.Now,
                    StaffId = 1,
                    Items = new System.Collections.Generic.List<OrderItem>
                    {
                        new OrderItem { ProductName = "Test Item", Quantity = 1, UnitPrice = 1.00, TotalPrice = 1.00 }
                    },
                    Subtotal = 1.00,
                    Discount = 0,
                    Total = 1.00,
                    PaymentMethod = "Cash"
                };
                
                ReceiptPrinter.PrintReceipt(testOrder, printer);
                MessageBox.Show("Test print sent successfully!", "Test Print", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Print Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnAddStaff_Click(object sender, RoutedEventArgs e)
        {
            var staffWindow = new StaffManagementWindow();
            if (staffWindow.ShowDialog() == true)
            {
                LoadStaff();
            }
        }
        
        private void btnEditStaff_Click(object sender, RoutedEventArgs e)
        {
            if (dgStaff.SelectedItem is StaffMember staff)
            {
                var staffWindow = new StaffManagementWindow(staff);
                if (staffWindow.ShowDialog() == true)
                {
                    LoadStaff();
                }
            }
            else
            {
                MessageBox.Show("Please select a staff member to edit.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void btnDeactivateStaff_Click(object sender, RoutedEventArgs e)
        {
            if (dgStaff.SelectedItem is not StaffMember staff)
            {
                MessageBox.Show("Please select a staff member to deactivate.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show($"Are you sure you want to deactivate {staff.Name}?", 
                "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = DatabaseHelper.GetConnection();
                    connection.Open();
                    
                    var sql = "UPDATE Staff SET IsActive = 0 WHERE Id = @Id";
                    using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@Id", staff.Id);
                    cmd.ExecuteNonQuery();
                    
                    MessageBox.Show("Staff member deactivated successfully.", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LoadStaff();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deactivating staff: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseHelper.SaveSetting("ShopName", txtShopName.Text);
                DatabaseHelper.SaveSetting("ShopAddress", txtShopAddress.Text);
                DatabaseHelper.SaveSetting("ShopPhone", txtShopPhone.Text);
                
                if (double.TryParse(txtTaxRate.Text, out var rate))
                {
                    DatabaseHelper.SaveSetting("TaxRate", (rate / 100).ToString("F4"));
                }
                
                if (cmbPrinters.SelectedItem is string printer)
                {
                    DatabaseHelper.SaveSetting("PrinterName", printer);
                }
                else
                {
                    DatabaseHelper.SaveSetting("PrinterName", "");
                }
                
                MessageBox.Show("Settings saved successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
