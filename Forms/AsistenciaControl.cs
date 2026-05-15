using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class AsistenciaControl : UserControl
    {
        private readonly bool _modoAdmin;
        private DataGridView dgv = null!;
        private DateTimePicker dtpFiltro = null!;
        private ComboBox cmbEmpleado = null!;

        public AsistenciaControl(bool modoAdmin)
        {
            _modoAdmin = modoAdmin;
            this.BackColor = Colores.Fondo;
            InitUI();
            Cargar();
        }

        private void InitUI()
        {
            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            int tx = 15;
            new Label { Text = "Fecha:", Location = new Point(tx, 20), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario }.Parent = toolbar;
            dtpFiltro = new DateTimePicker { Location = new Point(tx + 50, 16), Size = new Size(155, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            dtpFiltro.ValueChanged += (s, e) => Cargar();
            toolbar.Controls.Add(dtpFiltro);

            if (_modoAdmin)
            {
                new Label { Text = "Empleado:", Location = new Point(tx + 225, 20), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario }.Parent = toolbar;
                cmbEmpleado = new ComboBox { Location = new Point(tx + 295, 16), Size = new Size(240, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
                cmbEmpleado.Items.Add(new ComboItem("", "— Todos los empleados —"));
                foreach (var u in DataManager.ObtenerUsuarios().Where(u => u.Activo))
                    cmbEmpleado.Items.Add(new ComboItem(u.Id, u.NombreCompleto));
                cmbEmpleado.SelectedIndex = 0;
                cmbEmpleado.SelectedIndexChanged += (s, e) => Cargar();
                toolbar.Controls.Add(cmbEmpleado);
            }

            var btnHoy = new Button { Text = "Hoy", Location = new Point(_modoAdmin ? 555 : tx + 225, 16), Size = new Size(60, 28), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand };
            btnHoy.Click += (s, e) => { dtpFiltro.Value = DateTime.Today; Cargar(); };
            toolbar.Controls.Add(btnHoy);

            // Tarjeta estado (solo para empleado propio)
            if (!_modoAdmin)
            {
                var pnlEstado = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Colores.Fondo, Padding = new Padding(10, 10, 10, 5) };
                var registroHoy = DataManager.ObtenerRegistroAbierto(Session.UsuarioActual!.Id);
                bool activo = registroHoy != null;

                var card = new Panel { Location = new Point(10, 10), Size = new Size(360, 58), BackColor = Color.White };
                card.Paint += (s, e) =>
                {
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, card.Width - 1, card.Height - 1);
                    e.Graphics.FillRectangle(new SolidBrush(activo ? Colores.Acento : Colores.Advertencia), 0, 0, 5, card.Height);
                };
                var lblEstado = new Label { Text = activo ? "🟢  Sesión activa" : "🔴  Sin registro de entrada hoy", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = activo ? Colores.Acento : Colores.Advertencia, Location = new Point(15, 8), AutoSize = true };
                var lblHora = new Label { Text = activo ? $"Entrada: {registroHoy!.HoraEntrada:HH:mm}" : "No has registrado entrada hoy.", Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(15, 32), AutoSize = true };
                card.Controls.AddRange(new Control[] { lblEstado, lblHora });
                pnlEstado.Controls.Add(card);
                this.Controls.Add(pnlEstado);
            }

            // Grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 42,
                RowTemplate = { Height = 38 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colores.Primario;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Colores.TextoPrimario;

            if (_modoAdmin)
                dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Empleado", HeaderText = "Empleado", FillWeight = 200 });

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha", FillWeight = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Entrada", HeaderText = "🕐 Entrada", FillWeight = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Salida", HeaderText = "🕐 Salida", FillWeight = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tiempo", HeaderText = "⏱ Tiempo trabajado", FillWeight = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", FillWeight = 90 });

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
        }

        private void Cargar()
        {
            dgv.Rows.Clear();
            var registros = DataManager.ObtenerAsistencia()
                .Where(r => r.HoraEntrada.Date == dtpFiltro.Value.Date)
                .ToList();

            if (_modoAdmin)
            {
                var selId = (cmbEmpleado?.SelectedItem as ComboItem)?.Id ?? "";
                if (!string.IsNullOrEmpty(selId))
                    registros = registros.Where(r => r.UsuarioId == selId).ToList();
            }
            else
            {
                registros = registros.Where(r => r.UsuarioId == Session.UsuarioActual!.Id).ToList();
            }

            var usuarios = DataManager.ObtenerUsuarios();

            foreach (var r in registros.OrderByDescending(x => x.HoraEntrada))
            {
                var u = usuarios.FirstOrDefault(x => x.Id == r.UsuarioId);
                string estado = r.HoraSalida.HasValue ? "✅ Completo" : "🟢 Activo";
                string salida = r.HoraSalida.HasValue ? r.HoraSalida.Value.ToString("HH:mm") : "—";

                if (_modoAdmin)
                    dgv.Rows.Add(u?.NombreCompleto ?? r.UsuarioId, r.HoraEntrada.ToString("dd/MM/yyyy"), r.HoraEntrada.ToString("HH:mm"), salida, r.TiempoTrabajado, estado);
                else
                    dgv.Rows.Add(r.HoraEntrada.ToString("dd/MM/yyyy"), r.HoraEntrada.ToString("HH:mm"), salida, r.TiempoTrabajado, estado);

                // Color fila activa
                if (!r.HoraSalida.HasValue)
                    dgv.Rows[dgv.Rows.Count - 1].DefaultCellStyle.ForeColor = Colores.Acento;
            }

            if (dgv.Rows.Count == 0)
            {
                var msg = _modoAdmin ? "No hay registros de asistencia para este día." : "No tienes registros para este día.";
                dgv.Rows.Add(_modoAdmin ?
                    new object[] { msg, "", "", "", "", "" } :
                    new object[] { msg, "", "", "", "" });
            }
        }
    }
}
