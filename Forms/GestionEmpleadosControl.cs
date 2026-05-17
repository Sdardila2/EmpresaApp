using System;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class GestionEmpleadosControl : UserControl
    {
        private ListBox _lista = null!;
        private TextBox _txtBuscar = null!;

        public GestionEmpleadosControl()
        {
            Dock = DockStyle.Fill;
            BuildUi();
            Cargar();
        }

        private void BuildUi()
        {
            var top = MinimalUi.CreateTopBar();

            var btnNuevo = MinimalUi.CreateButton("Nuevo");
            btnNuevo.Click += (_, _) => AbrirForm(null);
            MinimalUi.AddToBar(top, btnNuevo);

            var btnDept = MinimalUi.CreateButton("Departamentos");
            btnDept.Click += (_, _) => { new GestionDepartamentosForm().ShowDialog(); Cargar(); };
            MinimalUi.AddToBar(top, btnDept);

            var btnEdit = MinimalUi.CreateButton("Editar");
            btnEdit.Click += (_, _) => EditarSeleccion();
            MinimalUi.AddToBar(top, btnEdit);

            _txtBuscar = MinimalUi.CreateTextBox();
            _txtBuscar.Width = 200;
            _txtBuscar.PlaceholderText = "Buscar...";
            _txtBuscar.TextChanged += (_, _) => Cargar();
            MinimalUi.AddToBar(top, _txtBuscar);

            _lista = MinimalUi.CreateListBox();
            _lista.Dock = DockStyle.Fill;
            _lista.DoubleClick += (_, _) => EditarSeleccion();

            Controls.Add(_lista);
            Controls.Add(top);
        }

        private void Cargar()
        {
            _lista.Items.Clear();
            var q = _txtBuscar.Text.Trim().ToLower();
            foreach (var u in DataManager.ObtenerUsuarios()
                .Where(u => string.IsNullOrEmpty(q) ||
                    u.NombreCompleto.ToLower().Contains(q) ||
                    u.Usuario_Login.ToLower().Contains(q) ||
                    u.Email.ToLower().Contains(q))
                .OrderBy(u => u.NombreCompleto))
                _lista.Items.Add(new UsuarioItem(u));
        }

        private void EditarSeleccion()
        {
            if (_lista.SelectedItem is UsuarioItem item)
                AbrirForm(item.U);
        }

        private void AbrirForm(Usuario? u)
        {
            new EmpleadoForm(u).ShowDialog();
            Cargar();
        }

        private sealed class UsuarioItem
        {
            public Usuario U { get; }
            public UsuarioItem(Usuario u) => U = u;
            public override string ToString() =>
                $"{U.NombreCompleto} | {U.Usuario_Login} | {U.Departamento} | {U.Rol} | {(U.Activo ? "activo" : "inactivo")}";
        }
    }

    public class GestionDepartamentosForm : Form
    {
        private ListBox _lista = null!;
        private TextBox _txtNuevo = null!;

        public GestionDepartamentosForm()
        {
            Text = "Departamentos";
            Size = new System.Drawing.Size(420, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ModernTheme.ApplyToForm(this);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));

            _lista = MinimalUi.CreateListBox();
            _lista.Dock = DockStyle.Fill;
            layout.SetColumnSpan(_lista, 2);
            layout.Controls.Add(_lista, 0, 0);

            _txtNuevo = MinimalUi.CreateTextBox();
            _txtNuevo.Dock = DockStyle.Fill;
            _txtNuevo.PlaceholderText = "Nombre del departamento";
            layout.Controls.Add(_txtNuevo, 0, 1);

            var btnAdd = MinimalUi.CreateButton("Agregar", primary: true);
            btnAdd.Dock = DockStyle.Fill;
            btnAdd.Margin = new Padding(10, 0, 0, 0);
            UiLayout.SizeButton(btnAdd, 100);
            btnAdd.Click += (_, _) =>
            {
                var nombre = _txtNuevo.Text.Trim();
                if (string.IsNullOrEmpty(nombre)) return;
                try
                {
                    DataManager.AgregarDepartamento(new Departamento { Nombre = nombre });
                    _txtNuevo.Clear();
                    CargarLista();
                }
                catch
                {
                    MessageBox.Show("Ya existe ese departamento.");
                }
            };
            layout.Controls.Add(btnAdd, 1, 1);

            var btnDel = MinimalUi.CreateButton("Desactivar seleccionado");
            btnDel.Dock = DockStyle.Fill;
            layout.SetColumnSpan(btnDel, 2);
            btnDel.Click += (_, _) =>
            {
                if (_lista.SelectedItem is DeptItem d &&
                    MessageBox.Show("Desactivar " + d.Nombre + "?", "Confirmar",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DataManager.EliminarDepartamento(d.Id);
                    CargarLista();
                }
            };
            layout.Controls.Add(btnDel, 0, 2);

            Controls.Add(layout);
            CargarLista();
        }

        private void CargarLista()
        {
            _lista.Items.Clear();
            foreach (var d in DataManager.ObtenerDepartamentos())
                _lista.Items.Add(new DeptItem(d.Id, d.Nombre));
        }

        private sealed class DeptItem
        {
            public int Id { get; }
            public string Nombre { get; }
            public DeptItem(int id, string nombre) { Id = id; Nombre = nombre; }
            public override string ToString() => Nombre;
        }
    }

    public class EmpleadoForm : Form
    {
        private readonly Usuario? _existente;
        private TextBox _nombre = null!, _apellido = null!, _email = null!, _login = null!, _pass = null!, _cargo = null!;
        private ComboBox _depto = null!, _rol = null!;
        private CheckBox _activo = null!;

        public EmpleadoForm(Usuario? usuario)
        {
            _existente = usuario;
            Text = usuario == null ? "Nuevo empleado" : "Editar empleado";
            Size = new System.Drawing.Size(440, 480);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ModernTheme.ApplyToForm(this);
            BuildUi();
            if (usuario != null) CargarDatos(usuario);
        }

        private void BuildUi()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 16, 20, 8),
                ColumnCount = 2,
                RowCount = 9
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 9; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            void Row(int row, string label, Control input)
            {
                var lbl = ModernTheme.CreateLabel(label, ModernTheme.LabelStyle.Caption);
                lbl.Anchor = AnchorStyles.Left;
                lbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                lbl.Dock = DockStyle.Fill;
                layout.Controls.Add(lbl, 0, row);
                input.Dock = DockStyle.Fill;
                input.Margin = new Padding(0, 4, 0, 4);
                layout.Controls.Add(input, 1, row);
            }

            _nombre = MinimalUi.CreateTextBox();
            _apellido = MinimalUi.CreateTextBox();
            _email = MinimalUi.CreateTextBox();
            _login = MinimalUi.CreateTextBox();
            _pass = MinimalUi.CreateTextBox(password: true);
            _cargo = MinimalUi.CreateTextBox();
            _depto = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_depto);
            _depto.Items.Add("(ninguno)");
            foreach (var d in DataManager.ObtenerDepartamentos()) _depto.Items.Add(d.Nombre);
            _depto.SelectedIndex = 0;
            _rol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_rol);
            _rol.Items.AddRange(new object[] { "Empleado", "Administrador" });
            _activo = new CheckBox
            {
                Text = "Activo",
                Checked = true,
                ForeColor = ModernTheme.Colors.Text,
                BackColor = ModernTheme.Colors.Bg,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            Row(0, "Nombre", _nombre);
            Row(1, "Apellido", _apellido);
            Row(2, "Email", _email);
            Row(3, "Usuario", _login);
            Row(4, "Contrasena", _pass);
            Row(5, "Departamento", _depto);
            Row(6, "Cargo", _cargo);
            Row(7, "Rol", _rol);
            layout.Controls.Add(_activo, 1, 8);

            var footer = UiLayout.CreateFooterBar();
            var btnOk = MinimalUi.CreateButton("Guardar", primary: true);
            var btnNo = MinimalUi.CreateButton("Cancelar");
            btnNo.Click += (_, _) => Close();
            btnOk.Click += (_, _) => Guardar();
            UiLayout.AddFooterButton(footer, btnNo);
            UiLayout.AddFooterButton(footer, btnOk);

            if (_existente != null && _existente.Id != Session.UsuarioActual?.Id)
            {
                var btnDel = MinimalUi.CreateButton("Desactivar");
                btnDel.Click += (_, _) =>
                {
                    if (MessageBox.Show("Desactivar cuenta?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        DataManager.EliminarUsuario(_existente.Id);
                        Close();
                    }
                };
                UiLayout.SizeButton(btnDel);
                btnDel.Margin = new Padding(0, 0, 0, 0);
                footer.Controls.Add(btnDel);
                footer.Controls.SetChildIndex(btnDel, footer.Controls.Count - 1);
            }

            Controls.Add(footer);
            Controls.Add(layout);
        }

        private void CargarDatos(Usuario u)
        {
            _nombre.Text = u.Nombre;
            _apellido.Text = u.Apellido;
            _email.Text = u.Email;
            _login.Text = u.Usuario_Login;
            _pass.Text = u.Password;
            _cargo.Text = u.Cargo;
            _rol.SelectedIndex = u.Rol == UserRole.Administrador ? 1 : 0;
            _activo.Checked = u.Activo;
            if (!string.IsNullOrEmpty(u.Departamento))
            {
                int i = _depto.FindStringExact(u.Departamento);
                if (i >= 0) _depto.SelectedIndex = i;
            }
        }

        private void Guardar()
        {
            if (string.IsNullOrWhiteSpace(_nombre.Text) || string.IsNullOrWhiteSpace(_login.Text) || string.IsNullOrWhiteSpace(_pass.Text))
            {
                MessageBox.Show("Complete nombre, usuario y contrasena.");
                return;
            }
            var todos = DataManager.ObtenerUsuarios();
            if (todos.Any(u => u.Usuario_Login.Equals(_login.Text.Trim(), StringComparison.OrdinalIgnoreCase) && u.Id != (_existente?.Id ?? "")))
            {
                MessageBox.Show("Usuario ya existe.");
                return;
            }
            string dept = _depto.SelectedIndex <= 0 ? "" : _depto.SelectedItem?.ToString() ?? "";
            if (_existente == null)
            {
                DataManager.AgregarUsuario(new Usuario
                {
                    Nombre = _nombre.Text.Trim(), Apellido = _apellido.Text.Trim(),
                    Email = _email.Text.Trim(), Usuario_Login = _login.Text.Trim(),
                    Password = _pass.Text, Departamento = dept, Cargo = _cargo.Text.Trim(),
                    Rol = _rol.SelectedIndex == 1 ? UserRole.Administrador : UserRole.Empleado,
                    Activo = _activo.Checked
                });
            }
            else
            {
                _existente.Nombre = _nombre.Text.Trim();
                _existente.Apellido = _apellido.Text.Trim();
                _existente.Email = _email.Text.Trim();
                _existente.Usuario_Login = _login.Text.Trim();
                _existente.Password = _pass.Text;
                _existente.Departamento = dept;
                _existente.Cargo = _cargo.Text.Trim();
                _existente.Rol = _rol.SelectedIndex == 1 ? UserRole.Administrador : UserRole.Empleado;
                _existente.Activo = _activo.Checked;
                DataManager.ActualizarUsuario(_existente);
            }
            Close();
        }
    }
}
