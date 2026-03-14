using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace db_bi_forms
{
    public partial class Form2 : Form
    {
        private int organizationId;
        private string connectionString = "server=localhost;database=db_bi;user=root;password=qwerty1234";

        // Хранение DataGridView для каждой вкладки в полях для доступа из методов
        private DataGridView dgvReports;
        private DataGridView dgvDataSources;
        private DataGridView dgvUsers;
        private DataGridView dgvOrganization;
        private DataGridView dgvVisualizations;

        public Form2(int organizationId)
        {
            InitializeComponent();
            this.organizationId = organizationId;
            this.Load += new EventHandler(Form2_Load);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadTabPage1Data();
            LoadTabPage2Data();
            LoadTabPage3Data();
            LoadTabPage4Data();
            LoadTabPage5Data();
        }

        private void LoadTabPage1Data()
        {
            tabPage1.Text = "Отчеты";
            string query = "SELECT report_id, title, description, creation_date, is_public FROM Report WHERE organization_id = @OrgId";
            DataTable dt = GetData(query);

            dgvReports = new DataGridView { Dock = DockStyle.Fill, ReadOnly = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvReports.DataSource = dt;

            tabPage1.Controls.Clear();

            FlowLayoutPanel panel = CreateCrudPanel(
                onAdd: AddReport,
                onEdit: () => EditReport(dgvReports),
                onDelete: () => DeleteReport(dgvReports)
            );

            tabPage1.Controls.Add(panel);
            tabPage1.Controls.Add(dgvReports);
            dgvReports.BringToFront();
        }

        private void LoadTabPage2Data()
        {
            tabPage2.Text = "Источники данных";
            string query = "SELECT source_id, type, connection_string, last_sync, sync_frequency FROM DataSource WHERE organization_id = @OrgId";
            DataTable dt = GetData(query);

            dgvDataSources = new DataGridView { Dock = DockStyle.Fill, ReadOnly = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvDataSources.DataSource = dt;

            tabPage2.Controls.Clear();

            FlowLayoutPanel panel = CreateCrudPanel(
                onAdd: AddDataSource,
                onEdit: () => EditDataSource(dgvDataSources),
                onDelete: () => DeleteDataSource(dgvDataSources)
            );

            tabPage2.Controls.Add(panel);
            tabPage2.Controls.Add(dgvDataSources);
            dgvDataSources.BringToFront();
        }

        private void LoadTabPage3Data()
        {
            tabPage3.Text = "Пользователи";
            string query = "SELECT user_id, email, role, last_login FROM User WHERE organization_id = @OrgId";
            DataTable dt = GetData(query);

            dgvUsers = new DataGridView { Dock = DockStyle.Fill, ReadOnly = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvUsers.DataSource = dt;

            tabPage3.Controls.Clear();

            FlowLayoutPanel panel = CreateCrudPanel(
                onAdd: AddUser,
                onEdit: () => EditUser(dgvUsers),
                onDelete: () => DeleteUser(dgvUsers)
            );

            tabPage3.Controls.Add(panel);
            tabPage3.Controls.Add(dgvUsers);
            dgvUsers.BringToFront();
        }

        private void LoadTabPage4Data()
        {
            tabPage4.Text = "Организация";
            string query = "SELECT organization_id, name, industry, registration_date, contact_email, subscription_plan FROM Organization WHERE organization_id = @OrgId";
            DataTable dt = GetData(query);

            dgvOrganization = new DataGridView { Dock = DockStyle.Fill, ReadOnly = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvOrganization.DataSource = dt;

            tabPage4.Controls.Clear();

            FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            Button btnEdit = new Button { Text = "Редактировать" };
            btnEdit.Width = 100;
            btnEdit.Click += (s, e) => EditOrganization(dgvOrganization);
            panel.Controls.Add(btnEdit);

            tabPage4.Controls.Add(panel);
            tabPage4.Controls.Add(dgvOrganization);
            dgvOrganization.BringToFront();
        }

        private void LoadTabPage5Data()
        {
            tabPage5.Text = "Визуализации";
            string query = @"
                SELECT v.visualization_id, v.type, v.data_query, v.settings
                FROM Visualization v
                JOIN Report r ON v.report_id = r.report_id
                WHERE r.organization_id = @OrgId";
            DataTable dt = GetData(query);

            dgvVisualizations = new DataGridView { Dock = DockStyle.Fill, ReadOnly = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvVisualizations.DataSource = dt;

            tabPage5.Controls.Clear();

            FlowLayoutPanel panel = CreateCrudPanel(
                onAdd: AddVisualization,
                onEdit: () => EditVisualization(dgvVisualizations),
                onDelete: () => DeleteVisualization(dgvVisualizations)
            );

            tabPage5.Controls.Add(panel);
            tabPage5.Controls.Add(dgvVisualizations);
            dgvVisualizations.BringToFront();
        }

        private FlowLayoutPanel CreateCrudPanel(Action onAdd, Action onEdit, Action onDelete)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            Button btnAdd = new Button { Text = "Добавить" };
            Button btnEdit = new Button { Text = "Редактировать" };
            btnEdit.Width = 100;
            Button btnDelete = new Button { Text = "Удалить" };

            btnAdd.Click += (s, e) => onAdd();
            btnEdit.Click += (s, e) => onEdit();
            btnDelete.Click += (s, e) => onDelete();

            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(btnDelete);

            return panel;
        }

        private DataTable GetData(string query)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@OrgId", organizationId);
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        private void AddReport()
        {
            string title = Prompt.ShowDialog("Введите заголовок отчета:", "Добавить отчет");
            if (string.IsNullOrWhiteSpace(title)) return;

            string description = Prompt.ShowDialog("Введите описание:", "Добавить отчет");
            if (description == null) return;

            bool isPublic = MessageBox.Show("Сделать отчет публичным?", "Публичность", MessageBoxButtons.YesNo) == DialogResult.Yes;

            string query = "INSERT INTO Report (title, description, creation_date, is_public, organization_id) VALUES (@Title, @Description, @CreationDate, @IsPublic, @OrgId)";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@CreationDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@IsPublic", isPublic);
                    cmd.Parameters.AddWithValue("@OrgId", organizationId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage1Data();
        }

        private void DeleteReport(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }
            int reportId = Convert.ToInt32(dgv.SelectedRows[0].Cells["report_id"].Value);
            if (MessageBox.Show("Удалить выбранный отчет?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string query = "DELETE FROM Report WHERE report_id = @ReportId";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReportId", reportId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadTabPage1Data();
            }
        }

        private void EditReport(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            DataGridViewRow row = dgv.SelectedRows[0];

            string currentTitle = row.Cells["title"].Value?.ToString() ?? "";
            string currentDescription = row.Cells["description"].Value?.ToString() ?? "";
            bool currentIsPublic = Convert.ToBoolean(row.Cells["is_public"].Value);

            string newTitle = Prompt.ShowDialog("Измените заголовок отчета:", "Редактировать отчет", currentTitle);
            if (string.IsNullOrWhiteSpace(newTitle)) return;

            string newDescription = Prompt.ShowDialog("Измените описание:", "Редактировать отчет", currentDescription);
            if (newDescription == null) return;

            bool newIsPublic = MessageBox.Show("Сделать отчет публичным?", "Публичность", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                currentIsPublic ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2) == DialogResult.Yes;

            int reportId = Convert.ToInt32(row.Cells["report_id"].Value);
            string query = "UPDATE Report SET title = @Title, description = @Description, is_public = @IsPublic WHERE report_id = @ReportId";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", newTitle);
                    cmd.Parameters.AddWithValue("@Description", newDescription);
                    cmd.Parameters.AddWithValue("@IsPublic", newIsPublic);
                    cmd.Parameters.AddWithValue("@ReportId", reportId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage1Data();
        }

        private void AddDataSource()
        {
            string type = Prompt.ShowDialog("Введите тип источника данных:", "Добавить источник данных");
            if (string.IsNullOrWhiteSpace(type)) return;

            string connectionString = Prompt.ShowDialog("Введите строку подключения:", "Добавить источник данных");
            if (connectionString == null) return;

            string query = "INSERT INTO DataSource (type, connection_string, last_sync, sync_frequency, organization_id) VALUES (@Type, @ConnectionString, @LastSync, @SyncFrequency, @OrgId)";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@ConnectionString", connectionString);
                    cmd.Parameters.AddWithValue("@LastSync", DateTime.Now);
                    cmd.Parameters.AddWithValue("@SyncFrequency", 0); // Установите значение по умолчанию
                    cmd.Parameters.AddWithValue("@OrgId", organizationId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage2Data();
        }

        private void DeleteDataSource(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }
            int sourceId = Convert.ToInt32(dgv.SelectedRows[0].Cells["source_id"].Value);
            if (MessageBox.Show("Удалить выбранный источник данных?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string query = "DELETE FROM DataSource WHERE source_id = @SourceId";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SourceId", sourceId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadTabPage2Data();
            }
        }

        private void EditDataSource(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            DataGridViewRow row = dgv.SelectedRows[0];

            string currentType = row.Cells["type"].Value?.ToString() ?? "";
            string currentConnectionString = row.Cells["connection_string"].Value?.ToString() ?? "";

            string newType = Prompt.ShowDialog("Измените тип источника данных:", "Редактировать источник данных", currentType);
            if (string.IsNullOrWhiteSpace(newType)) return;

            string newConnectionString = Prompt.ShowDialog("Измените строку подключения:", "Редактировать источник данных", currentConnectionString);
            if (newConnectionString == null) return;

            int sourceId = Convert.ToInt32(row.Cells["source_id"].Value);
            string query = "UPDATE DataSource SET type = @Type, connection_string = @ConnectionString WHERE source_id = @SourceId";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Type", newType);
                    cmd.Parameters.AddWithValue("@ConnectionString", newConnectionString);
                    cmd.Parameters.AddWithValue("@SourceId", sourceId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage2Data();
        }

        private void AddUser()
        {
            string email = Prompt.ShowDialog("Введите email пользователя:", "Добавить пользователя");
            if (string.IsNullOrWhiteSpace(email)) return;

            string role = Prompt.ShowDialog("Введите роль пользователя:", "Добавить пользователя");
            if (string.IsNullOrWhiteSpace(role)) return;

            string query = "INSERT INTO User (email, role, last_login, organization_id) VALUES (@Email, @Role, @LastLogin, @OrgId)";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@LastLogin", DateTime.Now);
                    cmd.Parameters.AddWithValue("@OrgId", organizationId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage3Data();
        }

        private void DeleteUser(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }
            int userId = Convert.ToInt32(dgv.SelectedRows[0].Cells["user_id"].Value);
            if (MessageBox.Show("Удалить выбранного пользователя?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string query = "DELETE FROM User WHERE user_id = @User Id";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@User Id", userId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadTabPage3Data();
            }
        }

        private void EditUser(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            DataGridViewRow row = dgv.SelectedRows[0];

            string currentEmail = row.Cells["email"].Value?.ToString() ?? "";
            string currentRole = row.Cells["role"].Value?.ToString() ?? "";

            string newEmail = Prompt.ShowDialog("Измените email пользователя:", "Редактировать пользователя", currentEmail);
            if (string.IsNullOrWhiteSpace(newEmail)) return;

            string newRole = Prompt.ShowDialog("Измените роль пользователя:", "Редактировать пользователя", currentRole);
            if (newRole == null) return;

            int userId = Convert.ToInt32(row.Cells["user_id"].Value);
            string query = "UPDATE User SET email = @Email, role = @Role WHERE user_id = @User Id";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", newEmail);
                    cmd.Parameters.AddWithValue("@Role", newRole);
                    cmd.Parameters.AddWithValue("@User Id", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage3Data();
        }

        private void EditOrganization(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            DataGridViewRow row = dgv.SelectedRows[0];

            string currentName = row.Cells["name"].Value?.ToString() ?? "";
            string currentIndustry = row.Cells["industry"].Value?.ToString() ?? "";
            string currentContactEmail = row.Cells["contact_email"].Value?.ToString() ?? "";
            string currentSubscriptionPlan = row.Cells["subscription_plan"].Value?.ToString() ?? "";

            string newName = Prompt.ShowDialog("Измените название организации:", "Редактировать организацию", currentName);
            if (string.IsNullOrWhiteSpace(newName)) return;

            string newIndustry = Prompt.ShowDialog("Измените отрасль:", "Редактировать организацию", currentIndustry);
            if (newIndustry == null) return;

            string newContactEmail = Prompt.ShowDialog("Измените контактный email:", "Редактировать организацию", currentContactEmail);
            if (newContactEmail == null) return;

            string newSubscriptionPlan = Prompt.ShowDialog("Измените план подписки:", "Редактировать организацию", currentSubscriptionPlan);
            if (newSubscriptionPlan == null) return;

            int organizationId = Convert.ToInt32(row.Cells["organization_id"].Value);
            string query = "UPDATE Organization SET name = @Name, industry = @Industry, contact_email = @ContactEmail, subscription_plan = @SubscriptionPlan WHERE organization_id = @OrgId";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", newName);
                    cmd.Parameters.AddWithValue("@Industry", newIndustry);
                    cmd.Parameters.AddWithValue("@ContactEmail", newContactEmail);
                    cmd.Parameters.AddWithValue("@SubscriptionPlan", newSubscriptionPlan);
                    cmd.Parameters.AddWithValue("@OrgId", organizationId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage4Data();
        }

        private void AddVisualization()
        {
            string type = Prompt.ShowDialog("Введите тип визуализации:", "Добавить визуализацию");
            if (string.IsNullOrWhiteSpace(type)) return;

            string dataQuery = Prompt.ShowDialog("Введите запрос данных:", "Добавить визуализацию");
            if (dataQuery == null) return;

            string settings = Prompt.ShowDialog("Введите настройки визуализации:", "Добавить визуализацию");
            if (settings == null) return;

            string query = "INSERT INTO Visualization (type, data_query, settings, report_id) VALUES (@Type, @DataQuery, @Settings, @ReportId)";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@DataQuery", dataQuery);
                    cmd.Parameters.AddWithValue("@Settings", settings);
                    cmd.Parameters.AddWithValue("@ReportId", 1); // Установите значение по умолчанию или выберите нужный report_id
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage5Data();
        }

        private void DeleteVisualization(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }
            int visualizationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["visualization_id"].Value);
            if (MessageBox.Show("Удалить выбранную визуализацию?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string query = "DELETE FROM Visualization WHERE visualization_id = @VisualizationId";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VisualizationId", visualizationId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadTabPage5Data();
            }
        }

        private void EditVisualization(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            DataGridViewRow row = dgv.SelectedRows[0];

            string currentType = row.Cells["type"].Value?.ToString() ?? "";
            string currentDataQuery = row.Cells["data_query"].Value?.ToString() ?? "";
            string currentSettings = row.Cells["settings"].Value?.ToString() ?? "";

            string newType = Prompt.ShowDialog("Измените тип визуализации:", "Редактировать визуализацию", currentType);
            if (string.IsNullOrWhiteSpace(newType)) return;

            string newDataQuery = Prompt.ShowDialog("Измените запрос данных:", "Редактировать визуализацию", currentDataQuery);
            if (newDataQuery == null) return;

            string newSettings = Prompt.ShowDialog("Измените настройки визуализации:", "Редактировать визуализацию", currentSettings);
            if (newSettings == null) return;

            int visualizationId = Convert.ToInt32(row.Cells["visualization_id"].Value);
            string query = "UPDATE Visualization SET type = @Type, data_query = @DataQuery, settings = @Settings WHERE visualization_id = @VisualizationId";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Type", newType);
                    cmd.Parameters.AddWithValue("@DataQuery", newDataQuery);
                    cmd.Parameters.AddWithValue("@Settings", newSettings);
                    cmd.Parameters.AddWithValue("@VisualizationId", visualizationId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            LoadTabPage5Data();
        }

    }

    // Простой класс для диалогового окна ввода текста (InputBox аналог)
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent
            };
            Label textLabel = new Label() { Left = 10, Top = 10, Text = text, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 10, Top = 35, Width = 360, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 220, Width = 75, Top = 70, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Отмена", Left = 300, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}

