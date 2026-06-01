using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CoffeeShopPro
{
    public partial class PaymentWindow : Window
    {
        private readonly List<CartItem> _cartItems;
        private readonly int _staffId;
        private readonly double _discountPercent;
        
        public Order Order { get; private set; } = new();
        public bool PrintReceipt => chkPrintReceipt.IsChecked ?? true;
        
        public PaymentWindow(List<CartItem> cartItems, int staffId, double discountPercent)
        {
            _cartItems = cartItems;
            _staffId = staffId;
            _discountPercent = discountPercent;
            
            InitializeComponent();
            
            LoadOrderSummary();
        }
        
        private void LoadOrderSummary()
        {
            dgOrderItems.ItemsSource = _cartItems;
            
            var subtotal = _cartItems.Sum(c => c.TotalPrice);
            var discountAmount = subtotal * _discountPercent / 100;
            var total = subtotal - discountAmount;
            
            txtSubtotal.Text = $"${subtotal:F2}";
            txtDiscount.Text = $"-${discountAmount:F2}";
            txtTotal.Text = $"${total:F2}";
        }
        
        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var subtotal = _cartItems.Sum(c => c.TotalPrice);
                var discountAmount = subtotal * _discountPercent / 100;
                var total = subtotal - discountAmount;
                
                string paymentMethod = "Cash";
                if (rbCard.IsChecked == true)
                    paymentMethod = "Credit Card";
                else if (rbMobile.IsChecked == true)
                    paymentMethod = "Mobile Pay";
                
                Order = new Order
                {
                    OrderDate = DateTime.Now,
                    StaffId = _staffId,
                    Items = _cartItems.Select(c => new OrderItem
                    {
                        ProductId = c.ProductId,
                        ProductName = c.ProductName,
                        Quantity = c.Quantity,
                        UnitPrice = c.UnitPrice,
                        TotalPrice = c.TotalPrice
                    }).ToList(),
                    Subtotal = subtotal,
                    Discount = discountAmount,
                    Total = total,
                    PaymentMethod = paymentMethod
                };
                
                // Save order to database
                var orderId = DatabaseHelper.SaveOrder(Order);
                Order.Id = orderId;
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing sale: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
