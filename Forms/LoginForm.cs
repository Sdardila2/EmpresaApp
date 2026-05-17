using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsuario = null!;
        private TextBox txtPassword = null!;
        private Label lblError = null!;

        public LoginForm()
        {
            Text = "EmpresaApp";
            Size = new Size(900, 540);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ModernTheme.ApplyToForm(this);

            var brand = new Panel { Dock = DockStyle.Left, Width = 360 };
            ModernTheme.EnableDoubleBuffer(brand);
            brand.Paint += PaintBrand;

            var formArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(32, 40, 32, 40) };

            var stack = UiLayout.CreateFormStack();
            stack.Width = 380;

            UiLayout.StackRow(stack, ModernTheme.CreateLabel("Bienvenido", ModernTheme.LabelStyle.Heading));
            UiLayout.StackRow(stack, ModernTheme.CreateLabel("Ingresa tus credenciales", ModernTheme.LabelStyle.Caption));
            UiLayout.StackRow(stack, new Panel { Height = 20 }, 20);

            UiLayout.StackLabel(stack, "Usuario");
            txtUsuario = new TextBox();
            UiLayout.StackRow(stack, ModernTheme.CreateInput(txtUsuario), 44);

            UiLayout.StackLabel(stack, "Contrasena");
            txtPassword = new TextBox { UseSystemPasswordChar = true };
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; IniciarSesion(); }
            };
            UiLayout.StackRow(stack, ModernTheme.CreateInput(txtPassword), 44);

            lblError = new Label
            {
                ForeColor = ModernTheme.Colors.Danger,
                Font = ModernTheme.FontCaption,
                AutoSize = true,
                Visible = false
            };
            UiLayout.StackRow(stack, lblError);

            var btnLogin = ModernTheme.CreateButton("Iniciar sesion", ModernTheme.ButtonVariant.Primary);
            btnLogin.Click += (_, _) => IniciarSesion();
            UiLayout.StackRow(stack, btnLogin, UiLayout.ButtonHeight);

            var btnSrv = ModernTheme.CreateButton("Configurar servidor", ModernTheme.ButtonVariant.Ghost);
            btnSrv.Click += (_, _) =>
            {
                ServerConfig.EliminarServidor();
                using var dlg = new ServidorForm();
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ServidorConfigurado)
                    DataManager.Inicializar(DbConfig.ConnectionString);
            };
            UiLayout.StackRow(stack, btnSrv, UiLayout.ButtonHeight);

            var center = new TableLayoutPanel();
            UiLayout.AddCentered(center, stack, 380);
            formArea.Controls.Add(center);

            Controls.Add(formArea);
            Controls.Add(brand);
        }

        private static void PaintBrand(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var panel = (Panel)sender!;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = panel.ClientRectangle;

            using (var grad = new LinearGradientBrush(r,
                Color.FromArgb(28, 24, 48), Color.FromArgb(12, 12, 16), 90f))
                g.FillRectangle(grad, r);

            using (var bar = new SolidBrush(ModernTheme.Colors.Accent))
                g.FillRectangle(bar, 0, 0, 3, r.Height);

            using var title = new Font("Segoe UI Semibold", 30f);
            g.DrawString("Empresa", title, Brushes.White, 52, 88);
            using var accentBrush = new SolidBrush(ModernTheme.Colors.Accent);
            g.DrawString("App", title, accentBrush, 52, 128);

            using var sub = new Font("Segoe UI", 10.5f);
            using var subBrush = new SolidBrush(ModernTheme.Colors.TextMuted);
            g.DrawString("Gestion de equipo y asistencia\nen tu red local.",
                sub, subBrush, 52, 188);
        }

        private void IniciarSesion()
        {
            lblError.Visible = false;
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Complete usuario y contrasena.";
                lblError.Visible = true;
                return;
            }
            try
            {
                DataManager.Inicializar(DbConfig.ConnectionString);
                var usuario = DataManager.ObtenerUsuarioPorLogin(txtUsuario.Text.Trim(), txtPassword.Text);
                if (usuario == null)
                {
                    lblError.Text = "Usuario o contrasena incorrectos.";
                    lblError.Visible = true;
                    return;
                }
                Session.UsuarioActual = usuario;
                DataManager.RegistrarEntrada(usuario.Id);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo conectar al servidor.";
                lblError.Visible = true;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
