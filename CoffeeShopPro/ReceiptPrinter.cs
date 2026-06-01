using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows;

namespace CoffeeShopPro
{
    public static class ReceiptPrinter
    {
        public static void PrintReceipt(Order order, string printerName)
        {
            try
            {
                var printDoc = new PrintDocument();
                
                if (!string.IsNullOrEmpty(printerName))
                {
                    printDoc.PrinterSettings.PrinterName = printerName;
                }
                
                var shopName = DatabaseHelper.GetSetting("ShopName", "Coffee Shop Pro");
                var shopAddress = DatabaseHelper.GetSetting("ShopAddress", "123 Main Street");
                var shopPhone = DatabaseHelper.GetSetting("ShopPhone", "(555) 123-4567");
                
                printDoc.PrintPage += (sender, e) =>
                {
                    var font = new Font("Courier New", 10);
                    var boldFont = new Font("Courier New", 10, FontStyle.Bold);
                    var smallFont = new Font("Courier New", 8);
                    
                    float y = 10;
                    float x = 10;
                    float lineWidth = 380;
                    
                    // Header
                    e.Graphics.DrawString(shopName, boldFont, Brushes.Black, x + lineWidth / 2 - 100, y);
                    y += 25;
                    e.Graphics.DrawString(shopAddress, smallFont, Brushes.Black, x + lineWidth / 2 - 100, y);
                    y += 18;
                    e.Graphics.DrawString(shopPhone, smallFont, Brushes.Black, x + lineWidth / 2 - 100, y);
                    y += 25;
                    
                    // Separator
                    e.Graphics.DrawLine(Pens.Black, x, y, x + lineWidth, y);
                    y += 15;
                    
                    // Order Info
                    e.Graphics.DrawString($"Order #: {order.Id}", smallFont, Brushes.Black, x, y);
                    y += 18;
                    e.Graphics.DrawString($"Date: {order.OrderDate:yyyy-MM-dd HH:mm}", smallFont, Brushes.Black, x, y);
                    y += 18;
                    
                    var staff = DatabaseHelper.GetActiveStaff().Find(s => s.Id == order.StaffId);
                    if (staff != null)
                    {
                        e.Graphics.DrawString($"Server: {staff.Name}", smallFont, Brushes.Black, x, y);
                        y += 18;
                    }
                    
                    // Separator
                    e.Graphics.DrawLine(Pens.Black, x, y, x + lineWidth, y);
                    y += 15;
                    
                    // Items Header
                    e.Graphics.DrawString("Item", boldFont, Brushes.Black, x, y);
                    e.Graphics.DrawString("Qty", boldFont, Brushes.Black, x + 200, y);
                    e.Graphics.DrawString("Price", boldFont, Brushes.Black, x + 250, y);
                    e.Graphics.DrawString("Total", boldFont, Brushes.Black, x + 320, y);
                    y += 20;
                    
                    // Items
                    foreach (var item in order.Items)
                    {
                        e.Graphics.DrawString(item.ProductName, font, Brushes.Black, x, y);
                        e.Graphics.DrawString(item.Quantity.ToString(), font, Brushes.Black, x + 200, y);
                        e.Graphics.DrawString($"${item.UnitPrice:F2}", font, Brushes.Black, x + 250, y);
                        e.Graphics.DrawString($"${item.TotalPrice:F2}", font, Brushes.Black, x + 320, y);
                        y += 18;
                    }
                    
                    // Separator
                    e.Graphics.DrawLine(Pens.Black, x, y, x + lineWidth, y);
                    y += 15;
                    
                    // Totals
                    var subtotalY = y;
                    e.Graphics.DrawString("Subtotal:", font, Brushes.Black, x + 250, subtotalY);
                    e.Graphics.DrawString($"${order.Subtotal:F2}", font, Brushes.Black, x + 320, subtotalY);
                    y += 18;
                    
                    if (order.Discount > 0)
                    {
                        e.Graphics.DrawString("Discount:", font, Brushes.Black, x + 250, y);
                        e.Graphics.DrawString($"-${order.Discount:F2}", font, Brushes.Black, x + 320, y);
                        y += 18;
                    }
                    
                    e.Graphics.DrawString("TOTAL:", boldFont, Brushes.Black, x + 250, y);
                    e.Graphics.DrawString($"${order.Total:F2}", boldFont, Brushes.Black, x + 320, y);
                    y += 25;
                    
                    // Payment
                    e.Graphics.DrawString($"Payment: {order.PaymentMethod ?? "Cash"}", font, Brushes.Black, x, y);
                    y += 25;
                    
                    // Footer
                    e.Graphics.DrawString("Thank you for your visit!", font, Brushes.Black, x + lineWidth / 2 - 80, y);
                    y += 18;
                    e.Graphics.DrawString("Please come again!", font, Brushes.Black, x + lineWidth / 2 - 70, y);
                };
                
                printDoc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public static string[] GetInstalledPrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
        }
    }
}
