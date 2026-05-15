using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class NotificacionesControl : UserControl
    {
        private Panel panelLista = null!;

        public NotificacionesControl()
        {
            this.BackColor = Colores.Fondo;
            InitUI();
            Cargar();
        }

        private void InitUI()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            var btnNueva = new Button
            {
                Text = "🚨  Enviar Alerta",
                Location = new Point(10, 10),
                Size = new Size(155, 36),
                BackColor = Colores.Alerta,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNueva.FlatAppearance.BorderSize = 0;
            btnNueva.Click += (s, e) =>
            {
                var form = new EnviarAlertaForm();
                form.ShowDialog();
                Cargar();
            };

            toolbar.Controls.Add(btnNueva);

            panelLista = new Panel { Dock = DockStyle.Fill, BackColor = Colores.Fondo, AutoScroll = true, Padding = new Padding(0, 10, 0, 10) };

            this.Controls.Add(panelLista);
            this.Controls.Add(toolbar);
        }

        private void Cargar()
        {
            panelLista.Controls.Clear();
            var uid = Session.UsuarioActual?.Id ?? "";
            List<Notificacion> lista;

            if (Session.EsAdmin)
                lista = DataManager.ObtenerNotificacionesParaAdmin();
            else
                lista = DataManager.ObtenerNotificaciones()
                    .Where(n => n.DestinatarioId == uid || string.IsNullOrEmpty(n.DestinatarioId))
                    .OrderByDescending(n => n.Fecha).ToList();

            if (lista.Count == 0)
            {
                panelLista.Controls.Add(new Label { Text = "No hay notificaciones.", Font = new Font("Segoe UI", 11), ForeColor = Colores.TextoSecundario, Location = new Point(20, 20), AutoSize = true });
                return;
            }

            int y = 0;
            foreach (var n in lista)
            {
                var card = CrearCard(n, y);
                panelLista.Controls.Add(card);
                y += 82;
            }
        }

        private Panel CrearCard(Notificacion n, int y)
        {
            Color colorTipo = n.Tipo == "Urgente" ? Colores.Alerta : n.Tipo == "Alerta" ? Colores.Advertencia : Colores.Secundario;
            string icono = n.Tipo == "Urgente" ? "🆘" : n.Tipo == "Alerta" ? "⚠️" : "ℹ️";

            var card = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(this.Width - 20, 78),
                BackColor = n.Leida ? Color.White : Color.FromArgb(255, 247, 237),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(colorTipo), 0, 0, 5, card.Height);
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, card.Height - 1, card.Width, card.Height - 1);
            };

            var lblBadge = new Label
            {
                Text = $"{icono} {n.Tipo.ToUpper()}",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = colorTipo,
                Location = new Point(15, 8),
                AutoSize = true
            };
            var lblDe = new Label
            {
                Text = $"De: {n.RemitenteNombre}  |  {n.RemitenteDepartamento}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Colores.TextoPrimario,
                Location = new Point(15, 26),
                AutoSize = true
            };
            var lblMsg = new Label
            {
                Text = n.Mensaje.Length > 90 ? n.Mensaje.Substring(0, 87) + "..." : n.Mensaje,
                Font = new Font("Segoe UI", 9),
                ForeColor = Colores.TextoSecundario,
                Location = new Point(15, 47),
                AutoSize = true
            };
            var lblFecha = new Label
            {
                Text = n.Fecha.ToString("dd/MM/yy HH:mm"),
                Font = new Font("Segoe UI", 8),
                ForeColor = Colores.TextoSecundario,
                Location = new Point(card.Width - 120, 8),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            card.Controls.AddRange(new Control[] { lblBadge, lblDe, lblMsg, lblFecha });
            card.Click += (s, e) =>
            {
                DataManager.MarcarNotificacionLeida(n.Id);
                MessageBox.Show($"📣 {icono} {n.Tipo}\n\nDe: {n.RemitenteNombre} ({n.RemitenteDepartamento})\n{n.Fecha:dd/MM/yyyy HH:mm}\n\n{n.Mensaje}", "Notificación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cargar();
            };

            return card;
        }
    }

    public class EnviarAlertaForm : Form
    {
        private RichTextBox txtMensaje = null!;
        private ComboBox cmbTipo = null!;

        public EnviarAlertaForm()
        {
            this.Text = "Enviar Alerta";
            this.Size = new Size(480, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Colores.Fondo;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(185, 28, 28) };
            new Label { Text = "🚨  Enviar Notificación de Alerta", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 14), AutoSize = true }.Parent = pnlHeader;

            var lbl1 = new Label { Text = "Nivel de Alerta:", Location = new Point(20, 75), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbTipo = new ComboBox { Location = new Point(20, 97), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbTipo.Items.AddRange(new object[] { "Info", "Alerta", "Urgente" });
            cmbTipo.SelectedIndex = 1;

            var lbl2 = new Label { Text = "Mensaje de Alerta:", Location = new Point(20, 145), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtMensaje = new RichTextBox { Location = new Point(20, 167), Size = new Size(435, 110), Font = new Font("Segoe UI", 10.5f), BorderStyle = BorderStyle.FixedSingle, ScrollBars = RichTextBoxScrollBars.Vertical };

            var lblInfo = new Label
            {
                Text = "⚠️ Esta alerta será enviada a todos los administradores.",
                Location = new Point(20, 285),
                Size = new Size(440, 22),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Colores.Advertencia
            };

            var btnEnviar = new Button { Text = "🚨  Enviar Alerta", Location = new Point(20, 310), Size = new Size(150, 40), BackColor = Colores.Alerta, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtMensaje.Text)) { MessageBox.Show("Escriba un mensaje.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var n = new Notificacion
                {
                    RemitenteId = Session.UsuarioActual!.Id,
                    RemitenteNombre = Session.UsuarioActual.NombreCompleto,
                    RemitenteDepartamento = Session.UsuarioActual.Departamento,
                    Mensaje = txtMensaje.Text.Trim(),
                    Tipo = cmbTipo.SelectedItem?.ToString() ?? "Alerta"
                };
                DataManager.AgregarNotificacion(n);
                MessageBox.Show("✅ Alerta enviada a los administradores.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };

            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(185, 310), Size = new Size(100, 40), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand };
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { pnlHeader, lbl1, cmbTipo, lbl2, txtMensaje, lblInfo, btnEnviar, btnCancelar });
        }
    }
}
