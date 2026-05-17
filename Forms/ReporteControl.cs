using System;
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
            Dock = DockStyle.Fill;
            var uid = Session.UsuarioActual?.Id ?? "";
            if (DataManager.TieneReporteHoy(uid))
                MostrarEnviado();
            else
                MostrarFormulario();
        }

        private void MostrarEnviado()
        {
            var r = DataManager.ObtenerReportes()
                .LastOrDefault(x => x.UsuarioId == Session.UsuarioActual!.Id && x.Fecha.Date == DateTime.Today);

            var txt = MinimalUi.CreateTextBox(multiline: true);
            txt.ReadOnly = true;
            txt.Dock = DockStyle.Fill;
            txt.Font = new System.Drawing.Font("Consolas", 9.5f);
            txt.Text = r == null ? "Reporte enviado hoy." :
                    $"Fecha: {r.Fecha:yyyy-MM-dd}\r\n\r\nActividades:\r\n{r.ActividadesRealizadas}\r\n\r\n" +
                    $"Logros:\r\n{r.LogrosDelDia}\r\n\r\nPendientes:\r\n{r.Pendientes}\r\n\r\n" +
                    $"Observaciones:\r\n{r.Observaciones}\r\n\r\nProductividad: {r.NivelProductividad}/5";
            Controls.Add(txt);
        }

        private void MostrarFormulario()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 8),
                ColumnCount = 1,
                RowCount = 6
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            var txtAct = MinimalUi.CreateTextBox(multiline: true);
            var txtLog = MinimalUi.CreateTextBox(multiline: true);
            var txtPen = MinimalUi.CreateTextBox(multiline: true);
            var txtObs = MinimalUi.CreateTextBox(multiline: true);

            layout.Controls.Add(Wrap("Actividades", txtAct), 0, 0);
            layout.Controls.Add(Wrap("Logros", txtLog), 0, 1);
            layout.Controls.Add(Wrap("Pendientes", txtPen), 0, 2);
            layout.Controls.Add(Wrap("Observaciones", txtObs), 0, 3);

            var pProd = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };
            var lblProd = ModernTheme.CreateLabel("Productividad (1-5)", ModernTheme.LabelStyle.Caption);
            lblProd.Margin = new Padding(0, 10, 12, 0);
            pProd.Controls.Add(lblProd);
            var num = new NumericUpDown { Minimum = 1, Maximum = 5, Value = 3, Width = 56, Height = 28 };
            num.Margin = new Padding(0, 8, 0, 0);
            pProd.Controls.Add(num);
            layout.Controls.Add(pProd, 0, 4);

            var btn = MinimalUi.CreateButton("Enviar reporte", primary: true);
            btn.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtAct.Text) || string.IsNullOrWhiteSpace(txtLog.Text))
                {
                    MessageBox.Show("Complete actividades y logros.");
                    return;
                }
                DataManager.AgregarReporte(new ReporteDiario
                {
                    UsuarioId = Session.UsuarioActual!.Id,
                    UsuarioNombre = Session.UsuarioActual.NombreCompleto,
                    Departamento = Session.UsuarioActual.Departamento,
                    ActividadesRealizadas = txtAct.Text.Trim(),
                    LogrosDelDia = txtLog.Text.Trim(),
                    Pendientes = txtPen.Text.Trim(),
                    Observaciones = txtObs.Text.Trim(),
                    NivelProductividad = (int)num.Value
                });
                Controls.Clear();
                MostrarEnviado();
            };
            layout.Controls.Add(UiLayout.CreateButtonRow(btn), 0, 5);
            Controls.Add(layout);
        }

        private static Panel Wrap(string titulo, Control input)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = ModernTheme.Colors.Bg, Padding = new Padding(0, 0, 0, 8) };
            var lbl = ModernTheme.CreateLabel(titulo, ModernTheme.LabelStyle.Caption);
            lbl.Dock = DockStyle.Top;
            lbl.Height = 22;
            input.Dock = DockStyle.Fill;
            p.Controls.Add(input);
            p.Controls.Add(lbl);
            return p;
        }
    }

    public class TodosReportesControl : UserControl
    {
        private DataGridView _grid = null!;
        private DateTimePicker _fecha = null!;

        public TodosReportesControl()
        {
            Dock = DockStyle.Fill;
            BuildUi();
            Cargar();
        }

        private void BuildUi()
        {
            var top = MinimalUi.CreateTopBar();
            MinimalUi.AddToBar(top, ModernTheme.CreateLabel("Fecha", ModernTheme.LabelStyle.Caption));
            _fecha = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            ModernTheme.StyleDateTimePicker(_fecha);
            _fecha.ValueChanged += (_, _) => Cargar();
            MinimalUi.AddToBar(top, _fecha);
            var btn = MinimalUi.CreateButton("Ver todos");
            btn.Click += (_, _) => { _fecha.Value = DateTime.MinValue.AddYears(1900); Cargar(); };
            MinimalUi.AddToBar(top, btn);

            _grid = MinimalUi.CreateGrid();
            _grid.Columns.Add("Empleado", "Empleado");
            _grid.Columns.Add("Depto", "Depto");
            _grid.Columns.Add("Fecha", "Fecha");
            _grid.Columns.Add("Actividades", "Actividades");
            _grid.Columns.Add("Prod", "Prod");

            Controls.Add(_grid);
            Controls.Add(top);
        }

        private void Cargar()
        {
            _grid.Rows.Clear();
            bool todos = _fecha.Value.Year <= 1901;
            foreach (var r in DataManager.ObtenerReportes()
                .Where(x => todos || x.Fecha.Date == _fecha.Value.Date)
                .OrderByDescending(x => x.Fecha))
            {
                string act = r.ActividadesRealizadas.Length > 60
                    ? r.ActividadesRealizadas[..57] + "..."
                    : r.ActividadesRealizadas;
                _grid.Rows.Add(r.UsuarioNombre, r.Departamento, r.Fecha.ToString("dd/MM/yyyy"), act, r.NivelProductividad);
            }
        }
    }
}
