using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BaoCao
{
    public partial class Login : Form
    {
        // Khai báo chuỗi kết nối là biến lớp
        private string connectionString = "Data Source=TRONGNGHIA-ACER\\SQLEXPRESS;Initial Catalog=QLSV;Integrated Security=True";

        public Login()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '*';
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void Login_Load(object sender, EventArgs e)
        {
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Lấy email và mật khẩu từ TextBox
            string email = txtEmail.Text;
            string password = txtPassword.Text;

            // Gọi phương thức AuthenticateUser để kiểm tra đăng nhập
            bool isAuthenticated = AuthenticateUser(email, password);

            if (isAuthenticated)
            {
                MessageBox.Show("Login successful!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Thực hiện các hành động sau khi đăng nhập thành công
                // Chuyển đến form chính
                QLSV main = new QLSV(); // Tạo instance của form mới
                main.Show(); // Hiển thị form chính
                this.Hide(); // Ẩn form đăng nhập
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AuthenticateUser(string email, string password)
        {
            bool isAuthenticated = false;

            // Truy vấn SQL để kiểm tra email và mật khẩu
            string query = "SELECT COUNT(1) FROM Login WHERE Email = @Email AND Password = @Password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);

                try
                {
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    isAuthenticated = count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return isAuthenticated;
        }
    }
}
