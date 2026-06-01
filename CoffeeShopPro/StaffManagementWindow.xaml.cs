using System.Windows;
using Microsoft.Data.Sqlite;

namespace CoffeeShopPro
{
    public partial class StaffManagementWindow : Window
    {
        private readonly StaffMember? _editingStaff;
        
        public StaffManagementWindow(StaffMember? staffToEdit = null)
        {
            _editingStaff = staffToEdit;
            
            InitializeComponent();
            
            if (_editingStaff != null)
            {
                txtTitle.Text = "✏️ Edit Staff";
                txtName.Text = _editingStaff.Name;
                
                // Select the appropriate role
                switch (_editingStaff.Role)
                {
                    case "Manager":
                        cmbRole.SelectedIndex = 0;
                        break;
                    case "Barista":
                        cmbRole.SelectedIndex = 1;
                        break;
                    case "Cashier":
                        cmbRole.SelectedIndex = 2;
                        break;
                }
                
                txtPinCode.Password = ""; // Don't show existing PIN for security
            }
            else
            {
                cmbRole.SelectedIndex = 1; // Default to Barista
            }
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = txtName.Text.Trim();
            var pinCode = txtPinCode.Password;
            var role = (cmbRole.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
            
            if (string.IsNullOrEmpty(name))
            {
                ShowError("Please enter a name");
                return;
            }
            
            if (_editingStaff == null)
            {
                // Adding new staff
                if (pinCode.Length != 4 || !int.TryParse(pinCode, out _))
                {
                    ShowError("PIN code must be 4 digits");
                    return;
                }
                
                try
                {
                    using var connection = DatabaseHelper.GetConnection();
                    connection.Open();
                    
                    var sql = "INSERT INTO Staff (Name, Role, PinCode) VALUES (@Name, @Role, @PinCode)";
                    using var cmd = new SqliteCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Role", role ?? "Barista");
                    cmd.Parameters.AddWithValue("@PinCode", pinCode);
                    cmd.ExecuteNonQuery();
                    
                    DialogResult = true;
                    Close();
                }
                catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
                {
                    ShowError("This PIN code is already in use. Please choose another.");
                }
                catch (Exception ex)
                {
                    ShowError($"Error: {ex.Message}");
                }
            }
            else
            {
                // Editing existing staff
                try
                {
                    using var connection = DatabaseHelper.GetConnection();
                    connection.Open();
                    
                    string sql;
                    if (!string.IsNullOrEmpty(pinCode))
                    {
                        if (pinCode.Length != 4 || !int.TryParse(pinCode, out _))
                        {
                            ShowError("PIN code must be 4 digits");
                            return;
                        }
                        
                        sql = "UPDATE Staff SET Name = @Name, Role = @Role, PinCode = @PinCode WHERE Id = @Id";
                        using var cmd = new SqliteCommand(sql, connection);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Role", role ?? "Barista");
                        cmd.Parameters.AddWithValue("@PinCode", pinCode);
                        cmd.Parameters.AddWithValue("@Id", _editingStaff.Id);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        sql = "UPDATE Staff SET Name = @Name, Role = @Role WHERE Id = @Id";
                        using var cmd = new SqliteCommand(sql, connection);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Role", role ?? "Barista");
                        cmd.Parameters.AddWithValue("@Id", _editingStaff.Id);
                        cmd.ExecuteNonQuery();
                    }
                    
                    DialogResult = true;
                    Close();
                }
                catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
                {
                    ShowError("This PIN code is already in use. Please choose another.");
                }
                catch (Exception ex)
                {
                    ShowError($"Error: {ex.Message}");
                }
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
