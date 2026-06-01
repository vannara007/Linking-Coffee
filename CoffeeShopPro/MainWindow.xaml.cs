using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoffeeShopPro
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<CartItem> _cart = new();
        private List<Product> _allProducts = new();
        private StaffMember? _currentStaff = null;
        
        public ICommand AddToCartCommand => new RelayCommand<Product>(AddToCart);
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            LoadCategories();
            LoadProducts();
            UpdateTotals();
            
            dgCart.ItemsSource = _cart;
        }
        
        private void LoadCategories()
        {
            var categories = DatabaseHelper.GetCategories();
            cmbCategory.ItemsSource = categories;
            cmbCategory.SelectedIndex = -1;
        }
        
        private void LoadProducts(string? category = null)
        {
            _allProducts = DatabaseHelper.GetProducts(category);
            lstProducts.ItemsSource = _allProducts;
        }
        
        private void AddToCart(Product product)
        {
            if (_currentStaff == null)
            {
                MessageBox.Show("Please login first!", "Login Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var existingItem = _cart.FirstOrDefault(c => c.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = 1
                });
            }
            
            dgCart.Items.Refresh();
            UpdateTotals();
        }
        
        private void UpdateTotals()
        {
            var subtotal = _cart.Sum(c => c.TotalPrice);
            var discountPercent = GetDiscountPercent();
            var discountAmount = subtotal * discountPercent / 100;
            var total = subtotal - discountAmount;
            
            txtSubtotal.Text = $"${subtotal:F2}";
            txtDiscountDisplay.Text = $"-${discountAmount:F2}";
            txtDiscountAmount.Text = $"${discountAmount:F2}";
            txtTotal.Text = $"${total:F2}";
        }
        
        private double GetDiscountPercent()
        {
            if (double.TryParse(txtDiscount.Text, out var percent))
            {
                return Math.Max(0, Math.Min(100, percent));
            }
            return 0;
        }
        
        private void txtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotals();
        }
        
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedItem is string category)
            {
                LoadProducts(category);
            }
        }
        
        private void btnAllCategories_Click(object sender, RoutedEventArgs e)
        {
            cmbCategory.SelectedIndex = -1;
            LoadProducts();
        }
        
        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) return;
            
            var result = MessageBox.Show("Are you sure you want to clear the cart?", 
                "Clear Cart", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _cart.Clear();
                txtDiscount.Text = "0";
                dgCart.Items.Refresh();
                UpdateTotals();
            }
        }
        
        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Cart is empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (_currentStaff == null)
            {
                MessageBox.Show("Please login first!", "Login Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Show payment dialog
            var paymentWindow = new PaymentWindow(_cart, _currentStaff.Id, GetDiscountPercent());
            if (paymentWindow.ShowDialog() == true)
            {
                // Print receipt if requested
                if (paymentWindow.PrintReceipt)
                {
                    var printerName = DatabaseHelper.GetSetting("PrinterName");
                    if (!string.IsNullOrEmpty(printerName))
                    {
                        ReceiptPrinter.PrintReceipt(paymentWindow.Order, printerName);
                    }
                    else
                    {
                        MessageBox.Show("No printer selected. Please configure printer in settings.", 
                            "Printer Not Configured", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                
                // Clear cart
                _cart.Clear();
                txtDiscount.Text = "0";
                dgCart.Items.Refresh();
                UpdateTotals();
            }
        }
        
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                _currentStaff = loginWindow.AuthenticatedStaff;
                if (_currentStaff != null)
                {
                    txtCurrentStaff.Text = $"{_currentStaff.Name} ({_currentStaff.Role})";
                    btnLogin.Visibility = Visibility.Collapsed;
                    btnLogout.Visibility = Visibility.Visible;
                }
            }
        }
        
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            _currentStaff = null;
            txtCurrentStaff.Text = "Not Logged In";
            btnLogin.Visibility = Visibility.Visible;
            btnLogout.Visibility = Visibility.Collapsed;
            _cart.Clear();
            txtDiscount.Text = "0";
            dgCart.Items.Refresh();
            UpdateTotals();
        }
        
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
    }
    
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;
        
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute((T?)parameter!);
        }
        
        public void Execute(object? parameter)
        {
            _execute((T?)parameter!);
        }
    }
}
