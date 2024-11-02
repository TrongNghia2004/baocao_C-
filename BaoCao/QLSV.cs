using System;
using System.Data;
using System.Drawing; // Thư viện cho Image
using System.IO;     // Thư viện cho MemoryStream
using System.Data.SqlClient; // Thư viện cho SqlConnection
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BaoCao
{
    public partial class QLSV : Form
    {
        private string connectionString = "Data Source=TRONGNGHIA-ACER\\SQLEXPRESS;Initial Catalog=QLSV;Integrated Security=True";
        private DataTable dt;
        private bool isRowAdded = false;

        public QLSV()
        {
            InitializeComponent();
            LoadData();
            PopulateComboBoxes();
            rbMale.Checked = true; // Default gender is Male
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Masv, Name, Age, Gender, Hometown, CurrentResidence, Department, CitizenID, Class, Image FROM Customers", conn);
                dt = new DataTable();
                adapter.Fill(dt);

                // Gán DataTable vào DataGridView
                dgvCustomer.DataSource = dt;
            }
        }


        private void PopulateComboBoxes()
        {
            var provinces = new[] {
    "Ha Noi", "Ho Chi Minh", "Da Nang", "Can Tho", "Hai Phong",
"Nha Trang", "Binh Duong", "Dong Nai", "Vinh Long", "An Giang",
"Bac Giang", "Bac Ninh", "Ben Tre", "Binh Dinh", "Binh Thuan",
"Ca Mau", "Gia Lai", "Ha Giang", "Ha Nam", "Ha Tinh",
"Ho Chi Minh", "Hung Yen", "Khanh Hoa", "Kien Giang", "Kon Tum",
"Lang Son", "Lao Cai", "Long An", "Nam Dinh", "Ninh Binh",
"Ninh Thuan", "Phu Tho", "Phu Yen", "Quang Binh", "Quang Nam",
"Quang Ngai", "Quang Ninh", "Soc Trang", "Son La", "Tay Ninh",
"Thai Binh", "Thai Nguyen", "Thanh Hoa", "Thua Thien Hue", "Tien Giang",
"Tra Vinh", "Tuyen Quang", "Vinh Phuc", "Yen Bai"

};

            cbHometown.Items.AddRange(provinces);
            cbCurrentResidence.Items.AddRange(provinces);

            var departments = new[] {
    "Khoa Cong nghe thong tin",
"Khoa Co khi",
"Khoa Ngoai ngu",
"Khoa Kinh te",
"Khoa Y te",
"Khoa Luat",
"Khoa Dien - Dien tu",
"Khoa Khoa hoc ung dung",
"Khoa Thuong mai",
"Khoa Du lich",
"Khoa Quan tri kinh doanh",
"Khoa Ke toan",
"Khoa Tai chinh - Ngan hang",
"Khoa Marketing",
"Khoa Ky thuat xay dung",
"Khoa Thiet ke do hoa",
"Khoa Thoi trang",
"Khoa Bao tri va sua chua o to"

};

            cbDepartment.Items.AddRange(departments);
        }

        private bool ValidateInputs()
        {
            // Kiểm tra ID
            if (tbId.Text.Length != 10 || !tbId.Text.All(char.IsDigit))
            {
                MessageBox.Show("ID must be a string of 10 digits, containing only numbers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra Citizen ID
            if (tbCitizenID.Text.Length != 12 || !tbCitizenID.Text.All(char.IsDigit))
            {
                MessageBox.Show("Citizen ID must be a string of 12 digits, containing only numbers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra tên
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra nếu tên chứa số hoặc ký tự đặc biệt
            if (tbName.Text.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Name cannot contain digits or special characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra tuổi
            if (!int.TryParse(tbAge.Text, out int age) || age <= 0)
            {
                MessageBox.Show("Age must be a positive integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra nếu tuổi chứa ký tự chữ cái hoặc ký tự đặc biệt
            if (tbAge.Text.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Age must only contain digits and cannot contain letters or special characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra Hometown, Current Residence và Department
            if (cbHometown.SelectedItem == null || cbCurrentResidence.SelectedItem == null || cbDepartment.SelectedItem == null)
            {
                MessageBox.Show("You must select Hometown, Current Residence, and Department.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra Class
            if (string.IsNullOrWhiteSpace(tbClass.Text))
            {
                MessageBox.Show("Class cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra nếu Class chứa ký tự đặc biệt
            if (tbClass.Text.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Class cannot contain special characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Kiểm tra ảnh
            if (pbStudentImage.Image == null)
            {
                MessageBox.Show("You must select an image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true; // Nếu tất cả các điều kiện đều hợp lệ
        }


        // Method to check if the ID exists in the database
        private bool IdExists(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Customers WHERE Masv = @Masv", conn);
                cmd.Parameters.AddWithValue("@Masv", id);

                int count = (int)cmd.ExecuteScalar();
                return count > 0; // Returns true if the ID exists
            }
        }

        private byte[] ImageToByteArray(Image imageIn)
        {
            // Kiểm tra xem hình ảnh có khác null không
            if (imageIn == null)
            {
                throw new ArgumentNullException(nameof(imageIn), "Image cannot be null.");
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                // Xử lý lỗi GDI+
                MessageBox.Show($"Error converting image to byte array: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // Trả về null nếu có lỗi
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khác
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // Trả về null nếu có lỗi
            }
        }
        public void SaveImage(string filePath, Image image)
        {
            try
            {
                // Ensure the directory exists
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Use FileStream to avoid file locking issues
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    image.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }


        //private string filePath = ""; // Biến toàn cục để lưu đường dẫn ảnh

        //// Sự kiện chọn ảnh và hiển thị trong PictureBox
        //private void btnSelectImage_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        filePath = openFileDialog.FileName; // Lưu đường dẫn ảnh
        //        pbStudentImage.Image = Image.FromFile(filePath); // Hiển thị ảnh trong PictureBox
        //    }
        //}

        // Sự kiện thêm hoặc cập nhật thông tin lên DataGridView và cơ sở dữ liệu
        private void btEdit_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                // Kiểm tra nếu không có ảnh thì không thể thêm
                if (pbStudentImage.Image == null)
                {
                    MessageBox.Show("You must select an image before adding a student.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Ngừng thực hiện nếu không có ảnh
                }

                int idx = dgvCustomer.CurrentCell.RowIndex;

                if (idx < 0 || idx >= dt.Rows.Count)
                {
                    MessageBox.Show("No student selected for editing.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow rowToEdit = dt.Rows[idx];
                string oldMasv = rowToEdit["Masv"].ToString();

                if (tbId.Text != oldMasv)
                {
                    MessageBox.Show("You cannot change the student ID.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbId.Text = oldMasv;
                    return;
                }

                rowToEdit["Name"] = tbName.Text;
                rowToEdit["Age"] = tbAge.Text;
                rowToEdit["Gender"] = rbMale.Checked ? "Male" : "Female";
                rowToEdit["Hometown"] = cbHometown.SelectedItem.ToString();
                rowToEdit["CurrentResidence"] = cbCurrentResidence.SelectedItem.ToString();
                rowToEdit["Department"] = cbDepartment.SelectedItem.ToString();
                rowToEdit["CitizenID"] = tbCitizenID.Text;
                rowToEdit["Class"] = tbClass.Text;

                // Kiểm tra ảnh mới
                byte[] newImage = pbStudentImage.Image != null ? ImageToByteArray(pbStudentImage.Image) : null;
                byte[] oldImage = rowToEdit["Image"] as byte[];

                // Nếu có ảnh mới, cập nhật ảnh
                if (newImage != null && !newImage.SequenceEqual(oldImage))
                {
                    rowToEdit["Image"] = newImage;
                }
                else
                {
                    // Nếu không có ảnh mới, giữ nguyên ảnh cũ
                    rowToEdit["Image"] = oldImage;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE Customers SET Name = @Name, Age = @Age, Gender = @Gender, Hometown = @Hometown, CurrentResidence = @CurrentResidence, Department = @Department, CitizenID = @CitizenID, Class = @Class, Image = @Image WHERE Masv = @OldMasv", conn);
                    cmd.Parameters.AddWithValue("@Name", tbName.Text);
                    cmd.Parameters.AddWithValue("@Age", tbAge.Text);
                    cmd.Parameters.AddWithValue("@Gender", rbMale.Checked ? "Male" : "Female");
                    cmd.Parameters.AddWithValue("@Hometown", cbHometown.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@CurrentResidence", cbCurrentResidence.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@Department", cbDepartment.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@CitizenID", tbCitizenID.Text);
                    cmd.Parameters.AddWithValue("@Class", tbClass.Text);
                    cmd.Parameters.AddWithValue("@Image", rowToEdit["Image"] != DBNull.Value ? (object)rowToEdit["Image"] : DBNull.Value);
                    cmd.Parameters.AddWithValue("@OldMasv", oldMasv);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Student information has been successfully updated.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                //tbId.Enabled = false;

                dgvCustomer.DataSource = dt;
                ClearInputFields();
            }
        }





        private void btNew_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                if (IdExists(tbId.Text))
                {
                    MessageBox.Show("ID already exists. Please enter a different ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DataRow newRow = dt.NewRow();
                newRow["Masv"] = tbId.Text;
                newRow["Name"] = tbName.Text;
                newRow["Age"] = tbAge.Text;
                newRow["Gender"] = rbMale.Checked ? "Male" : "Female";
                newRow["Hometown"] = cbHometown.SelectedItem.ToString();
                newRow["CurrentResidence"] = cbCurrentResidence.SelectedItem.ToString();
                newRow["Department"] = cbDepartment.SelectedItem.ToString();
                newRow["CitizenID"] = tbCitizenID.Text;
                newRow["Class"] = tbClass.Text;

                // Kiểm tra xem người dùng có chọn ảnh hay không
                if (pbStudentImage.Image != null)
                {
                    newRow["Image"] = ImageToByteArray(pbStudentImage.Image); // Chuyển đổi ảnh thành mảng byte
                }
                else
                {
                    newRow["Image"] = DBNull.Value; // Nếu không có ảnh, lưu giá trị DBNull
                }

                dt.Rows.Add(newRow);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Customers (Masv, Name, Age, Gender, Hometown, CurrentResidence, Department, CitizenID, Class, Image) VALUES (@Masv, @Name, @Age, @Gender, @Hometown, @CurrentResidence, @Department, @CitizenID, @Class, @Image)", conn);
                    cmd.Parameters.AddWithValue("@Masv", tbId.Text);
                    cmd.Parameters.AddWithValue("@Name", tbName.Text);
                    cmd.Parameters.AddWithValue("@Age", tbAge.Text);
                    cmd.Parameters.AddWithValue("@Gender", rbMale.Checked ? "Male" : "Female");
                    cmd.Parameters.AddWithValue("@Hometown", cbHometown.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@CurrentResidence", cbCurrentResidence.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@Department", cbDepartment.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@CitizenID", tbCitizenID.Text);
                    cmd.Parameters.AddWithValue("@Class", tbClass.Text);

                    // Thêm ảnh vào cơ sở dữ liệu
                    if (pbStudentImage.Image != null)
                    {
                        cmd.Parameters.AddWithValue("@Image", ImageToByteArray(pbStudentImage.Image));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Image", DBNull.Value); // Nếu không có ảnh, lưu giá trị DBNull
                    }

                    cmd.ExecuteNonQuery();
                }

                dgvCustomer.DataSource = dt;
                ClearInputFields();
            }
        }




        private void btDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomer.CurrentCell != null)
            {
                int idx = dgvCustomer.CurrentCell.RowIndex;

                if (idx < 0 || idx >= dt.Rows.Count) return; // Ensure the row index is valid

                // Get Masv of the selected row
                string masvToDelete = dt.Rows[idx]["Masv"].ToString();

                // Delete from DataTable
                dt.Rows.RemoveAt(idx);

                // Delete from database
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE Masv = @Masv", conn);
                    cmd.Parameters.AddWithValue("@Masv", masvToDelete);
                    cmd.ExecuteNonQuery();
                }

                dgvCustomer.DataSource = dt; // Refresh DataGridView
                ClearInputFields(); // Clear input fields after deleting
            }
        }

       

        private void dgvCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                //tbId.Text = dgvCustomer.Rows[e.RowIndex].Cells["Masv"].Value?.ToString();

                // Gán giá trị Masv vào oldMasv và tbId
                //oldMasv = dgvCustomer.Rows[e.RowIndex].Cells["Masv"].Value?.ToString();
                //tbId.Text = oldMasv;

                // Đặt các giá trị cho các trường khác
                tbName.Text = dgvCustomer.Rows[e.RowIndex].Cells["Name"].Value?.ToString();
                tbClass.Text = dgvCustomer.Rows[e.RowIndex].Cells["Class"].Value?.ToString();
                tbAge.Text = dgvCustomer.Rows[e.RowIndex].Cells["Age"].Value?.ToString();
                tbCitizenID.Text = dgvCustomer.Rows[e.RowIndex].Cells["CitizenID"].Value?.ToString();

                // Xử lý giới tính
                string gender = dgvCustomer.Rows[e.RowIndex].Cells["Gender"].Value?.ToString();
                if (gender == "Male")
                {
                    rbMale.Checked = true;
                    rbFemale.Checked = false;
                }
                else
                {
                    rbMale.Checked = false;
                    rbFemale.Checked = true;
                }

                // Đặt giá trị cho ComboBox Hometown, Current Residence, và Department
                cbHometown.SelectedItem = dgvCustomer.Rows[e.RowIndex].Cells["Hometown"].Value?.ToString();
                cbCurrentResidence.SelectedItem = dgvCustomer.Rows[e.RowIndex].Cells["CurrentResidence"].Value?.ToString();
                cbDepartment.SelectedItem = dgvCustomer.Rows[e.RowIndex].Cells["Department"].Value?.ToString();

                // Kiểm tra và hiển thị ảnh
                //byte[] imageBytes = dgvCustomer.Rows[e.RowIndex].Cells["Image"].Value as byte[];
                //if (imageBytes != null)
                //{
                //    using (MemoryStream ms = new MemoryStream(imageBytes))
                //    {
                //        pbStudentImage.Image = Image.FromStream(ms);
                //    }
                //}
                //else
                //{
                //    pbStudentImage.Image = null; // Nếu không có ảnh, đặt PictureBox là null
                //}

                // Vô hiệu hóa việc sửa ID
                //tbId.Enabled = false;
            }
        }



        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra nếu hàng được chọn hợp lệ
            {
                // Làm sạch các trường nhập liệu trước khi điền dữ liệu mới
                ClearInputFields();

                DataRowView selectedRow = (DataRowView)dgvCustomer.Rows[e.RowIndex].DataBoundItem;

                // Điền thông tin vào các trường nhập liệu
                tbId.Text = selectedRow["Masv"].ToString();
                tbName.Text = selectedRow["Name"].ToString();
                tbAge.Text = selectedRow["Age"].ToString();
                rbMale.Checked = selectedRow["Gender"].ToString() == "Male";
                rbFemale.Checked = selectedRow["Gender"].ToString() == "Female";
                cbHometown.SelectedItem = selectedRow["Hometown"].ToString();
                cbCurrentResidence.SelectedItem = selectedRow["CurrentResidence"].ToString();
                cbDepartment.SelectedItem = selectedRow["Department"].ToString();
                tbCitizenID.Text = selectedRow["CitizenID"].ToString();
                tbClass.Text = selectedRow["Class"].ToString();

                // Cập nhật hình ảnh
                if (selectedRow["Image"] != DBNull.Value)
                {
                    byte[] imageBytes = (byte[])selectedRow["Image"];
                    pbStudentImage.Image = ByteArrayToImage(imageBytes);
                }
                else
                {
                    pbStudentImage.Image = null; // Xóa hình ảnh nếu không có
                }
            }
        }

        private void ClearInputFields()
        {
            tbId.Clear();
            tbName.Clear();
            tbAge.Clear();
            rbMale.Checked = true; // Default gender is Male
            cbHometown.SelectedIndex = -1; // Reset combobox
            cbCurrentResidence.SelectedIndex = -1; // Reset combobox
            cbDepartment.SelectedIndex = -1; // Reset combobox
            tbCitizenID.Clear();
            tbClass.Clear();

            // Reset the PictureBox to clear the image
            pbStudentImage.Image = null; // Assuming pbStudentImage is your PictureBox
        }

        private void dgvCustomer_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dt == null || dt.Rows.Count == 0 || e.RowIndex < 0 || e.RowIndex >= dt.Rows.Count)
            {
                ClearInputFields(); // Xóa các trường nếu không có dữ liệu
                return;
            }

            // Chỉ lấy thông tin nếu hàng không bị xóa
            if (dt.Rows[e.RowIndex].RowState != DataRowState.Deleted)
            {
                PopulateFieldsFromRow(e.RowIndex);
            }
        }


        







        private void PopulateFieldsFromRow(int idx)
        {
            // Kiểm tra chỉ số hàng
            if (idx < 0 || idx >= dt.Rows.Count)
            {
                MessageBox.Show("Invalid row index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Cập nhật các trường thông tin từ DataTable
            tbId.Text = Convert.ToString(dt.Rows[idx]["Masv"]);
            tbName.Text = Convert.ToString(dt.Rows[idx]["Name"]);
            tbAge.Text = Convert.ToString(dt.Rows[idx]["Age"]);

            // Cập nhật giới tính
            string gender = Convert.ToString(dt.Rows[idx]["Gender"]);
            rbMale.Checked = (gender == "Male");
            rbFemale.Checked = (gender == "Female");

            // Cập nhật quê hương
            string hometown = Convert.ToString(dt.Rows[idx]["Hometown"]);
            cbHometown.SelectedItem = cbHometown.Items.Contains(hometown) ? hometown : null;

            // Cập nhật nơi cư trú hiện tại
            string currentResidence = Convert.ToString(dt.Rows[idx]["CurrentResidence"]);
            cbCurrentResidence.SelectedItem = cbCurrentResidence.Items.Contains(currentResidence) ? currentResidence : null;

            // Cập nhật phòng ban
            string department = Convert.ToString(dt.Rows[idx]["Department"]);
            cbDepartment.SelectedItem = cbDepartment.Items.Contains(department) ? department : null;

            // Cập nhật thông tin khác
            tbCitizenID.Text = Convert.ToString(dt.Rows[idx]["CitizenID"]);
            tbClass.Text = Convert.ToString(dt.Rows[idx]["Class"]);

            //// Cập nhật hình ảnh
            //if (dt.Rows[idx]["Image"] != DBNull.Value)
            //{
            //    byte[] imageBytes = (byte[])dt.Rows[idx]["Image"];
            //    pbStudentImage.Image = ByteArrayToImage(imageBytes);
            //}
            //else
            //{
            //    pbStudentImage.Image = null; // Clear the PictureBox if no image
            //}
        }

        // Method to convert byte array to Image
        private Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }




        



        private void btBack_Click(object sender, EventArgs e)
        {
            LoadData(); // Tải lại dữ liệu gốc
        }


        private void btExit_Click(object sender, EventArgs e)
        {
            this.Close(); // Close current form
        }




        private void QLSV_Load(object sender, EventArgs e)
        {
            // Đặt DataGridView là chỉ đọc
            dgvCustomer.ReadOnly = true;

            // Hoặc thiết lập từng cột là chỉ đọc
            dgvCustomer.Columns["Masv"].ReadOnly = true;
            dgvCustomer.Columns["Name"].ReadOnly = true;
            dgvCustomer.Columns["Age"].ReadOnly = true;
            dgvCustomer.Columns["Gender"].ReadOnly = true;
            dgvCustomer.Columns["Hometown"].ReadOnly = true;
            dgvCustomer.Columns["CurrentResidence"].ReadOnly = true;
            dgvCustomer.Columns["Department"].ReadOnly = true;
            dgvCustomer.Columns["CitizenID"].ReadOnly = true;
            dgvCustomer.Columns["Class"].ReadOnly = true;
            dgvCustomer.Columns["Image"].ReadOnly = true;
            LoadData();
        }


        private void tbClass_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void cbHometown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbStudentImage.Image = Image.FromFile(openFileDialog.FileName); // Hiển thị ảnh trong PictureBox
            }
        }

        private void btnReInitialize_Click(object sender, EventArgs e)
        {
            // Clear or reset the input fields
            tbId.Text = "";
            tbName.Text = "";                  // Name
            tbAge.Text = "";                   // Age
            rbMale.Checked = true;             // Default Gender to "Male"
            rbFemale.Checked = false;          // Ensure Female is unchecked

            // Reset ComboBoxes
            cbHometown.SelectedIndex = -1;     // Hometown (no selection)
            cbCurrentResidence.SelectedIndex = -1; // Current Residence (no selection)
            cbDepartment.SelectedIndex = -1;   // Department (no selection)

            // Clear other text fields
            tbCitizenID.Text = "";             // Citizen ID
            tbClass.Text = "";                 // Class

            // Clear the image in PictureBox or set a default
            pbStudentImage.Image = null;      // Clear the image
                                               // Or load a default image if required
                                               // pictureBoxImage.Image = Image.FromFile("path/to/default/image.png");

            // Additional reset for any other relevant controls
        }

    }
}
