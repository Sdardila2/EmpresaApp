using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class ReporteControl : UserControl
    {
        public ReporteControl()
        {
            this.BackColor = Colores.Fondo;
            var uid = Session.UsuarioActual?.Id ?? "";
            if (DataManager.TieneReporteHoy(uid))
                MostrarReporteEnviado();
            else
                MostrarFormulario();
        }

        private void MostrarReporteEnviado()
        {
            var reporte = DataManager.ObtenerReportes()
                .LastOrDefault(r => r.UsuarioId == Session.UsuarioActual!.Id && r.Fecha.Date == DateTime.Today);

            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Colores.Fondo, Padding = new Padding(20) };

            var card = new Panel { Location = new Point(20, 20), Size = new Size(700, 380), BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(Colores.Acento), 0, 0, card.Width, 6);
            };

            var lblTit = new Label { Text = "✅ Reporte del día enviado", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Colores.Acento, Location = new Point(25, 25), AutoSize = true };
            var lblFecha = new Label { Text = $"📅 {(reporte?.Fecha ?? DateTime.Today):dddd, dd 'de' MMMM 'de' yyyy}", Font = new Font("Segoe UI", 10), ForeColor = Colores.TextoSecundario, Location = new Point(25, 60), AutoSize = true };

            int y = 95;
            void SeccionDetalle(string titulo, string contenido)
            {
                card.Controls.Add(new Label { Text = titulo, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario, Location = new Point(25, y), AutoSize = true });
                card.Controls.Add(new Label { Text = contenido, Font = new Font("Segoe UI", 10), ForeColor = Colores.TextoPrimario, Location = new Point(25, y + 20), Size = new Size(640, 35), AutoEllipsis = true });
                y += 65;
            }

            if (reporte != null)
            {
                SeccionDetalle("Actividades realizadas:", reporte.ActividadesRealizadas);
                SeccionDetalle("Logros del día:", reporte.LogrosDelDia);
                SeccionDetalle("Pendientes:", reporte.Pendientes);
                if (!string.IsNullOrEmpty(reporte.Observaciones))
                    SeccionDetalle("Observaciones:", reporte.Observaciones);

                var estrellas = new string('⭐', reporte.NivelProductividad);
                card.Controls.Add(new Label { Text = $"Productividad: {estrellas}", Font = new Font("Segoe UI Emoji", 12), ForeColor = Colores.Advertencia, Location = new Point(25, y), AutoSize = true });
            }

            card.Controls.AddRange(new Control[] { lblTit, lblFecha });
            pnl.Controls.Add(card);
            this.Controls.Add(pnl);
        }

        private void MostrarFormulario()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Colores.Fondo };
            var pnlForm = new Panel { Location = new Point(10, 10), Size = new Size(760, 620), BackColor = Color.White };
            pnlForm.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, pnlForm.Width - 1, pnlForm.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(Colores.Secundario), 0, 0, pnlForm.Width, 6);
            };

            var lblTit = new Label { Text = "📊 Reporte Final del Día", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Colores.TextoPrimario, Location = new Point(25, 25), AutoSize = true };
            var lblSub = new Label { Text = $"📅 {DateTime.Now:dddd, dd 'de' MMMM 'de' yyyy}  |  {Session.UsuarioActual?.NombreCompleto} - {Session.UsuarioActual?.Departamento}", Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(25, 60), AutoSize = true };

            int y = 90;
            RichTextBox Campo(string label, string placeholder, int height = 70)
            {
                pnlForm.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario, Location = new Point(25, y), AutoSize = true });
                var rtb = new RichTextBox { Location = new Point(25, y + 22), Size = new Size(710, height), Font = new Font("Segoe UI", 10.5f), BorderStyle = BorderStyle.FixedSingle, ScrollBars = RichTextBoxScrollBars.Vertical };
                pnlForm.Controls.Add(rtb);
                y += height + 40;
                return rtb;
            }

            var txtActividades = Campo("📌 ¿Qué actividades realizaste hoy? *", "", 80);
            var txtLogros = Campo("🏆 ¿Cuáles fueron tus logros del día? *", "", 70);
            var txtPendientes = Campo("📋 ¿Qué quedó pendiente para mañana?", "", 60);
            var txtObservaciones = Campo("💬 Observaciones adicionales", "", 60);

            // Nivel productividad
            pnlForm.Controls.Add(new Label { Text = "⭐ Nivel de productividad del día (1-5):", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario, Location = new Point(25, y), AutoSize = true });
            var track = new TrackBar { Location = new Point(25, y + 22), Size = new Size(350, 40), Minimum = 1, Maximum = 5, Value = 3, TickFrequency = 1 };
            var lblTrackVal = new Label { Text = "⭐⭐⭐", Font = new Font("Segoe UI Emoji", 13), ForeColor = Colores.Advertencia, Location = new Point(390, y + 22), AutoSize = true };
            track.ValueChanged += (s, e) => lblTrackVal.Text = new string('⭐', track.Value);
            pnlForm.Controls.Add(track);
            pnlForm.Controls.Add(lblTrackVal);
            y += 70;

            var btnEnviar = new Button
            {
                Text = "📤  Enviar Reporte",
                Location = new Point(25, y + 15),
                Size = new Size(180, 46),
                BackColor = Colores.Acento,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtActividades.Text) || string.IsNullOrWhiteSpace(txtLogros.Text))
                {
                    MessageBox.Show("Complete los campos obligatorios (*).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var reporte = new ReporteDiario
                {
                    UsuarioId = Session.UsuarioActual!.Id,
                    UsuarioNombre = Session.UsuarioActual.NombreCompleto,
                    Departamento = Session.UsuarioActual.Departamento,
                    ActividadesRealizadas = txtActividades.Text.Trim(),
                    LogrosDelDia = txtLogros.Text.Trim(),
                    Pendientes = txtPendientes.Text.Trim(),
                    Observaciones = txtObservaciones.Text.Trim(),
                    NivelProductividad = track.Value
                };
                DataManager.AgregarReporte(reporte);
                MessageBox.Show("✅ Reporte enviado correctamente.\n¡Buen trabajo hoy!", "Reporte Enviado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Controls.Clear();
                MostrarReporteEnviado();
            };

            pnlForm.Controls.AddRange(new Control[] { lblTit, lblSub, btnEnviar });
            pnlForm.Height = y + 80;
            scroll.Controls.Add(pnlForm);
            this.Controls.Add(scroll);
        }
    }

    public class TodosReportesControl : UserControl
    {
        private DataGridView dgv = null!;
        private DateTimePicker dtpFecha = null!;

        public TodosReportesControl()
        {
            this.BackColor = Colores.Fondo;
            InitUI();
            Cargar();
        }

        private void InitUI()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            new Label { Text = "Filtrar por fecha:", Location = new Point(15, 18), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario }.Parent = toolbar;
            dtpFecha = new DateTimePicker { Location = new Point(130, 14), Size = new Size(180, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            dtpFecha.ValueChanged += (s, e) => Cargar();

            var btnTodos = new Button { Text = "Ver todos", Location = new Point(325, 14), Size = new Size(90, 28), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand };
            btnTodos.Click += (s, e) => { dtpFecha.Value = DateTime.MinValue.AddYears(1900); Cargar(); };

            toolbar.Controls.AddRange(new Control[] { dtpFecha, btnTodos });

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
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 36 }
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colores.Primario;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Empleado", HeaderText = "Empleado", FillWeight = 180 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Depto", HeaderText = "Departamento", FillWeight = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha", FillWeight = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Actividades", HeaderText = "Actividades", FillWeight = 250 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Logros", HeaderText = "Logros", FillWeight = 200 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Prod", HeaderText = "★ Prod.", FillWeight = 70 });

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
        }

        private void Cargar()
        {
            dgv.Rows.Clear();
            var reportes = DataManager.ObtenerReportes()
                .Where(r => dtpFecha.Value.Year > 1901 ? r.Fecha.Date == dtpFecha.Value.Date : true)
                .OrderByDescending(r => r.Fecha).ToList();
            foreach (var r in reportes)
            {
                dgv.Rows.Add(r.UsuarioNombre, r.Departamento, r.Fecha.ToString("dd/MM/yyyy"),
                    r.ActividadesRealizadas.Length > 60 ? r.ActividadesRealizadas.Substring(0, 57) + "..." : r.ActividadesRealizadas,
                    r.LogrosDelDia.Length > 50 ? r.LogrosDelDia.Substring(0, 47) + "..." : r.LogrosDelDia,
                    new string('⭐', r.NivelProductividad));
            }
        }
    }
}
