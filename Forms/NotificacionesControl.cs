using System;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class NotificacionesControl : UserControl
    {
        private ListBox _lista = null!;

        public NotificacionesControl()
        {
            Dock = DockStyle.Fill;
            BuildUi();
            Cargar();
        }

        private void BuildUi()
        {
            var top = MinimalUi.CreateTopBar();
            var btn = MinimalUi.CreateButton("Enviar alerta", primary: true);
            btn.Click += (_, _) => { new EnviarAlertaForm().ShowDialog(); Cargar(); };
            MinimalUi.AddToBar(top, btn);

            _lista = MinimalUi.CreateListBox();
            _lista.Dock = DockStyle.Fill;
            _lista.DoubleClick += (_, _) => VerSeleccion();

            Controls.Add(_lista);
            Controls.Add(top);
        }

        private void Cargar()
        {
            _lista.Items.Clear();
            var uid = Session.UsuarioActual?.Id ?? "";
            var lista = Session.EsAdmin
                ? DataManager.ObtenerNotificacionesParaAdmin()
                : DataManager.ObtenerNotificaciones()
                    .Where(n => n.DestinatarioId == uid || string.IsNullOrEmpty(n.DestinatarioId))
                    .OrderByDescending(n => n.Fecha)
                    .ToList();

            foreach (var n in lista)
                _lista.Items.Add(new NotifItem(n));
            if (_lista.Items.Count == 0)
                _lista.Items.Add("(sin notificaciones)");
        }

        private void VerSeleccion()
        {
            if (_lista.SelectedItem is not NotifItem item) return;
            DataManager.MarcarNotificacionLeida(item.N.Id);
            MessageBox.Show(
                $"{item.N.Tipo}\n{item.N.RemitenteNombre}\n{item.N.Fecha:dd/MM/yyyy HH:mm}\n\n{item.N.Mensaje}",
                "Notificacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Cargar();
        }

        private sealed class NotifItem
        {
            public Notificacion N { get; }
            public NotifItem(Notificacion n) => N = n;
            public override string ToString() =>
                $"{N.Fecha:dd/MM HH:mm} [{N.Tipo}] {N.RemitenteNombre}: {(N.Mensaje.Length > 50 ? N.Mensaje[..47] + "..." : N.Mensaje)}";
        }
    }

    public class EnviarAlertaForm : Form
    {
        public EnviarAlertaForm()
        {
            Text = "Enviar alerta";
            Size = new System.Drawing.Size(420, 280);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ModernTheme.ApplyToForm(this);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 16, 20, 8),
                ColumnCount = 2,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(ModernTheme.CreateLabel("Tipo", ModernTheme.LabelStyle.Caption), 0, 0);
            var cmb = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(cmb);
            cmb.Items.AddRange(new object[] { "Info", "Alerta", "Urgente" });
            cmb.SelectedIndex = 1;
            layout.Controls.Add(cmb, 1, 0);

            layout.Controls.Add(ModernTheme.CreateLabel("Texto", ModernTheme.LabelStyle.Caption), 0, 1);
            var txt = MinimalUi.CreateTextBox(multiline: true);
            txt.Dock = DockStyle.Fill;
            layout.Controls.Add(txt, 1, 1);

            var footer = UiLayout.CreateFooterBar();
            var btnOk = MinimalUi.CreateButton("Enviar", primary: true);
            var btnNo = MinimalUi.CreateButton("Cancelar");
            btnNo.Click += (_, _) => Close();
            btnOk.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    MessageBox.Show("Escriba un mensaje.");
                    return;
                }
                DataManager.AgregarNotificacion(new Notificacion
                {
                    RemitenteId = Session.UsuarioActual!.Id,
                    RemitenteNombre = Session.UsuarioActual.NombreCompleto,
                    RemitenteDepartamento = Session.UsuarioActual.Departamento,
                    Mensaje = txt.Text.Trim(),
                    Tipo = cmb.SelectedItem?.ToString() ?? "Alerta"
                });
                DialogResult = DialogResult.OK;
                Close();
            };
            UiLayout.AddFooterButton(footer, btnNo);
            UiLayout.AddFooterButton(footer, btnOk);

            Controls.Add(footer);
            Controls.Add(layout);
        }
    }
}
