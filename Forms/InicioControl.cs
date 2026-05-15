using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class InicioControl : UserControl
    {
        public InicioControl()
        {
            this.BackColor = Colores.Fondo;
            this.Padding = new Padding(5);
            CargarContenido();
        }

        private void CargarContenido()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // Bienvenida
            var pnlBienvenida = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(900, 100),
                BackColor = Colores.Secundario
            };
            pnlBienvenida.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlBienvenida.ClientRectangle,
                    Colores.Secundario, Color.FromArgb(79, 70, 229),
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, pnlBienvenida.ClientRectangle);
            };

            var hora = DateTime.Now.Hour;
            var saludo = hora < 12 ? "☀️ Buenos días" : hora < 18 ? "🌤️ Buenas tardes" : "🌙 Buenas noches";
            var lblBienvenida = new Label
            {
                Text = $"{saludo}, {Session.UsuarioActual?.Nombre}!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 18),
                AutoSize = true
            };
            var lblFecha = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy - HH:mm", new System.Globalization.CultureInfo("es-ES")),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 230, 255),
                Location = new Point(25, 58),
                AutoSize = true
            };
            pnlBienvenida.Controls.AddRange(new Control[] { lblBienvenida, lblFecha });

            // Stats cards
            var statsFlow = new FlowLayoutPanel
            {
                Location = new Point(0, 115),
                Size = new Size(900, 130),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Colores.Fondo
            };

            var uid = Session.UsuarioActual?.Id ?? "";
            int mensajes = DataManager.ContarMensajesNuevos(uid);
            int notifs = DataManager.ContarNotificacionesNoLeidas(uid);
            bool tieneReporte = DataManager.TieneReporteHoy(uid);
            var registroHoy = DataManager.ObtenerRegistroAbierto(uid);

            AgregarStatCard(statsFlow, "📨", "Mensajes Nuevos", mensajes.ToString(), mensajes > 0 ? Colores.Secundario : Colores.Acento);
            AgregarStatCard(statsFlow, "🔔", "Alertas Pendientes", notifs.ToString(), notifs > 0 ? Colores.Alerta : Colores.Acento);
            AgregarStatCard(statsFlow, registroHoy != null ? "🟢" : "🔴", "Estado Hoy", registroHoy != null ? "Activo" : "Sin entrada", registroHoy != null ? Colores.Acento : Colores.Advertencia);
            AgregarStatCard(statsFlow, tieneReporte ? "✅" : "⚠️", "Reporte Diario", tieneReporte ? "Enviado" : "Pendiente", tieneReporte ? Colores.Acento : Colores.Advertencia);

            // Mensajes recientes
            int y = 260;
            var lblRecientes = CrearTituloSeccion("📬 Mensajes Recientes", y);
            y += 40;

            var mensajesRecientes = DataManager.ObtenerMensajesDeUsuario(uid).Take(5).ToList();
            if (mensajesRecientes.Count == 0)
            {
                var lbl = new Label { Text = "  No tienes mensajes recientes.", Location = new Point(0, y), Size = new Size(880, 40), ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 10) };
                scroll.Controls.Add(lbl);
                y += 50;
            }
            else
            {
                foreach (var m in mensajesRecientes)
                {
                    var card = CrearMensajeCard(m, y);
                    scroll.Controls.Add(card);
                    y += 65;
                }
            }

            // Notificaciones recientes (solo admin)
            if (Session.EsAdmin)
            {
                y += 10;
                scroll.Controls.Add(CrearTituloSeccion("🚨 Alertas Recientes del Equipo", y));
                y += 40;

                var alertas = DataManager.ObtenerNotificacionesParaAdmin().Take(5).ToList();
                if (alertas.Count == 0)
                {
                    var lbl = new Label { Text = "  No hay alertas recientes.", Location = new Point(0, y), Size = new Size(880, 40), ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 10) };
                    scroll.Controls.Add(lbl);
                    y += 50;
                }
                else
                {
                    foreach (var n in alertas)
                    {
                        var card = CrearAlertaCard(n, y);
                        scroll.Controls.Add(card);
                        y += 70;
                    }
                }
            }

            scroll.Controls.Add(pnlBienvenida);
            scroll.Controls.Add(statsFlow);
            scroll.Controls.Add(lblRecientes);

            this.Controls.Add(scroll);
        }

        private void AgregarStatCard(FlowLayoutPanel parent, string icon, string titulo, string valor, Color color)
        {
            var card = new Panel
            {
                Size = new Size(190, 110),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 15, 0),
                Cursor = Cursors.Default
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(color), 0, 0, 5, card.Height);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, card.Width - 1, card.Height - 1);
            };

            var lblIcon = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 22), Location = new Point(12, 10), AutoSize = true };
            var lblVal = new Label { Text = valor, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = color, Location = new Point(12, 45), AutoSize = true };
            var lblTit = new Label { Text = titulo, Font = new Font("Segoe UI", 8.5f), ForeColor = Colores.TextoSecundario, Location = new Point(12, 85), AutoSize = true };

            card.Controls.AddRange(new Control[] { lblIcon, lblVal, lblTit });
            parent.Controls.Add(card);
        }

        private Label CrearTituloSeccion(string texto, int y)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Colores.TextoPrimario,
                Location = new Point(0, y),
                AutoSize = true
            };
        }

        private Panel CrearMensajeCard(EmpresaApp.Models.Mensaje m, int y)
        {
            var card = new Panel { Location = new Point(0, y), Size = new Size(880, 58), BackColor = Color.White };
            card.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, card.Width - 1, card.Height - 1);

            var esNuevo = m.Estado == EmpresaApp.Models.EstadoMensaje.Nuevo;
            var iconoTipo = m.Tipo == EmpresaApp.Models.TipoMensaje.Tarea ? "✅" : m.Tipo == EmpresaApp.Models.TipoMensaje.Alerta ? "🚨" : "📩";
            Color colorBorde = m.Tipo == EmpresaApp.Models.TipoMensaje.Alerta ? Colores.Alerta : m.Tipo == EmpresaApp.Models.TipoMensaje.Tarea ? Colores.Advertencia : Colores.Secundario;
            card.Paint += (s, e) => e.Graphics.FillRectangle(new SolidBrush(colorBorde), 0, 0, 4, card.Height);

            var lblRemitente = new Label { Text = $"{iconoTipo} De: {m.RemitenteNombre}", Font = new Font("Segoe UI", 9.5f, esNuevo ? FontStyle.Bold : FontStyle.Regular), ForeColor = Colores.TextoPrimario, Location = new Point(15, 8), AutoSize = true };
            var lblAsunto = new Label { Text = m.Asunto, Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(15, 30), AutoSize = true };
            var lblFecha = new Label { Text = m.FechaEnvio.ToString("dd/MM HH:mm"), Font = new Font("Segoe UI", 8), ForeColor = Colores.TextoSecundario, Location = new Point(810, 10), AutoSize = true };
            if (esNuevo) { var badge = new Label { Text = "NUEVO", Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.White, BackColor = Colores.Secundario, Location = new Point(760, 10), Size = new Size(45, 18), TextAlign = ContentAlignment.MiddleCenter }; card.Controls.Add(badge); }

            card.Controls.AddRange(new Control[] { lblRemitente, lblAsunto, lblFecha });
            return card;
        }

        private Panel CrearAlertaCard(EmpresaApp.Models.Notificacion n, int y)
        {
            var card = new Panel { Location = new Point(0, y), Size = new Size(880, 62), BackColor = Color.White };
            Color colorAlerta = n.Tipo == "Urgente" ? Colores.Alerta : n.Tipo == "Alerta" ? Colores.Advertencia : Colores.Secundario;
            card.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(colorAlerta), 0, 0, 4, card.Height);
            };
            var icono = n.Tipo == "Urgente" ? "🆘" : n.Tipo == "Alerta" ? "⚠️" : "ℹ️";
            var lblDe = new Label { Text = $"{icono} {n.RemitenteNombre} ({n.RemitenteDepartamento})", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Colores.TextoPrimario, Location = new Point(15, 8), AutoSize = true };
            var lblMsg = new Label { Text = n.Mensaje.Length > 80 ? n.Mensaje.Substring(0, 77) + "..." : n.Mensaje, Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(15, 30), AutoSize = true };
            var lblFecha = new Label { Text = n.Fecha.ToString("dd/MM HH:mm"), Font = new Font("Segoe UI", 8), ForeColor = Colores.TextoSecundario, Location = new Point(810, 8), AutoSize = true };
            card.Controls.AddRange(new Control[] { lblDe, lblMsg, lblFecha });
            return card;
        }
    }
}
