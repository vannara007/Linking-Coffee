using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace CoffeeShopPro
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "coffeeshop.db");
        
        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            
            // Create Products table
            var createProducts = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Price REAL NOT NULL,
                    IsActive INTEGER DEFAULT 1
                )";
            
            using (var cmd = new SqliteCommand(createProducts, connection))
            {
                cmd.ExecuteNonQuery();
            }
            
            // Create Staff table
            var createStaff = @"
                CREATE TABLE IF NOT EXISTS Staff (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    PinCode TEXT NOT NULL UNIQUE,
                    IsActive INTEGER DEFAULT 1
                )";
            
            using (var cmd = new SqliteCommand(createStaff, connection))
            {
                cmd.ExecuteNonQuery();
            }
            
            // Create Settings table
            var createSettings = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                )";
            
            using (var cmd = new SqliteCommand(createSettings, connection))
            {
                cmd.ExecuteNonQuery();
            }
            
            // Create Orders table
            var createOrders = @"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderDate TEXT NOT NULL,
                    StaffId INTEGER,
                    Subtotal REAL NOT NULL,
                    Discount REAL NOT NULL,
                    Total REAL NOT NULL,
                    PaymentMethod TEXT,
                    FOREIGN KEY (StaffId) REFERENCES Staff(Id)
                )";
            
            using (var cmd = new SqliteCommand(createOrders, connection))
            {
                cmd.ExecuteNonQuery();
            }
            
            // Create OrderItems table
            var createOrderItems = @"
                CREATE TABLE IF NOT EXISTS OrderItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    TotalPrice REAL NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )";
            
            using (var cmd = new SqliteCommand(createOrderItems, connection))
            {
                cmd.ExecuteNonQuery();
            }
            
            // Insert default products if empty
            var checkProducts = "SELECT COUNT(*) FROM Products";
            using (var cmd = new SqliteCommand(checkProducts, connection))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    var insertProducts = @"
                        INSERT INTO Products (Name, Category, Price) VALUES
                        ('Espresso', 'Coffee', 3.50),
                        ('Cappuccino', 'Coffee', 4.50),
                        ('Latte', 'Coffee', 4.75),
                        ('Americano', 'Coffee', 3.75),
                        ('Mocha', 'Coffee', 5.00),
                        ('Caramel Macchiato', 'Coffee', 5.25),
                        ('Croissant', 'Pastry', 3.25),
                        ('Muffin', 'Pastry', 3.50),
                        ('Bagel', 'Pastry', 2.75),
                        ('Cookie', 'Pastry', 2.50),
                        ('Sandwich', 'Food', 6.50),
                        ('Salad', 'Food', 7.00)";
                    
                    using var insertCmd = new SqliteCommand(insertProducts, connection);
                    insertCmd.ExecuteNonQuery();
                }
            }
            
            // Insert default staff if empty
            var checkStaff = "SELECT COUNT(*) FROM Staff";
            using (var cmd = new SqliteCommand(checkStaff, connection))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    var insertStaff = @"
                        INSERT INTO Staff (Name, Role, PinCode) VALUES
                        ('Admin User', 'Manager', '0000'),
                        ('John Doe', 'Barista', '1234'),
                        ('Jane Smith', 'Cashier', '5678')";
                    
                    using var insertCmd = new SqliteCommand(insertStaff, connection);
                    insertCmd.ExecuteNonQuery();
                }
            }
            
            // Insert default settings if empty
            var checkSetting = "SELECT COUNT(*) FROM Settings WHERE Key = 'PrinterName'";
            using (var cmd = new SqliteCommand(checkSetting, connection))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    var insertSettings = @"
                        INSERT INTO Settings (Key, Value) VALUES
                        ('PrinterName', ''),
                        ('ShopName', 'Coffee Shop Pro'),
                        ('ShopAddress', '123 Main Street, City'),
                        ('ShopPhone', '(555) 123-4567'),
                        ('TaxRate', '0.08')";
                    
                    using var insertCmd = new SqliteCommand(insertSettings, connection);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
        
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection($"Data Source={DbPath}");
        }
        
        public static List<Product> GetProducts(string? category = null)
        {
            var products = new List<Product>();
            using var connection = GetConnection();
            connection.Open();
            
            var sql = "SELECT Id, Name, Category, Price FROM Products WHERE IsActive = 1";
            if (!string.IsNullOrEmpty(category))
            {
                sql += " AND Category = @Category";
            }
            
            using var cmd = new SqliteCommand(sql, connection);
            if (!string.IsNullOrEmpty(category))
            {
                cmd.Parameters.AddWithValue("@Category", category);
            }
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Price = reader.GetDouble(3)
                });
            }
            
            return products;
        }
        
        public static List<string> GetCategories()
        {
            var categories = new List<string>();
            using var connection = GetConnection();
            connection.Open();
            
            var sql = "SELECT DISTINCT Category FROM Products WHERE IsActive = 1 ORDER BY Category";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString(0));
            }
            
            return categories;
        }
        
        public static List<StaffMember> GetActiveStaff()
        {
            var staff = new List<StaffMember>();
            using var connection = GetConnection();
            connection.Open();
            
            var sql = "SELECT Id, Name, Role FROM Staff WHERE IsActive = 1 ORDER BY Name";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                staff.Add(new StaffMember
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Role = reader.GetString(2)
                });
            }
            
            return staff;
        }
        
        public static StaffMember? AuthenticateStaff(string pinCode)
        {
            using var connection = GetConnection();
            connection.Open();
            
            var sql = "SELECT Id, Name, Role FROM Staff WHERE PinCode = @PinCode AND IsActive = 1";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@PinCode", pinCode);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new StaffMember
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Role = reader.GetString(2)
                };
            }
            
            return null;
        }
        
        public static string GetSetting(string key, string defaultValue = "")
        {
            using var connection = GetConnection();
            connection.Open();
            
            var sql = "SELECT Value FROM Settings WHERE Key = @Key";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Key", key);
            
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? defaultValue;
        }
        
        public static void SaveSetting(string key, string value)
        {
            using var connection = GetConnection();
            connection.Open();
            
            var sql = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@Key, @Value)";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Value", value);
            cmd.ExecuteNonQuery();
        }
        
        public static int SaveOrder(Order order)
        {
            using var connection = GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var insertOrder = @"
                    INSERT INTO Orders (OrderDate, StaffId, Subtotal, Discount, Total, PaymentMethod)
                    VALUES (@OrderDate, @StaffId, @Subtotal, @Discount, @Total, @PaymentMethod);
                    SELECT last_insert_rowid();";
                
                int orderId;
                using (var cmd = new SqliteCommand(insertOrder, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@StaffId", order.StaffId);
                    cmd.Parameters.AddWithValue("@Subtotal", order.Subtotal);
                    cmd.Parameters.AddWithValue("@Discount", order.Discount);
                    cmd.Parameters.AddWithValue("@Total", order.Total);
                    cmd.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod ?? "Cash");
                    orderId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                foreach (var item in order.Items)
                {
                    var insertItem = @"
                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
                        VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice, @TotalPrice)";
                    
                    using (var cmd = new SqliteCommand(insertItem, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                transaction.Commit();
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public double Price { get; set; }
    }
    
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice => UnitPrice * Quantity;
    }
    
    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
    }
    
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int StaffId { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public string? PaymentMethod { get; set; }
    }
    
    public class StaffMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
