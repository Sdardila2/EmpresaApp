// ???????????????????????????????????????????????????????????????
//  LoginFormModern.cs — Modern animated login form
//  ???????????????????????????????????????????????????????????????
using System;
using System.Drawing;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;
using EmpresaApp.Services;

namespace EmpresaApp.Forms
{
    public class LoginFormModern : Form
    {
        private TextBox txtUsuario = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblError = null!;
        private Panel pnlLoading = null!;
        private ProgressBar pbLoading = null!;
        private Label lblConnectionStatus = null!;

        public LoginFormModern()
        {
            InitializeComponent();
            CheckNetworkStatus();
        }

        private void InitializeComponent()
        {
            this.Text = "EmpresaApp - Iniciar Sesión";
            this.Size = new Size(480, 700);
            this.MinimumSize = new Size(480, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = ModernTheme.Colors.Background;
            this.Font = new Font("Segoe UI", 10);
            this.DoubleBuffered = true;

            // ?? PANEL SUPERIOR (Gradiente) ??????????????????????
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = ModernTheme.Colors.Primary };
            pnlTop.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(pnlTop.Width, pnlTop.Height),
                    ModernTheme.Colors.Primary,
                    ModernTheme.Colors.Secondary))
                {
                    brush.GammaCorrection = true;
                    e.Graphics.FillRectangle(brush, pnlTop.ClientRectangle);
                }
            };

            var iconLabel = new Label
            {
                Text = "??",
                Font = new Font("Segoe UI Emoji", 48),
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 12, 0, 0)
            };

            var titleLabel = new Label
            {
                Text = "EmpresaApp",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 8, 0, 0)
            };

            var subtitleLabel = new Label
            {
                Text = "Sistema de Gestión Empresarial",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 220, 240),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 12)
            };

            pnlTop.Controls.AddRange(new Control[] { subtitleLabel, titleLabel, iconLabel });

            // ?? PANEL PRINCIPAL ????????????????????????????????
            var pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = ModernTheme.Colors.Background, Padding = new Padding(32) };

            var loginTitle = new Label
            {
                Text = "Iniciar Sesión",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 24)
            };

            // ?? USUARIO ????????????????????????????????????????
            var lblUser = new Label
            {
                Text = "Usuario o Email",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };

            txtUsuario = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Height = 44,
                Padding = new Padding(12),
                BackColor = ModernTheme.Colors.Light,
                ForeColor = ModernTheme.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            ModernTheme.ApplyModernTextBoxStyle(txtUsuario);

            // ?? CONTRASEÑA ?????????????????????????????????????
            var lblPassword = new Label
            {
                Text = "Contraseña",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 16, 0, 6)
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Height = 44,
                Padding = new Padding(12),
                BackColor = ModernTheme.Colors.Light,
                ForeColor = ModernTheme.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '?'
            };
            ModernTheme.ApplyModernTextBoxStyle(txtPassword);
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnLogin_Click(s, e);
                }
            };

            // ?? ERROR MESSAGE ??????????????????????????????????
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = ModernTheme.Colors.Danger,
                AutoSize = true,
                Visible = false,
                Margin = new Padding(0, 12, 0, 0),
                MaximumSize = new Size(400, 100)
            };

            // ?? LOGIN BUTTON ???????????????????????????????????
            btnLogin = new Button
            {
                Text = "INICIAR SESIÓN",
                Height = 48,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 24, 0, 0),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            ModernTheme.ApplyModernButtonStyle(btnLogin, ModernTheme.Colors.Primary);
            btnLogin.Click += BtnLogin_Click;

            // ?? LOADING PANEL ??????????????????????????????????
            pnlLoading = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(200, 0, 0, 0),
                Visible = false,
                Padding = new Padding(20)
            };

            var pnlLoadingContent = new Panel
            {
                BackColor = ModernTheme.Colors.CardBackground,
                Dock = DockStyle.None,
                Size = new Size(300, 150),
                Padding = new Padding(20)
            };
            ModernTheme.ApplyModernCardStyle(pnlLoadingContent);

            var lblLoading = new Label
            {
                Text = "Iniciando sesión...",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 30
            };

            pbLoading = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 8,
                Margin = new Padding(0, 12, 0, 0),
                Style = ProgressBarStyle.Marquee
            };
            ModernTheme.ApplyModernProgressBarStyle(pbLoading);

            pnlLoadingContent.Controls.Add(pbLoading);
            pnlLoadingContent.Controls.Add(lblLoading);
            pnlLoading.Controls.Add(pnlLoadingContent);

            // ?? CONNECTION STATUS ??????????????????????????????
            lblConnectionStatus = new Label
            {
                Text = "? Conectado a la red",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ModernTheme.Colors.Success,
                AutoSize = true,
                Margin = new Padding(0, 16, 0, 0),
                TextAlign = ContentAlignment.TopCenter
            };

            // ?? ASSEMBLY MAIN ??????????????????????????????????
            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            container.Controls.AddRange(new Control[]
            {
                loginTitle, lblUser, txtUsuario,
                lblPassword, txtPassword, lblError,
                btnLogin, lblConnectionStatus
            });

            pnlMain.Controls.Add(container);

            this.Controls.Add(pnlLoading);
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlTop);
        }

        private void CheckNetworkStatus()
        {
            var validator = LANNetworkValidator.Instance;
            if (validator.CheckNetworkConnectivity())
            {
                lblConnectionStatus.Text = "? Conectado a la red";
                lblConnectionStatus.ForeColor = ModernTheme.Colors.Success;
            }
            else
            {
                lblConnectionStatus.Text = "? Sin conexión de red";
                lblConnectionStatus.ForeColor = ModernTheme.Colors.Warning;
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Por favor ingrese usuario y contraseña");
                return;
            }

            btnLogin.Enabled = false;
            pnlLoading.Visible = true;

            // Simular validación en background
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1500;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ValidarLogin(usuario, password);
                btnLogin.Enabled = true;
                pnlLoading.Visible = false;
                timer.Dispose();
            };
            timer.Start();
        }

        private void ValidarLogin(string usuario, string password)
        {
            try
            {
                DataManager.Inicializar(DbConfig.ConnectionString);
                var user = DataManager.ObtenerUsuarioPorLogin(usuario, password);

                if (user != null)
                {
                    // Initialize session
                    Session.UsuarioActual = user;
                    DataManager.RegistrarEntrada(user.Id);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MostrarError("Usuario o contraseña inválidos");
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error: {ex.Message}");
            }
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visible = true;
            ModernTheme.AnimatePulse(lblError);
        }
    }
}
