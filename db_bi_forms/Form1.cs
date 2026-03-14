using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace db_bi_forms
{
    public partial class Form1 : Form
    {
        private string connectionString = "server=localhost;database=db_bi;user=root;password=qwerty1234"; // an example

        public Form1()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Обратитесь к администратору на Вашем предприятии или в нашу службу поддержки: служба-поддержки@почта.ру");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            string password = textBox2.Text;

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                int? organizationId = GetOrganizationIdIfCredentialsValid(login, password);
                if (organizationId != null)
                {
                    Form2 form2 = new Form2(organizationId.Value);
                    form2.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя и пароль для входа в аккаунт");
            }
        }

        private int? GetOrganizationIdIfCredentialsValid(string email, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT organization_id FROM User WHERE email = @Email AND password_hash = SHA2(@Password, 256)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
