using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;
using Microsoft.Data.SqlClient;

namespace EmpresaApp.Forms
{
    public class ServidorForm : Form
    {
        public bool ServidorConfigurado { get; private set; }

        private TabControl tabControl = null!;
        private TabPage tabCrear = null!;
        private TabPage tabUnirse = null!;

        public ServidorForm()
        {
            Text = "Configurar servidor";
            Size = new Size(520, 580);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ModernTheme.ApplyToForm(this);

            tabControl = new TabControl { Dock = DockStyle.Fill };
            ModernTheme.StyleTabControl(tabControl);
            tabCrear = new TabPage("Crear");
            tabUnirse = new TabPage("Unirse");
            BuildTabCrear();
            BuildTabUnirse();
            tabControl.TabPages.Add(tabCrear);
            tabControl.TabPages.Add(tabUnirse);
            Controls.Add(tabControl);
        }

        private static Panel CrearScrollPanel(TabPage tab)
        {
            tab.BackColor = ModernTheme.Colors.Background;
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(16),
                BackColor = ModernTheme.Colors.Background
            };
            tab.Controls.Add(scroll);
            return scroll;
        }

        private void BuildTabCrear()
        {
            var scroll = CrearScrollPanel(tabCrear);
            var form = UiLayout.CreateFormStack();
            form.Width = 440;
            form.Dock = DockStyle.Top;
            scroll.Controls.Add(form);

            var txtServidor = AddField(form, "Servidor SQL", ".");
            var txtDb = AddField(form, "Base de datos", "EmpresaApp");

            UiLayout.StackLabel(form, "Autenticacion");
            var cboAuth = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(cboAuth);
            cboAuth.Items.AddRange(new object[] { "Windows", "SQL Server" });
            cboAuth.SelectedIndex = 0;
            UiLayout.StackRow(form, cboAuth, UiLayout.ControlHeight);

            var pnlSql = new Panel { Dock = DockStyle.Fill, Height = 96, Visible = false };
            var txtUser = AddFieldToPanel(pnlSql, "Usuario SQL");
            var txtPass = AddFieldToPanel(pnlSql, "Contrasena SQL", password: true, top: 48);
            UiLayout.StackRow(form, pnlSql, 96);
            cboAuth.SelectedIndexChanged += (_, _) => { pnlSql.Visible = cboAuth.SelectedIndex == 1; };

            var txtAdminUser = AddField(form, "Usuario admin", "admin");
            var txtAdminPass = AddField(form, "Contrasena admin", "admin123", password: true);

            var lblStatus = new Label
            {
                ForeColor = ModernTheme.Colors.Danger,
                Font = ModernTheme.FontCaption,
                AutoSize = false,
                Height = 36,
                Visible = false,
                Dock = DockStyle.Fill
            };
            UiLayout.StackRow(form, lblStatus, 36);

            var btnCrear = ModernTheme.CreateWideButton("Crear servidor", 440, ModernTheme.ButtonVariant.Primary);
            btnCrear.Click += (_, _) =>
                CrearServidor(txtServidor, txtDb, cboAuth, txtUser, txtPass, txtAdminUser, txtAdminPass, lblStatus, btnCrear);
            UiLayout.StackRow(form, btnCrear, UiLayout.ButtonHeight + 8);
        }

        private void BuildTabUnirse()
        {
            var scroll = CrearScrollPanel(tabUnirse);
            var form = UiLayout.CreateFormStack();
            form.Width = 440;
            form.Dock = DockStyle.Top;
            scroll.Controls.Add(form);

            var txtServidor = AddField(form, "Servidor SQL", ".");
            var txtDb = AddField(form, "Base de datos", "EmpresaApp");

            UiLayout.StackLabel(form, "Autenticacion");
            var cboAuth = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(cboAuth);
            cboAuth.Items.AddRange(new object[] { "Windows", "SQL Server" });
            cboAuth.SelectedIndex = 0;
            UiLayout.StackRow(form, cboAuth, UiLayout.ControlHeight);

            var pnlSql = new Panel { Dock = DockStyle.Fill, Height = 96, Visible = false };
            var txtUser = AddFieldToPanel(pnlSql, "Usuario SQL");
            var txtPass = AddFieldToPanel(pnlSql, "Contrasena SQL", password: true, top: 48);
            UiLayout.StackRow(form, pnlSql, 96);
            cboAuth.SelectedIndexChanged += (_, _) => { pnlSql.Visible = cboAuth.SelectedIndex == 1; };

            UiLayout.StackLabel(form, "Cadena de conexion");
            var txtFull = MinimalUi.CreateTextBox();
            txtFull.ReadOnly = true;
            txtFull.Dock = DockStyle.Fill;
            UiLayout.StackRow(form, txtFull, UiLayout.ControlHeight);

            void Actualizar()
            {
                string a = cboAuth.SelectedIndex == 0
                    ? "Trusted_Connection=True;TrustServerCertificate=True;"
                    : $"User Id={txtUser.Text};Password={txtPass.Text};TrustServerCertificate=True;";
                txtFull.Text = $"Server={txtServidor.Text};Database={txtDb.Text};{a}";
            }
            txtServidor.TextChanged += (_, _) => Actualizar();
            txtDb.TextChanged += (_, _) => Actualizar();
            txtUser.TextChanged += (_, _) => Actualizar();
            txtPass.TextChanged += (_, _) => Actualizar();
            cboAuth.SelectedIndexChanged += (_, _) => Actualizar();
            Actualizar();

            var lblStatus = new Label
            {
                ForeColor = ModernTheme.Colors.Danger,
                Font = ModernTheme.FontCaption,
                AutoSize = false,
                Height = 36,
                Visible = false,
                Dock = DockStyle.Fill
            };
            UiLayout.StackRow(form, lblStatus, 36);

            var btnUnirse = ModernTheme.CreateWideButton("Conectar", 440, ModernTheme.ButtonVariant.Primary);
            btnUnirse.Click += (_, _) =>
            {
                lblStatus.Visible = false;
                string connStr = txtFull.Text.Trim();
                if (string.IsNullOrWhiteSpace(connStr))
                {
                    Alerta(lblStatus, "Complete la cadena de conexion.");
                    return;
                }
                btnUnirse.Enabled = false;
                try
                {
                    using var conn = new SqlConnection(connStr);
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText =
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Usuarios'";
                    int tablas = (int)cmd.ExecuteScalar()!;
                    if (tablas == 0)
                    {
                        Alerta(lblStatus, "La base no tiene el esquema de EmpresaApp. Use Crear.");
                        return;
                    }
                    DbConfig.ConnectionString = connStr;
                    ServerConfig.GuardarServidor(connStr);
                    ServidorConfigurado = true;
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    Alerta(lblStatus, "Error: " + ex.Message);
                }
                finally
                {
                    btnUnirse.Enabled = true;
                }
            };
            UiLayout.StackRow(form, btnUnirse, UiLayout.ButtonHeight + 8);
        }

        private static TextBox AddField(TableLayoutPanel form, string label, string value = "", bool password = false)
        {
            UiLayout.StackLabel(form, label);
            var tb = new TextBox { Text = value };
            if (password) tb.UseSystemPasswordChar = true;
            var wrap = ModernTheme.CreateInput(tb);
            UiLayout.StackRow(form, wrap, 44);
            return tb;
        }

        private static TextBox AddFieldToPanel(Panel panel, string placeholder, bool password = false, int top = 0)
        {
            var tb = new TextBox { PlaceholderText = placeholder };
            if (password) tb.UseSystemPasswordChar = true;
            var wrap = ModernTheme.CreateInput(tb);
            wrap.Dock = DockStyle.None;
            wrap.Location = new Point(0, top);
            wrap.Width = panel.Width > 0 ? panel.Width : 400;
            wrap.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel.Controls.Add(wrap);
            return tb;
        }

        private void CrearServidor(
            TextBox txtServidor, TextBox txtDb, ComboBox cboAuth,
            TextBox txtUser, TextBox txtPass,
            TextBox txtAdminUser, TextBox txtAdminPass,
            Label lblStatus, Button btnCrear)
        {
            lblStatus.Visible = false;

            if (string.IsNullOrWhiteSpace(txtServidor.Text) ||
                string.IsNullOrWhiteSpace(txtDb.Text) ||
                string.IsNullOrWhiteSpace(txtAdminUser.Text) ||
                string.IsNullOrWhiteSpace(txtAdminPass.Text))
            {
                Alerta(lblStatus, "Complete todos los campos.");
                return;
            }

            string auth = cboAuth.SelectedIndex == 0
                ? "Trusted_Connection=True;TrustServerCertificate=True;"
                : $"User Id={txtUser.Text};Password={txtPass.Text};TrustServerCertificate=True;";

            string connMaster = $"Server={txtServidor.Text};Database=master;{auth}";
            string dbName = txtDb.Text.Trim();
            string connApp = $"Server={txtServidor.Text};Database={dbName};{auth}";

            btnCrear.Enabled = false;
            try
            {
                using (var conn = new SqlConnection(connMaster))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $@"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}')
                        BEGIN
                            CREATE DATABASE [{dbName}]
                        END";
                    cmd.ExecuteNonQuery();
                }

                string sqlScript = ObtenerSqlSetup();

                using (var conn = new SqlConnection(connApp))
                {
                    conn.Open();
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.CommandText =
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Usuarios'";
                    int tablas = (int)checkCmd.ExecuteScalar()!;

                    if (tablas == 0)
                    {
                        foreach (string batch in sqlScript.Split(
                            new[] { "\nGO", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string trimmed = batch.Trim();
                            if (string.IsNullOrEmpty(trimmed)) continue;
                            using var batchCmd = conn.CreateCommand();
                            batchCmd.CommandText = trimmed;
                            batchCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        EnsureAdmin(conn, txtAdminUser.Text.Trim(), txtAdminPass.Text);
                        DbConfig.ConnectionString = connApp;
                        ServerConfig.GuardarServidor(connApp);
                        ServidorConfigurado = true;
                        DialogResult = DialogResult.OK;
                        return;
                    }

                    EnsureAdmin(conn, txtAdminUser.Text.Trim(), txtAdminPass.Text);
                }

                DbConfig.ConnectionString = connApp;
                ServerConfig.GuardarServidor(connApp);
                ServidorConfigurado = true;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Alerta(lblStatus, "Error: " + ex.Message);
            }
            finally
            {
                btnCrear.Enabled = true;
            }
        }

        private static void EnsureAdmin(IDbConnection conn, string login, string password)
        {
            using var cmd = (SqlCommand)((SqlConnection)conn).CreateCommand();
            cmd.CommandText = @"
                IF EXISTS (SELECT 1 FROM Usuarios WHERE Usuario_Login = @login)
                    UPDATE Usuarios SET Password = @pass WHERE Usuario_Login = @login
                ELSE
                    INSERT INTO Usuarios
                        (Id, Nombre, Apellido, Email, Usuario_Login, Password, Departamento, Cargo, Rol)
                    VALUES
                        (NEWID(), 'Admin', 'Sistema', 'admin@empresa.com',
                         @login, @pass, 'Sistemas', 'Administrador General', 0)";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@pass", password);
            cmd.ExecuteNonQuery();
        }

        private static void Alerta(Label lbl, string texto)
        {
            lbl.ForeColor = ModernTheme.Colors.Danger;
            lbl.Text = texto;
            lbl.Visible = true;
        }

        private static string ObtenerSqlSetup()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlSetup.sql");
            return File.Exists(path) ? File.ReadAllText(path) : ServerConfig.SqlSetupScript;
        }
    }
}
