using System;
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
        private DataGridView _grid = null!;
        private DateTimePicker _dtpFecha = null!;
        private ComboBox? _cmbEmpleado;
        private Label? _lblEstado;

        public AsistenciaControl(bool modoAdmin)
        {
            _modoAdmin = modoAdmin;
            Dock = DockStyle.Fill;
            BuildUi();
            Cargar();
        }

        private void BuildUi()
        {
            var top = MinimalUi.CreateTopBar();

            MinimalUi.AddToBar(top, ModernTheme.CreateLabel("Fecha", ModernTheme.LabelStyle.Caption));

            _dtpFecha = new DateTimePicker { Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            ModernTheme.StyleDateTimePicker(_dtpFecha);
            _dtpFecha.ValueChanged += (_, _) => Cargar();
            MinimalUi.AddToBar(top, _dtpFecha);

            if (_modoAdmin)
            {
                MinimalUi.AddToBar(top, ModernTheme.CreateLabel("Empleado", ModernTheme.LabelStyle.Caption));
                _cmbEmpleado = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
                ModernTheme.StyleComboBox(_cmbEmpleado);
                _cmbEmpleado.Items.Add(new ComboItem("", "Todos"));
                foreach (var u in DataManager.ObtenerUsuarios().Where(u => u.Activo))
                    _cmbEmpleado.Items.Add(new ComboItem(u.Id, u.NombreCompleto));
                _cmbEmpleado.SelectedIndex = 0;
                _cmbEmpleado.SelectedIndexChanged += (_, _) => Cargar();
                MinimalUi.AddToBar(top, _cmbEmpleado);
            }
            else
            {
                var reg = DataManager.ObtenerRegistroAbierto(Session.UsuarioActual!.Id);
                _lblEstado = ModernTheme.CreateLabel(
                    reg != null ? $"Sesion activa · entrada {reg.HoraEntrada:HH:mm}" : "Sin entrada hoy",
                    ModernTheme.LabelStyle.Caption);
                MinimalUi.AddToBar(top, _lblEstado);
            }

            var btnHoy = MinimalUi.CreateButton("Hoy");
            btnHoy.Click += (_, _) => { _dtpFecha.Value = DateTime.Today; Cargar(); };
            MinimalUi.AddToBar(top, btnHoy);

            _grid = MinimalUi.CreateGrid();
            if (_modoAdmin)
                _grid.Columns.Add("Empleado", "Empleado");
            _grid.Columns.Add("Fecha", "Fecha");
            _grid.Columns.Add("Entrada", "Entrada");
            _grid.Columns.Add("Salida", "Salida");
            _grid.Columns.Add("Tiempo", "Tiempo");
            _grid.Columns.Add("Estado", "Estado");

            Controls.Add(_grid);
            Controls.Add(top);
        }

        private void Cargar()
        {
            _grid.Rows.Clear();
            var registros = DataManager.ObtenerAsistencia()
                .Where(r => r.HoraEntrada.Date == _dtpFecha.Value.Date)
                .ToList();

            if (_modoAdmin)
            {
                var selId = (_cmbEmpleado?.SelectedItem as ComboItem)?.Id ?? "";
                if (!string.IsNullOrEmpty(selId))
                    registros = registros.Where(r => r.UsuarioId == selId).ToList();
            }
            else
                registros = registros.Where(r => r.UsuarioId == Session.UsuarioActual!.Id).ToList();

            var usuarios = DataManager.ObtenerUsuarios();
            foreach (var r in registros.OrderByDescending(x => x.HoraEntrada))
            {
                var u = usuarios.FirstOrDefault(x => x.Id == r.UsuarioId);
                string salida = r.HoraSalida.HasValue ? r.HoraSalida.Value.ToString("HH:mm") : "-";
                string estado = r.HoraSalida.HasValue ? "Completo" : "Activo";
                if (_modoAdmin)
                    _grid.Rows.Add(u?.NombreCompleto ?? r.UsuarioId, r.Fecha, r.HoraEntrada.ToString("HH:mm"), salida, r.TiempoTrabajado, estado);
                else
                    _grid.Rows.Add(r.Fecha, r.HoraEntrada.ToString("HH:mm"), salida, r.TiempoTrabajado, estado);
            }

            if (_grid.Rows.Count == 0)
            {
                if (_modoAdmin)
                    _grid.Rows.Add("Sin registros", "", "", "", "", "");
                else
                    _grid.Rows.Add("Sin registros", "", "", "", "");
            }
        }
    }
}
