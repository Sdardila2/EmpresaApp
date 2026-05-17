using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class DashboardForm : Form
    {
        private Panel _contentHost = null!;
        private Label _pageTitle = null!;
        private Label _pageSubtitle = null!;
        private readonly List<(string key, Button btn)> _nav = new();
        private string? _actual;

        public DashboardForm()
        {
            Text = "EmpresaApp";
            Size = new Size(1180, 720);
            MinimumSize = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;
            ModernTheme.ApplyToForm(this);

            var sidebar = BuildSidebar();
            var main = BuildMain();

            Controls.Add(main);
            Controls.Add(sidebar);

            FormClosing += (_, _) =>
            {
                if (Session.UsuarioActual != null)
                    DataManager.RegistrarSalida(Session.UsuarioActual.Id);
            };

            if (_nav.Count > 0)
                IrA(_nav[0].key);
        }

        private Panel BuildSidebar()
        {
            var side = new Panel
            {
                Dock = DockStyle.Left,
                Width = 248,
                BackColor = ModernTheme.Colors.Surface,
                Padding = new Padding(16, 20, 16, 16)
            };
            ModernTheme.EnableDoubleBuffer(side);

            // Logo
            var logo = ModernTheme.CreateLabel("EmpresaApp", ModernTheme.LabelStyle.Subheading);
            logo.Dock = DockStyle.Top;
            logo.Height = 32;
            logo.Padding = new Padding(4, 0, 0, 0);

            // Usuario
            var userCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                Margin = new Padding(0, 20, 0, 16),
                BackColor = Color.Transparent
            };
            ModernTheme.EnableDoubleBuffer(userCard);
            userCard.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, userCard.Width - 1, userCard.Height - 1);
                using var path = ModernTheme.RoundedRect(r, 8);
                using var fill = new SolidBrush(ModernTheme.Colors.Elevated);
                e.Graphics.FillPath(fill, path);
            };

            var u = Session.UsuarioActual;
            char iniN = !string.IsNullOrEmpty(u?.Nombre) ? char.ToUpper(u.Nombre[0]) : 'U';
            char iniA = !string.IsNullOrEmpty(u?.Apellido) ? char.ToUpper(u.Apellido[0]) : iniN;
            string iniciales = $"{iniN}{iniA}";

            var avatar = new Panel { Size = new Size(40, 40), Location = new Point(12, 16), BackColor = Color.Transparent };
            avatar.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var b = new SolidBrush(ModernTheme.Colors.AccentMuted);
                e.Graphics.FillEllipse(b, 0, 0, 38, 38);
                TextRenderer.DrawText(e.Graphics, iniciales, ModernTheme.FontSubheading,
                    new Rectangle(0, 0, 38, 38), ModernTheme.Colors.Accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lblName = ModernTheme.CreateLabel(Session.UsuarioActual?.NombreCompleto ?? "", ModernTheme.LabelStyle.Body);
            lblName.Location = new Point(60, 18);
            lblName.MaximumSize = new Size(160, 0);
            lblName.AutoEllipsis = true;

            var lblRole = ModernTheme.CreateLabel(
                Session.EsAdmin ? "Administrador" : "Empleado",
                ModernTheme.LabelStyle.Caption);
            lblRole.Location = new Point(60, 38);

            userCard.Controls.AddRange(new Control[] { avatar, lblName, lblRole });

            var sep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = ModernTheme.Colors.Border,
                Margin = new Padding(0, 0, 0, 12)
            };

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            void Nav(string key) => AddNav(nav, key);

            Nav("Inicio");
            Nav("Mensajes");
            Nav("Tareas");
            Nav("Notificaciones");
            Nav("Reporte diario");
            Nav("Mi asistencia");

            if (Session.EsAdmin)
            {
                var adminLbl = ModernTheme.CreateLabel("ADMINISTRACION", ModernTheme.LabelStyle.Caption);
                adminLbl.Width = 208;
                adminLbl.Margin = new Padding(6, 20, 0, 6);
                nav.Controls.Add(adminLbl);
                Nav("Empleados");
                Nav("Reportes equipo");
                Nav("Asistencia equipo");
                Nav("Enviar mensaje");
            }

            var btnSalir = ModernTheme.CreateButton("Cerrar sesion", ModernTheme.ButtonVariant.Ghost);
            btnSalir.Dock = DockStyle.Bottom;
            btnSalir.Height = 38;
            btnSalir.Click += BtnSalir_Click;

            side.Controls.Add(nav);
            side.Controls.Add(btnSalir);
            side.Controls.Add(sep);
            side.Controls.Add(userCard);
            side.Controls.Add(logo);
            return side;
        }

        private void AddNav(FlowLayoutPanel nav, string key)
        {
            var btn = ModernTheme.CreateNavItem(key);
            btn.Click += (_, _) => IrA(key);
            nav.Controls.Add(btn);
            _nav.Add((key, btn));
        }

        private Panel BuildMain()
        {
            var main = new Panel { Dock = DockStyle.Fill, BackColor = ModernTheme.Colors.Bg };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                Padding = new Padding(32, 16, 32, 0),
                BackColor = ModernTheme.Colors.Bg
            };

            var headerText = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            _pageTitle = ModernTheme.CreateLabel("Inicio", ModernTheme.LabelStyle.Heading);
            _pageTitle.Dock = DockStyle.Top;
            _pageTitle.AutoSize = false;
            _pageTitle.Height = 28;

            _pageSubtitle = ModernTheme.CreateLabel("", ModernTheme.LabelStyle.Caption);
            _pageSubtitle.Dock = DockStyle.Top;
            _pageSubtitle.AutoSize = false;
            _pageSubtitle.Height = 20;

            var headerBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = ModernTheme.Colors.Border
            };

            headerText.Controls.Add(_pageSubtitle);
            headerText.Controls.Add(_pageTitle);
            header.Controls.Add(headerBottom);
            header.Controls.Add(headerText);

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32, 20, 32, 28),
                BackColor = ModernTheme.Colors.Bg
            };
            ModernTheme.EnableDoubleBuffer(_contentHost);

            main.Controls.Add(_contentHost);
            main.Controls.Add(header);
            return main;
        }

        private void IrA(string key)
        {
            if (_actual == key) return;
            _actual = key;

            foreach (var (k, btn) in _nav)
                ModernTheme.SetNavSelected(btn, k == key);

            _pageTitle.Text = key;
            _pageSubtitle.Text = key switch
            {
                "Inicio" => "Resumen de tu actividad",
                "Mensajes" or "Tareas" => "Comunicacion interna",
                "Notificaciones" => "Alertas y avisos",
                "Reporte diario" => "Informe del dia",
                "Mi asistencia" or "Asistencia equipo" => "Registro de horarios",
                "Empleados" => "Gestion del personal",
                "Reportes equipo" => "Reportes del equipo",
                "Enviar mensaje" => "Nuevo mensaje o tarea",
                _ => ""
            };

            Control? ctrl = key switch
            {
                "Inicio" => new InicioControl(),
                "Mensajes" => new MensajesControl(TipoMensaje_Enum.Mensaje),
                "Tareas" => new MensajesControl(TipoMensaje_Enum.Tarea),
                "Notificaciones" => new NotificacionesControl(),
                "Reporte diario" => new ReporteControl(),
                "Mi asistencia" => new AsistenciaControl(false),
                "Empleados" => new GestionEmpleadosControl(),
                "Reportes equipo" => new TodosReportesControl(),
                "Asistencia equipo" => new AsistenciaControl(true),
                "Enviar mensaje" => new EnviarMensajeControl(),
                _ => null
            };

            if (ctrl == null) return;
            if (ctrl is UserControl uc) ModernTheme.ApplyToUserControl(uc);
            ModernTheme.SwapContent(_contentHost, ctrl);
            Text = $"EmpresaApp — {key}";
        }

        private void BtnSalir_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("¿Cerrar sesion?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (Session.UsuarioActual != null)
                DataManager.RegistrarSalida(Session.UsuarioActual.Id);
            Session.Cerrar();
            Close();
        }
    }

    public enum TipoMensaje_Enum { Mensaje, Tarea }
}
