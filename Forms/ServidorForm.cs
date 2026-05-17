// ═══════════════════════════════════════════════════════════════
//  ServidorForm.cs
//  Pantalla de bienvenida que aparece ANTES del login.
//  Permite:
//    • Crear un servidor nuevo (instala el esquema SQL y crea el admin)
//    • Unirse a un servidor existente (valida conexión y lo guarda)
// ═══════════════════════════════════════════════════════════════
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
        public bool ServidorConfigurado { get; private set; } = false;

        private TabControl tabControl = null!;
        private TabPage    tabCrear   = null!;
        private TabPage    tabUnirse  = null!;

        public ServidorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "EmpresaApp — Configurar Servidor";
            this.Size            = new Size(520, 640);
            this.MinimumSize     = new Size(520, 480);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox     = true;
            this.BackColor       = Colores.Fondo;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── Header ────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 110,
                BackColor = Colores.Primario
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlHeader.ClientRectangle,
                    Colores.Primario, Color.FromArgb(37, 99, 235),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
            };

            var lblIcon = new Label
            {
                Text = "🏢", Font = new Font("Segoe UI Emoji", 26),
                ForeColor = Color.White, Location = new Point(20, 18), AutoSize = true
            };
            var lblTitle = new Label
            {
                Text = "EmpresaApp",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(74, 16), AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Configura tu servidor antes de iniciar sesión",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 210, 245),
                Location = new Point(74, 56), AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblSub });

            // ── Tab Control (fills remaining space below header) ──
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f)
            };

            tabCrear  = new TabPage("  ➕  Crear Servidor  ");
            tabUnirse = new TabPage("  🔗  Unirse a Servidor  ");

            BuildTabCrear();
            BuildTabUnirse();

            tabControl.TabPages.Add(tabCrear);
            tabControl.TabPages.Add(tabUnirse);

            // Order matters: Fill first, then Top
            this.Controls.Add(tabControl);
            this.Controls.Add(pnlHeader);
        }

        // ════════════════════════════════════════════════════════
        //  Helper: crea un Panel scrollable que llena el TabPage
        // ════════════════════════════════════════════════════════
        private static Panel CrearScrollPanel(TabPage tab)
        {
            var scroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = Colores.Fondo,
                Padding    = new Padding(0, 0, 0, 12)
            };
            tab.Controls.Add(scroll);
            return scroll;
        }

        // ════════════════════════════════════════════════════════
        //  TAB: CREAR SERVIDOR
        // ════════════════════════════════════════════════════════
        private void BuildTabCrear()
        {
            tabCrear.BackColor = Colores.Fondo;
            tabCrear.Padding   = new Padding(0);

            var scroll = CrearScrollPanel(tabCrear);
            int y = 18;
            const int W = 430;   // ancho de controles
            const int X = 15;    // margen izquierdo

            // Info
            var lblInfo = new Label
            {
                Text = "Crea una nueva base de datos EmpresaApp en tu SQL Server.\n" +
                       "Se crearán todas las tablas y un usuario administrador por defecto.",
                Location = new Point(X, y), Size = new Size(W, 46),
                ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9)
            };
            scroll.Controls.Add(lblInfo);
            y += 54;

            // Servidor SQL
            Lbl(scroll, "Servidor SQL", ref y, X);
            var txtServidor = Txt(scroll, "Ej: .  ó  MI-PC\\SQLEXPRESS", ".", ref y, W, X);

            // Base de datos
            Lbl(scroll, "Nombre de la base de datos", ref y, X);
            var txtDb = Txt(scroll, "EmpresaApp", "EmpresaApp", ref y, W, X);

            // Auth
            Lbl(scroll, "Autenticación", ref y, X);
            var cboAuth = new ComboBox
            {
                Location = new Point(X, y), Size = new Size(W, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10)
            };
            cboAuth.Items.AddRange(new object[]
                { "Windows (Trusted Connection)", "SQL Server (usuario y contraseña)" });
            cboAuth.SelectedIndex = 0;
            scroll.Controls.Add(cboAuth);
            y += 34;

            // Panel SQL auth (visible solo cuando se elige SQL Server)
            var pnlSql = new Panel
            {
                Location = new Point(X, y), Size = new Size(W, 74),
                Visible = false, BackColor = Color.FromArgb(240, 244, 250)
            };
            pnlSql.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(200, 215, 235)), 0, 0, pnlSql.Width - 1, pnlSql.Height - 1);

            var txtUser = Txt2(pnlSql, "Usuario SQL",     3, W - 6, 0);
            var txtPass = Txt2(pnlSql, "Contraseña SQL",  3, W - 6, 38, true);
            scroll.Controls.Add(pnlSql);

            // El pnlSql ocupa espacio solo cuando está visible; usamos un spacer dinámico
            var spacerSql = new Panel
            {
                Location = new Point(X, y), Size = new Size(W, 8),
                Visible = true, BackColor = Color.Transparent
            };
            scroll.Controls.Add(spacerSql);
            y += 12;  // margen mínimo siempre

            cboAuth.SelectedIndexChanged += (s, e) =>
            {
                bool sql = cboAuth.SelectedIndex == 1;
                pnlSql.Visible  = sql;
                spacerSql.Height = sql ? 74 + 8 : 8;
                // Reubicar controles que siguen a pnlSql
                int newY = pnlSql.Top + spacerSql.Height;
                foreach (Control c in scroll.Controls)
                {
                    if (c.Top >= y && c != pnlSql && c != spacerSql)
                        c.Top += sql ? 74 : -74;
                }
                scroll.Invalidate();
            };

            // Admin
            Lbl(scroll, "Usuario admin por defecto", ref y, X);
            var txtAdminUser = Txt(scroll, "Ej: admin", "admin", ref y, W, X);
            Lbl(scroll, "Contraseña admin", ref y, X);
            var txtAdminPass = Txt(scroll, "Ej: admin123", "admin123", ref y, W, X, true);

            // Separador
            var sep = new Panel
            {
                Location = new Point(X, y), Size = new Size(W, 1),
                BackColor = Color.FromArgb(220, 228, 240)
            };
            scroll.Controls.Add(sep);
            y += 10;

            // Status
            var lblStatus = new Label
            {
                Location = new Point(X, y), Size = new Size(W, 38),
                ForeColor = Colores.Acento, Font = new Font("Segoe UI", 9),
                Text = "", Visible = false
            };
            scroll.Controls.Add(lblStatus);
            y += 42;

            // Botón crear
            var btnCrear = new Button
            {
                Text      = "🚀  Crear Servidor",
                Location  = new Point(X, y),
                Size      = new Size(W, 46),
                BackColor = Colores.Acento,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnCrear.FlatAppearance.BorderSize = 0;
            btnCrear.Click += (s, e) =>
                CrearServidor(txtServidor, txtDb, cboAuth, txtUser, txtPass,
                              txtAdminUser, txtAdminPass, lblStatus, btnCrear);
            scroll.Controls.Add(btnCrear);
            y += 54;

            // Dummy control para que AutoScroll sepa el alto total
            var anchor = new Panel
            {
                Location = new Point(0, y), Size = new Size(1, 1),
                BackColor = Color.Transparent
            };
            scroll.Controls.Add(anchor);
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
                Alerta(lblStatus, "⚠ Completa todos los campos obligatorios.");
                return;
            }

            string auth = cboAuth.SelectedIndex == 0
                ? "Trusted_Connection=True;TrustServerCertificate=True;"
                : $"User Id={txtUser.Text};Password={txtPass.Text};TrustServerCertificate=True;";

            string connMaster = $"Server={txtServidor.Text};Database=master;{auth}";
            string dbName     = txtDb.Text.Trim();
            string connApp    = $"Server={txtServidor.Text};Database={dbName};{auth}";

            btnCrear.Enabled = false;
            btnCrear.Text    = "⏳  Creando...";

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
                        Exito(lblStatus, $"✅ Conectado a base de datos existente en '{dbName}'.");
                        btnCrear.Enabled = true;
                        btnCrear.Text    = "🚀  Crear Servidor";
                        this.DialogResult = DialogResult.OK;
                        return;
                    }

                    EnsureAdmin(conn, txtAdminUser.Text.Trim(), txtAdminPass.Text);
                }

                DbConfig.ConnectionString = connApp;
                ServerConfig.GuardarServidor(connApp);
                ServidorConfigurado = true;
                Exito(lblStatus,
                    $"✅ Servidor '{dbName}' creado.\nAdmin: {txtAdminUser.Text} / {txtAdminPass.Text}");

                System.Threading.Tasks.Task.Delay(1800).ContinueWith(_ =>
                    this.Invoke(() => { this.DialogResult = DialogResult.OK; }));
            }
            catch (Exception ex)
            {
                Alerta(lblStatus, $"✗ Error: {ex.Message}");
            }
            finally
            {
                btnCrear.Enabled = true;
                btnCrear.Text    = "🚀  Crear Servidor";
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
            cmd.Parameters.AddWithValue("@pass",  password);
            cmd.ExecuteNonQuery();
        }

        // ════════════════════════════════════════════════════════
        //  TAB: UNIRSE A SERVIDOR
        // ════════════════════════════════════════════════════════
        private void BuildTabUnirse()
        {
            tabUnirse.BackColor = Colores.Fondo;
            tabUnirse.Padding   = new Padding(0);

            var scroll = CrearScrollPanel(tabUnirse);
            int y = 18;
            const int W = 430;
            const int X = 15;

            var lblInfo = new Label
            {
                Text = "Conéctate a un servidor EmpresaApp que ya existe.\n" +
                       "Necesitas la dirección del servidor y las credenciales.",
                Location = new Point(X, y), Size = new Size(W, 46),
                ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9)
            };
            scroll.Controls.Add(lblInfo);
            y += 54;

            Lbl(scroll, "Servidor SQL", ref y, X);
            var txtServidor = Txt(scroll, "Ej: 192.168.1.5  ó  MI-PC\\SQLEXPRESS", ".", ref y, W, X);

            Lbl(scroll, "Base de datos", ref y, X);
            var txtDb = Txt(scroll, "EmpresaApp", "EmpresaApp", ref y, W, X);

            Lbl(scroll, "Autenticación", ref y, X);
            var cboAuth = new ComboBox
            {
                Location = new Point(X, y), Size = new Size(W, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10)
            };
            cboAuth.Items.AddRange(new object[]
                { "Windows (Trusted Connection)", "SQL Server (usuario y contraseña)" });
            cboAuth.SelectedIndex = 0;
            scroll.Controls.Add(cboAuth);
            y += 34;

            var pnlSql = new Panel
            {
                Location = new Point(X, y), Size = new Size(W, 74),
                Visible = false, BackColor = Color.FromArgb(240, 244, 250)
            };
            pnlSql.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(200, 215, 235)), 0, 0, pnlSql.Width - 1, pnlSql.Height - 1);

            var txtUser = Txt2(pnlSql, "Usuario SQL",    3, W - 6, 0);
            var txtPass = Txt2(pnlSql, "Contraseña SQL", 3, W - 6, 38, true);
            scroll.Controls.Add(pnlSql);

            var spacerSql = new Panel
            {
                Location = new Point(X, y), Size = new Size(W, 8),
                BackColor = Color.Transparent
            };
            scroll.Controls.Add(spacerSql);
            y += 12;

            cboAuth.SelectedIndexChanged += (s, e) =>
            {
                bool sql = cboAuth.SelectedIndex == 1;
                pnlSql.Visible   = sql;
                spacerSql.Height = sql ? 74 + 8 : 8;
                foreach (Control c in scroll.Controls)
                    if (c.Top >= y && c != pnlSql && c != spacerSql)
                        c.Top += sql ? 74 : -74;
                scroll.Invalidate();
            };

            // Cadena completa
            Lbl(scroll, "Cadena de conexión completa (editable)", ref y, X);
            var txtFull = new TextBox
            {
                Location = new Point(X, y), Size = new Size(W, 28),
                Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle
            };
            scroll.Controls.Add(txtFull);
            y += 36;

            void Actualizar()
            {
                string a = cboAuth.SelectedIndex == 0
                    ? "Trusted_Connection=True;TrustServerCertificate=True;"
                    : $"User Id={txtUser.Text};Password={txtPass.Text};TrustServerCertificate=True;";
                txtFull.Text = $"Server={txtServidor.Text};Database={txtDb.Text};{a}";
            }
            txtServidor.TextChanged          += (s, e) => Actualizar();
            txtDb.TextChanged                += (s, e) => Actualizar();
            txtUser.TextChanged              += (s, e) => Actualizar();
            txtPass.TextChanged              += (s, e) => Actualizar();
            cboAuth.SelectedIndexChanged     += (s, e) => Actualizar();
            Actualizar();

            var sep = new Panel
            {
                Location = new Point(X, y), Size = new Size(W, 1),
                BackColor = Color.FromArgb(220, 228, 240)
            };
            scroll.Controls.Add(sep);
            y += 10;

            var lblStatus = new Label
            {
                Location = new Point(X, y), Size = new Size(W, 38),
                ForeColor = Colores.Acento, Font = new Font("Segoe UI", 9),
                Text = "", Visible = false
            };
            scroll.Controls.Add(lblStatus);
            y += 42;

            var btnUnirse = new Button
            {
                Text      = "🔗  Unirse al Servidor",
                Location  = new Point(X, y), Size = new Size(W, 46),
                BackColor = Colores.Secundario, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnUnirse.FlatAppearance.BorderSize = 0;
            btnUnirse.Click += (s, e) =>
            {
                lblStatus.Visible = false;
                string connStr = txtFull.Text.Trim();
                if (string.IsNullOrWhiteSpace(connStr))
                {
                    Alerta(lblStatus, "⚠ Completa los campos de conexión.");
                    return;
                }

                btnUnirse.Enabled = false;
                btnUnirse.Text    = "⏳  Verificando...";

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
                        Alerta(lblStatus,
                            "⚠ La base de datos existe pero no tiene el esquema de EmpresaApp.\n" +
                            "Usa 'Crear Servidor' para inicializarla.");
                        return;
                    }

                    DbConfig.ConnectionString = connStr;
                    ServerConfig.GuardarServidor(connStr);
                    ServidorConfigurado = true;
                    Exito(lblStatus, "✅ Conexión exitosa. ¡Bienvenido al servidor!");

                    System.Threading.Tasks.Task.Delay(1200).ContinueWith(_ =>
                        this.Invoke(() => { this.DialogResult = DialogResult.OK; }));
                }
                catch (Exception ex)
                {
                    Alerta(lblStatus, $"✗ No se pudo conectar:\n{ex.Message}");
                }
                finally
                {
                    btnUnirse.Enabled = true;
                    btnUnirse.Text    = "🔗  Unirse al Servidor";
                }
            };
            scroll.Controls.Add(btnUnirse);
            y += 54;

            var anchor = new Panel
            {
                Location = new Point(0, y), Size = new Size(1, 1),
                BackColor = Color.Transparent
            };
            scroll.Controls.Add(anchor);
        }

        // ════════════════════════════════════════════════════════
        //  UI Helpers
        // ════════════════════════════════════════════════════════
        private static void Lbl(Control parent, string text, ref int y, int x)
        {
            parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                ForeColor = Colores.TextoSecundario,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            });
            y += 20;
        }

        private static TextBox Txt(Control parent, string placeholder, string val,
            ref int y, int w, int x, bool password = false)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y), Size = new Size(w, 28),
                Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle,
                Text = val, PlaceholderText = placeholder
            };
            if (password) tb.PasswordChar = '●';
            parent.Controls.Add(tb);
            y += 34;
            return tb;
        }

        /// <summary>Textbox inside a sub-panel (no ref y).</summary>
        private static TextBox Txt2(Control parent, string placeholder,
            int x, int w, int y, bool password = false)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y + 2), Size = new Size(w, 28),
                Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = placeholder
            };
            if (password) tb.PasswordChar = '●';
            parent.Controls.Add(tb);
            return tb;
        }

        private static void Alerta(Label lbl, string texto)
        {
            lbl.ForeColor = Colores.Alerta;
            lbl.Text      = texto;
            lbl.Visible   = true;
        }

        private static void Exito(Label lbl, string texto)
        {
            lbl.ForeColor = Colores.Acento;
            lbl.Text      = texto;
            lbl.Visible   = true;
        }

        private static string ObtenerSqlSetup()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlSetup.sql");
            return File.Exists(path) ? File.ReadAllText(path) : ServerConfig.SqlSetupScript;
        }
    }
}
