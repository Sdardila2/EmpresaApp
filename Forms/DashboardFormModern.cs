// ???????????????????????????????????????????????????????????????
//  DashboardFormModern.cs — Modern responsive dashboard with sync
//  ???????????????????????????????????????????????????????????????
using System;
using System.Drawing;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;
using EmpresaApp.Services;

namespace EmpresaApp.Forms
{
    public class DashboardFormModern : Form
    {
        private Panel panelSidebar = null!;
        private Panel panelContent = null!;
        private Panel panelHeader = null!;
        private Label lblTituloPagina = null!;
        private Label lblConnectionStatus = null!;
        private Panel pnlNotificationToast = null!;
        private System.Windows.Forms.Timer timerRefresh = null!;
        private SyncService syncService = null!;

        public DashboardFormModern()
        {
            InitializeComponent();
            InitializeSync();
            CargarPaginaInicio();
            IniciarTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "EmpresaApp";
            this.Size = new Size(1400, 800);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ModernTheme.Colors.Background;
            this.Font = new Font("Segoe UI", 9.5f);
            this.DoubleBuffered = true;

            // ??? SIDEBAR ???????????????????????????????????????????
            panelSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = ModernTheme.Colors.Primary,
                Padding = new Padding(0)
            };
            panelSidebar.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(0, panelSidebar.Height),
                    ModernTheme.Colors.Primary,
                    Color.FromArgb(79, 70, 229)))
                {
                    brush.GammaCorrection = true;
                    e.Graphics.FillRectangle(brush, panelSidebar.ClientRectangle);
                }
            };

            // User Panel
            var pnlUser = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = ModernTheme.Colors.CardBackground,
                Padding = new Padding(12)
            };
            pnlUser.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(50, 80, 120), 1),
                    0, pnlUser.Height - 1, pnlUser.Width, pnlUser.Height - 1);
            };

            var userFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false
            };

            var lblUserIcon = new Label
            {
                Text = Session.EsAdmin ? "??" : "??",
                Font = new Font("Segoe UI Emoji", 32),
                ForeColor = Color.White,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 236
            };

            var lblUserName = new Label
            {
                Text = Session.UsuarioActual?.NombreCompleto ?? "Usuario",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 236,
                Height = 28,
                Padding = new Padding(0, 6, 0, 0)
            };

            var lblUserRole = new Label
            {
                Text = Session.UsuarioActual?.Rol.ToString() ?? "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(148, 187, 233),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 236,
                Height = 18
            };

            userFlow.Controls.AddRange(new Control[] { lblUserIcon, lblUserName, lblUserRole });
            pnlUser.Controls.Add(userFlow);

            // Menu Panel
            var menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 12, 0, 0),
                BackColor = ModernTheme.Colors.Primary
            };

            void AddMenuItem(string icon, string texto, Action accion)
            {
                var btn = new Button
                {
                    Text = $"  {icon}  {texto}",
                    Size = new Size(260, 50),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ModernTheme.Colors.Primary,
                    ForeColor = Color.FromArgb(200, 220, 240),
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 0, 2)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
                btn.Click += (s, e) => { accion(); ActualizarBadge(); };
                menuPanel.Controls.Add(btn);
            }

            AddMenuItem("??", "Inicio", CargarPaginaInicio);
            AddMenuItem("??", "Mensajes", CargarMensajes);
            AddMenuItem("?", "Mis Tareas", CargarTareas);
            AddMenuItem("??", "Notificaciones", CargarNotificaciones);
            AddMenuItem("??", "Reporte Diario", CargarReporte);
            AddMenuItem("??", "Mi Asistencia", CargarAsistencia);

            if (Session.EsAdmin)
            {
                var sep = new Label
                {
                    Size = new Size(260, 1),
                    BackColor = Color.FromArgb(50, 80, 120),
                    Margin = new Padding(0, 10, 0, 10)
                };
                menuPanel.Controls.Add(sep);

                var lblAdmin = new Label
                {
                    Text = "  ADMINISTRACIÓN",
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 150, 200),
                    Size = new Size(260, 30),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(0, 4, 0, 4)
                };
                menuPanel.Controls.Add(lblAdmin);

                AddMenuItem("??", "Gestión Empleados", CargarGestionEmpleados);
                AddMenuItem("??", "Todos los Reportes", CargarTodosReportes);
                AddMenuItem("??", "Registro Asistencia", CargarTodaAsistencia);
                AddMenuItem("??", "Enviar Comunicado", CargarEnviarComunicado);
            }

            // Logout Button
            var btnLogout = new Button
            {
                Text = "  ??  Cerrar Sesión",
                Dock = DockStyle.Bottom,
                Height = 52,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(15, 40, 75),
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 50, 90);
            btnLogout.Click += BtnLogout_Click;

            panelSidebar.Controls.Add(menuPanel);
            panelSidebar.Controls.Add(btnLogout);
            panelSidebar.Controls.Add(pnlUser);

            // ??? HEADER ????????????????????????????????????????
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 12, 20, 12)
            };
            panelHeader.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(ModernTheme.Colors.Border, 1),
                    0, panelHeader.Height - 1, panelHeader.Width, panelHeader.Height - 1);
            };

            var headerFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            lblTituloPagina = new Label
            {
                Text = "Inicio",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.TextPrimary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblConnectionStatus = new Label
            {
                Text = "?",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = ModernTheme.Colors.Success,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(0, 0, 20, 0)
            };

            headerFlow.Controls.Add(lblTituloPagina);
            headerFlow.Controls.Add(lblConnectionStatus);
            panelHeader.Controls.Add(headerFlow);

            // ??? CONTENT ????????????????????????????????????????
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Colors.Background,
                Padding = new Padding(16),
                AutoScroll = true
            };

            // ??? NOTIFICATION TOAST ????????????????????????????
            pnlNotificationToast = new Panel
            {
                Size = new Size(300, 80),
                Location = new Point(20, 80),
                BackColor = ModernTheme.Colors.Success,
                Visible = false,
                Padding = new Padding(12)
            };
            ModernTheme.ApplyModernCardStyle(pnlNotificationToast);
            this.Controls.Add(pnlNotificationToast);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);

            this.FormClosing += DashboardForm_FormClosing;
        }

        private void InitializeSync()
        {
            syncService = SyncService.Instance;
            syncService.Subscribe(OnSyncEvent);
        }

        private void OnSyncEvent(SyncEvent syncEvent)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnSyncEvent(syncEvent)));
                return;
            }

            switch (syncEvent.Tipo)
            {
                case SyncEventType.ConexionPerdida:
                    lblConnectionStatus.Text = "?";
                    lblConnectionStatus.ForeColor = ModernTheme.Colors.Danger;
                    MostrarToast("Conexión perdida", ModernTheme.Colors.Danger);
                    break;

                case SyncEventType.ConexionRestaurada:
                    lblConnectionStatus.Text = "?";
                    lblConnectionStatus.ForeColor = ModernTheme.Colors.Success;
                    MostrarToast("Conexión restaurada", ModernTheme.Colors.Success);
                    break;

                case SyncEventType.MensajeNuevo:
                    MostrarToast("?? Nuevo mensaje", ModernTheme.Colors.Primary);
                    break;

                case SyncEventType.TareaCompletada:
                    MostrarToast("? Tarea completada", ModernTheme.Colors.Success);
                    break;

                case SyncEventType.ReporteEnviado:
                    MostrarToast("?? Reporte enviado", ModernTheme.Colors.Primary);
                    break;

                case SyncEventType.NotificacionNueva:
                    MostrarToast("?? Nueva notificación", ModernTheme.Colors.Warning);
                    break;
            }
        }

        private void MostrarToast(string mensaje, Color color)
        {
            pnlNotificationToast.BackColor = color;
            var lblMsg = new Label
            {
                Text = mensaje,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlNotificationToast.Controls.Clear();
            pnlNotificationToast.Controls.Add(lblMsg);
            pnlNotificationToast.Visible = true;

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                pnlNotificationToast.Visible = false;
                timer.Dispose();
            };
            timer.Start();
        }

        private void IniciarTimer()
        {
            timerRefresh = new System.Windows.Forms.Timer { Interval = 15000 };
            timerRefresh.Tick += (s, e) => ActualizarBadge();
            timerRefresh.Start();
            ActualizarBadge();
        }

        private void ActualizarBadge()
        {
            if (Session.UsuarioActual == null) return;
            int msgs = DataManager.ContarMensajesNuevos(Session.UsuarioActual.Id);
            int notifs = DataManager.ContarNotificacionesNoLeidas(Session.UsuarioActual.Id);
        }

        private void CambiarContenido(Control control, string titulo)
        {
            lblTituloPagina.Text = titulo;
            panelContent.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Add(control);
            ModernTheme.AnimateFadeIn(control, 300);
        }

        private void CargarPaginaInicio() => CambiarContenido(new InicioControl(), "?? Panel de Inicio");
        private void CargarMensajes() => CambiarContenido(new MensajesControl(TipoMensaje_Enum.Mensaje), "?? Mensajes");
        private void CargarTareas() => CambiarContenido(new MensajesControl(TipoMensaje_Enum.Tarea), "? Mis Tareas");
        private void CargarNotificaciones() => CambiarContenido(new NotificacionesControl(), "?? Notificaciones");
        private void CargarReporte() => CambiarContenido(new ReporteControl(), "?? Reporte Diario");
        private void CargarAsistencia() => CambiarContenido(new AsistenciaControl(false), "?? Mi Asistencia");
        private void CargarGestionEmpleados() => CambiarContenido(new GestionEmpleadosControl(), "?? Gestión de Empleados");
        private void CargarTodosReportes() => CambiarContenido(new TodosReportesControl(), "?? Reportes del Equipo");
        private void CargarTodaAsistencia() => CambiarContenido(new AsistenciaControl(true), "?? Registro de Asistencia");
        private void CargarEnviarComunicado() => CambiarContenido(new EnviarMensajeControl(), "?? Nuevo Mensaje / Tarea");

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (Session.UsuarioActual != null)
                    DataManager.RegistrarSalida(Session.UsuarioActual.Id);
                timerRefresh.Stop();
                syncService.Stop();
                Session.Cerrar();
                this.Close();
            }
        }

        private void DashboardForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (Session.UsuarioActual != null)
                DataManager.RegistrarSalida(Session.UsuarioActual.Id);
            timerRefresh?.Stop();
            syncService?.Stop();
        }
    }
}
