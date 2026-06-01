# Coffee Shop Pro - WPF POS Application

A complete WPF Point of Sale (POS) application for coffee shops with SQLite backend, receipt printing, staff management, and more.

## Features

### 🛒 Point of Sale
- Interactive product catalog with category filtering
- Shopping cart with quantity management
- Percentage-based discounts
- Real-time total calculation
- Multiple payment methods (Cash, Credit Card, Mobile Pay)

### 🖨 Receipt Printing
- Thermal receipt printer support
- Customizable shop information on receipts
- Test print functionality
- Printer selection in settings

### 👥 Staff Management
- PIN-based authentication
- Role-based access (Manager, Barista, Cashier)
- Add, edit, and deactivate staff members
- Track which staff member processed each order

### ⚙ Settings
- Customize shop name, address, and phone
- Configure tax rate
- Select receipt printer from installed printers
- Manage staff members

### 📊 Database
- SQLite database for local storage
- Automatic database initialization
- Products, staff, orders, and settings tables
- Default data seeding

## Default Login Credentials

| Name | Role | PIN Code |
|------|------|----------|
| Admin User | Manager | 0000 |
| John Doe | Barista | 1234 |
| Jane Smith | Cashier | 5678 |

## Project Structure

```
CoffeeShopPro/
├── App.xaml                 # Application definition
├── App.xaml.cs              # Application startup logic
├── MainWindow.xaml          # Main POS interface
├── MainWindow.xaml.cs       # Main window logic
├── LoginWindow.xaml         # Staff login dialog
├── LoginWindow.xaml.cs      # Login logic
├── PaymentWindow.xaml       # Checkout/payment dialog
├── PaymentWindow.xaml.cs    # Payment processing
├── SettingsWindow.xaml      # Settings interface
├── SettingsWindow.xaml.cs   # Settings management
├── StaffManagementWindow.xaml  # Staff CRUD interface
├── StaffManagementWindow.xaml.cs  # Staff management logic
├── DatabaseHelper.cs        # SQLite database operations
├── ReceiptPrinter.cs        # Receipt printing logic
└── CoffeeShopPro.csproj     # Project file
```

## Requirements

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or VS Code
- Receipt printer (optional, for printing receipts)

## Installation & Build

1. **Open in Visual Studio:**
   ```
   Open CoffeeShopPro.sln in Visual Studio 2022
   ```

2. **Or build via command line:**
   ```bash
   cd CoffeeShopPro
   dotnet restore
   dotnet build
   dotnet run
   ```

3. **Publish as standalone app:**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true
   ```

## Usage Guide

### First Time Setup
1. Launch the application
2. Click "Login" button (bottom right)
3. Enter PIN code: `0000` (Admin)
4. Navigate to Settings (⚙ button, top right)
5. Configure shop information
6. Select your receipt printer (if available)
7. Save settings

### Processing an Order
1. Login with your staff credentials
2. Click products to add them to cart
3. Adjust discount percentage if needed
4. Click "Checkout"
5. Select payment method
6. Check "Print Receipt" if needed
7. Click "Complete Sale"

### Managing Staff
1. Go to Settings → Staff tab
2. Click "Add Staff" to create new employee
3. Enter name, role, and 4-digit PIN
4. Use "Edit" to modify existing staff
5. Use "Deactivate" to remove access (soft delete)

## Database Schema

### Products
- Id, Name, Category, Price, IsActive

### Staff
- Id, Name, Role, PinCode, IsActive

### Orders
- Id, OrderDate, StaffId, Subtotal, Discount, Total, PaymentMethod

### OrderItems
- Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice

### Settings
- Key, Value

## Customization

### Adding Products
Products can be added directly to the SQLite database:
```sql
INSERT INTO Products (Name, Category, Price) VALUES ('New Item', 'Coffee', 4.99);
```

### Changing Theme Colors
Edit colors in `App.xaml` and `MainWindow.xaml`:
- Primary: `#6F4E37` (Coffee brown)
- Accent: `#FFD700` (Gold)
- Background: `#F5F5DC` (Beige)

## Troubleshooting

### Printer Not Working
1. Ensure printer is installed and set as default in Windows
2. Check printer name in Settings → Printer tab
3. Use "Test Print" to verify configuration
4. Restart application after installing new printers

### Database Issues
The database file (`coffeeshop.db`) is created in the application directory on first run. If corrupted, delete it and restart the app to regenerate.

## License

This project is provided as-is for educational and commercial use.

## Support

For issues or feature requests, please check the source code documentation or contact the development team.
