using System;
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
            ModernTheme.ApplyToUserControl(this);
            Dock = DockStyle.Fill;

            var uid = Session.UsuarioActual?.Id ?? "";
            int mensajes = DataManager.ContarMensajesNuevos(uid);
            int notifs = DataManager.ContarNotificacionesNoLeidas(uid);
            bool tieneReporte = DataManager.TieneReporteHoy(uid);
            var registro = DataManager.ObtenerRegistroAbierto(uid);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = ModernTheme.Colors.Bg
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Métricas
            var metrics = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                BackColor = ModernTheme.Colors.Bg,
                Margin = new Padding(0, 0, 0, 20)
            };
            metrics.Controls.Add(ModernTheme.CreateStatTile("Mensajes", mensajes.ToString(), ModernTheme.Colors.Accent));
            metrics.Controls.Add(ModernTheme.CreateStatTile("Notificaciones", notifs.ToString(), ModernTheme.Colors.Warning));
            metrics.Controls.Add(ModernTheme.CreateStatTile("Asistencia",
                registro != null ? "Activa" : "Pendiente",
                registro != null ? ModernTheme.Colors.Success : ModernTheme.Colors.TextDim));
            metrics.Controls.Add(ModernTheme.CreateStatTile("Reporte",
                tieneReporte ? "Enviado" : "Pendiente",
                tieneReporte ? ModernTheme.Colors.Success : ModernTheme.Colors.TextDim));

            // Lista mensajes
            var card = ModernTheme.CreateCard();
            card.Dock = DockStyle.Fill;

            var title = ModernTheme.CreateLabel("Actividad reciente", ModernTheme.LabelStyle.Subheading);
            title.Dock = DockStyle.Top;
            title.Margin = new Padding(0, 0, 0, 12);

            var list = MinimalUi.CreateListBox();
            list.Dock = DockStyle.Fill;
            var recientes = DataManager.ObtenerMensajesDeUsuario(uid).Take(12).ToList();
            if (recientes.Count == 0)
                list.Items.Add("No hay mensajes recientes.");
            else
                foreach (var m in recientes)
                    list.Items.Add($"{m.FechaEnvio:dd MMM · HH:mm}  —  {m.RemitenteNombre}: {m.Asunto}");

            card.Controls.Add(list);
            card.Controls.Add(title);

            root.Controls.Add(metrics, 0, 0);
            root.Controls.Add(card, 0, 1);
            Controls.Add(root);
        }
    }
}
