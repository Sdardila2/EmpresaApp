using System;
using System.Drawing;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class DashboardForm : Form
    {
        private Panel panelSidebar = null!;
        private Panel panelContent = null!;
        private Panel panelHeader = null!;
        private Label lblTituloPagina = null!;
        private Label lblNotifBadge = null!;
        private System.Windows.Forms.Timer timerRefresh = null!;

        public DashboardForm()
        {
            InitializeComponent();
            CargarPaginaInicio();
            IniciarTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "EmpresaApp";
            this.Size = new Size(1200, 750);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Colores.Fondo;
            this.Font = new Font("Segoe UI", 9.5f);

            // Sidebar
            panelSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Colores.Primario
            };

            // Sidebar header user info
            var pnlUser = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(15, 40, 75) };
            var lblUserIcon = new Label
            {
                Text = Session.EsAdmin ? "👑" : "👤",
                Font = new Font("Segoe UI Emoji", 26),
                ForeColor = Color.White,
                Location = new Point(90, 12),
                AutoSize = true
            };
            var lblUserName = new Label
            {
                Text = Session.UsuarioActual?.NombreCompleto ?? "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 62),
                Size = new Size(210, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var lblUserRole = new Label
            {
                Text = Session.UsuarioActual?.Rol.ToString() ?? "",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(148, 187, 233),
                Location = new Point(10, 85),
                Size = new Size(210, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var lblUserDept = new Label
            {
                Text = Session.UsuarioActual?.Departamento ?? "",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(100, 150, 200),
                Location = new Point(10, 103),
                Size = new Size(210, 16),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlUser.Controls.AddRange(new Control[] { lblUserIcon, lblUserName, lblUserRole, lblUserDept });

            // Menu items
            var menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,   // scrollbar si menu crece
                Padding = new Padding(0, 10, 0, 0),
                BackColor = Colores.Primario
            };

            void AddMenuItem(string icon, string texto, Action accion)
            {
                var btn = new Button
                {
                    Text = $"  {icon}  {texto}",
                    Size = new Size(230, 46),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Colores.Primario,
                    ForeColor = Color.FromArgb(200, 220, 240),
                    Font = new Font("Segoe UI", 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 1, 0, 1)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
                btn.Click += (s, e) => { accion(); ActualizarBadge(); };
                menuPanel.Controls.Add(btn);
            }

            AddMenuItem("🏠", "Inicio", CargarPaginaInicio);
            AddMenuItem("📨", "Mensajes", CargarMensajes);
            AddMenuItem("✅", "Mis Tareas", CargarTareas);
            AddMenuItem("🔔", "Notificaciones", CargarNotificaciones);
            AddMenuItem("📊", "Reporte Diario", CargarReporte);
            AddMenuItem("📅", "Mi Asistencia", CargarAsistencia);

            if (Session.EsAdmin)
            {
                var sep = new Label { Size = new Size(230, 1), BackColor = Color.FromArgb(50, 80, 120), Margin = new Padding(0, 8, 0, 8) };
                menuPanel.Controls.Add(sep);
                var lblAdmin = new Label
                {
                    Text = "  ADMINISTRACIÓN",
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 150, 200),
                    Size = new Size(230, 24),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                menuPanel.Controls.Add(lblAdmin);
                AddMenuItem("👥", "Gestión Empleados", CargarGestionEmpleados);
                AddMenuItem("📋", "Todos los Reportes", CargarTodosReportes);
                AddMenuItem("📊", "Registro Asistencia", CargarTodaAsistencia);
                AddMenuItem("📢", "Enviar Comunicado", CargarEnviarComunicado);
            }

            // Badge notificaciones
            lblNotifBadge = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Colores.Alerta,
                Size = new Size(0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Bottom logout
            var btnLogout = new Button
            {
                Text = "  🚪  Cerrar Sesión",
                Dock = DockStyle.Bottom,
                Height = 46,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(15, 40, 75),
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;

            panelSidebar.Controls.Add(menuPanel);
            panelSidebar.Controls.Add(btnLogout);
            panelSidebar.Controls.Add(pnlUser);

            // Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            panelHeader.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1),
                    0, panelHeader.Height - 1, panelHeader.Width, panelHeader.Height - 1);
            };

            lblTituloPagina = new Label
            {
                Text = "Inicio",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Colores.TextoPrimario,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblFecha = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("es-ES")),
                Font = new Font("Segoe UI", 9),
                ForeColor = Colores.TextoSecundario,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                AutoSize = true
            };
            lblFecha.Location = new Point(this.Width - 230 - 20, 22);

            panelHeader.Controls.Add(lblTituloPagina);
            panelHeader.Controls.Add(lblFecha);

            // Content
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colores.Fondo,
                Padding = new Padding(20),
                AutoScroll = true  // scrollbar en contenido principal
            };

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);

            this.FormClosing += DashboardForm_FormClosing;
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
        }

        private void CargarPaginaInicio() => CambiarContenido(new InicioControl(), "🏠 Panel de Inicio");
        private void CargarMensajes() => CambiarContenido(new MensajesControl(TipoMensaje_Enum.Mensaje), "📨 Mensajes");
        private void CargarTareas() => CambiarContenido(new MensajesControl(TipoMensaje_Enum.Tarea), "✅ Mis Tareas");
        private void CargarNotificaciones() => CambiarContenido(new NotificacionesControl(), "🔔 Notificaciones");
        private void CargarReporte() => CambiarContenido(new ReporteControl(), "📊 Reporte Diario");
        private void CargarAsistencia() => CambiarContenido(new AsistenciaControl(false), "📅 Mi Asistencia");
        private void CargarGestionEmpleados() => CambiarContenido(new GestionEmpleadosControl(), "👥 Gestión de Empleados");
        private void CargarTodosReportes() => CambiarContenido(new TodosReportesControl(), "📋 Reportes del Equipo");
        private void CargarTodaAsistencia() => CambiarContenido(new AsistenciaControl(true), "📊 Registro de Asistencia");
        private void CargarEnviarComunicado() => CambiarContenido(new EnviarMensajeControl(), "📢 Nuevo Mensaje / Tarea");

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (Session.UsuarioActual != null)
                    DataManager.RegistrarSalida(Session.UsuarioActual.Id);
                timerRefresh.Stop();
                Session.Cerrar();
                this.Close();
            }
        }

        private void DashboardForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (Session.UsuarioActual != null)
                DataManager.RegistrarSalida(Session.UsuarioActual.Id);
            timerRefresh?.Stop();
        }
    }

    public enum TipoMensaje_Enum { Mensaje, Tarea }
}
