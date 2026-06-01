using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoffeeShopApp
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<OrderItem> Cart { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            Cart = new ObservableCollection<OrderItem>();
            DataContext = this;
        }

        private void AddToCart(string itemName, decimal price)
        {
            var existingItem = Cart.FirstOrDefault(x => x.Name == itemName);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Cart.Add(new OrderItem { Name = itemName, Price = price, Quantity = 1 });
            }
            
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = Cart.Sum(x => x.Price * x.Quantity);
            lblTotal.Content = $"Total: ${total:F2}";
        }

        private void btnEspresso_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Espresso", 3.50m);
        }

        private void btnCappuccino_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Cappuccino", 4.50m);
        }

        private void btnLatte_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Latte", 4.75m);
        }

        private void btnMocha_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Mocha", 5.00m);
        }

        private void btnAmericano_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Americano", 3.75m);
        }

        private void btnCroissant_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Croissant", 3.25m);
        }

        private void btnMuffin_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Muffin", 3.00m);
        }

        private void btnCookie_Click(object sender, RoutedEventArgs e)
        {
            AddToCart("Cookie", 2.50m);
        }

        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Your cart is empty!", "Coffee Shop", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal total = Cart.Sum(x => x.Price * x.Quantity);
            MessageBox.Show($"Thank you for your order!\nTotal: ${total:F2}", "Coffee Shop", MessageBoxButton.OK, MessageBoxImage.Information);
            Cart.Clear();
            UpdateTotal();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Cart.Clear();
            UpdateTotal();
        }
    }

    public class OrderItem
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        
        public decimal LineTotal => Price * Quantity;
    }
}
