// ═══════════════════════════════════════════════════════════════
//  GestionEmpleadosControl.cs
//  — Empleados agrupados por departamento en el grid
//  — Departamento: ComboBox cargado desde tabla Departamentos
//  — Gestión de departamentos (crear / desactivar)
// ═══════════════════════════════════════════════════════════════
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class GestionEmpleadosControl : UserControl
    {
        private DataGridView dgv      = null!;
        private TextBox      txtBuscar = null!;

        public GestionEmpleadosControl()
        {
            this.BackColor = Colores.Fondo;
            InitUI();
            CargarEmpleados();
        }

        private void InitUI()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(
                new Pen(Color.FromArgb(226, 232, 240)), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            var btnNuevo = new Button
            {
                Text = "➕  Nuevo Empleado", Location = new Point(10, 12), Size = new Size(160, 36),
                BackColor = Colores.Acento, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += (s, e) => AbrirFormEmpleado(null);

            var btnDepts = new Button
            {
                Text = "🏢  Departamentos", Location = new Point(180, 12), Size = new Size(160, 36),
                BackColor = Colores.Primario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnDepts.FlatAppearance.BorderSize = 0;
            btnDepts.Click += (s, e) => { new GestionDepartamentosForm().ShowDialog(); CargarEmpleados(); };

            txtBuscar = new TextBox
            {
                Location = new Point(355, 14), Size = new Size(240, 32),
                Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "🔍 Buscar empleado..."
            };
            txtBuscar.TextChanged += (s, e) => CargarEmpleados();

            toolbar.Controls.AddRange(new Control[] { btnNuevo, btnDepts, txtBuscar });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, ScrollBars = ScrollBars.Both,
                Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 42, RowTemplate = { Height = 40 }
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colores.Primario;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Colores.TextoPrimario;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditarEmpleado(e.RowIndex); };

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",      Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre",  HeaderText = "Nombre Completo", FillWeight = 200 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Usuario", HeaderText = "Usuario",         FillWeight = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email",   HeaderText = "Email",           FillWeight = 200 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Depto",   HeaderText = "Departamento",    FillWeight = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cargo",   HeaderText = "Cargo",           FillWeight = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rol",     HeaderText = "Rol",             FillWeight = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado",  HeaderText = "Estado",          FillWeight = 80  });
            dgv.Columns.Add(new DataGridViewButtonColumn  { Name = "Editar",  HeaderText = "", Text = "✏️ Editar", UseColumnTextForButtonValue = true, FillWeight = 70 });

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["Editar"].Index)
                    EditarEmpleado(e.RowIndex);
            };

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
        }

        private void CargarEmpleados()
        {
            dgv.Rows.Clear();
            var buscar    = txtBuscar.Text.ToLower();
            var empleados = DataManager.ObtenerUsuarios()
                .Where(u => string.IsNullOrEmpty(buscar) ||
                    u.NombreCompleto.ToLower().Contains(buscar) ||
                    u.Email.ToLower().Contains(buscar)         ||
                    u.Departamento.ToLower().Contains(buscar)  ||
                    u.Cargo.ToLower().Contains(buscar))
                .OrderBy(u => string.IsNullOrWhiteSpace(u.Departamento) ? "zzz" : u.Departamento)
                .ThenBy(u => u.NombreCompleto)
                .ToList();

            // Agrupar por departamento
            var grupos = empleados
                .GroupBy(u => string.IsNullOrWhiteSpace(u.Departamento) ? "Sin departamento" : u.Departamento)
                .OrderBy(g => g.Key == "Sin departamento" ? "zzz" : g.Key);

            foreach (var grupo in grupos)
            {
                // Fila encabezado de departamento
                int hi = dgv.Rows.Add("", $"🏢  {grupo.Key}  ({grupo.Count()} empleado{(grupo.Count() != 1 ? "s" : "")})",
                    "", "", "", "", "", "", "");
                var hr = dgv.Rows[hi];
                hr.DefaultCellStyle.BackColor             = Color.FromArgb(37, 99, 235);
                hr.DefaultCellStyle.ForeColor             = Color.White;
                hr.DefaultCellStyle.Font                  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                hr.DefaultCellStyle.SelectionBackColor    = Color.FromArgb(37, 99, 235);
                hr.DefaultCellStyle.SelectionForeColor    = Color.White;
                if (hr.Cells["Editar"] is DataGridViewButtonCell bc) bc.Value = "";

                foreach (var u in grupo)
                {
                    dgv.Rows.Add(u.Id, u.NombreCompleto, u.Usuario_Login, u.Email,
                        u.Departamento, u.Cargo, u.Rol.ToString(),
                        u.Activo ? "✅ Activo" : "❌ Inactivo");
                    if (!u.Activo)
                        dgv.Rows[dgv.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Gray;
                }
            }
        }

        private void EditarEmpleado(int rowIndex)
        {
            var id = dgv.Rows[rowIndex].Cells["Id"].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;
            var u = DataManager.ObtenerUsuarios().FirstOrDefault(x => x.Id == id);
            if (u != null) AbrirFormEmpleado(u);
        }

        private void AbrirFormEmpleado(Usuario? usuario)
        {
            new EmpleadoForm(usuario).ShowDialog();
            CargarEmpleados();
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Formulario gestión de departamentos
    // ─────────────────────────────────────────────────────────
    public class GestionDepartamentosForm : Form
    {
        private ListBox lstDepartamentos = null!;
        private TextBox txtNuevo         = null!;

        public GestionDepartamentosForm()
        {
            this.Text = "🏢 Gestión de Departamentos";
            this.Size = new Size(420, 480);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = Colores.Fondo;
            InitUI();
            CargarLista();
        }

        private void InitUI()
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Colores.Primario };
            new Label { Text = "🏢  Departamentos", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 14), AutoSize = true }.Parent = pnlHeader;

            new Label { Text = "Departamentos activos:", Location = new Point(20, 70), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario }.Parent = this;

            lstDepartamentos = new ListBox { Location = new Point(20, 92), Size = new Size(362, 210), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, SelectionMode = SelectionMode.One };
            this.Controls.Add(lstDepartamentos);

            var btnEliminar = new Button
            {
                Text = "🗑️  Desactivar seleccionado", Location = new Point(20, 314), Size = new Size(210, 36),
                BackColor = Colores.Alerta, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += (s, e) =>
            {
                if (lstDepartamentos.SelectedItem is DeptItem item)
                {
                    if (MessageBox.Show(
                        $"¿Desactivar \"{item.Nombre}\"?\nLos empleados asignados no se verán afectados.",
                        "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DataManager.EliminarDepartamento(item.Id);
                        CargarLista();
                    }
                }
            };
            this.Controls.Add(btnEliminar);

            new Label { Text = "Agregar nuevo departamento:", Location = new Point(20, 366), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario }.Parent = this;
            txtNuevo = new TextBox { Location = new Point(20, 388), Size = new Size(240, 30), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Nombre del departamento..." };
            this.Controls.Add(txtNuevo);

            var btnAgregar = new Button
            {
                Text = "➕ Agregar", Location = new Point(272, 386), Size = new Size(110, 34),
                BackColor = Colores.Acento, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += (s, e) =>
            {
                var nombre = txtNuevo.Text.Trim();
                if (string.IsNullOrEmpty(nombre)) return;
                try
                {
                    DataManager.AgregarDepartamento(new Departamento { Nombre = nombre });
                    txtNuevo.Clear();
                    CargarLista();
                }
                catch
                {
                    MessageBox.Show("Ya existe un departamento con ese nombre.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            this.Controls.Add(btnAgregar);
            this.Controls.Add(pnlHeader);
        }

        private void CargarLista()
        {
            lstDepartamentos.Items.Clear();
            foreach (var d in DataManager.ObtenerDepartamentos())
                lstDepartamentos.Items.Add(new DeptItem(d.Id, d.Nombre));
        }

        private class DeptItem
        {
            public int    Id     { get; }
            public string Nombre { get; }
            public DeptItem(int id, string nombre) { Id = id; Nombre = nombre; }
            public override string ToString() => Nombre;
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Formulario crear / editar empleado
    //  Departamento → ComboBox desde tabla Departamentos
    // ─────────────────────────────────────────────────────────
    public class EmpleadoForm : Form
    {
        private readonly Usuario? _usuarioExistente;
        private TextBox  txtNombre = null!, txtApellido = null!, txtEmail = null!,
                         txtLogin  = null!, txtPassword = null!, txtCargo = null!;
        private ComboBox cmbDepto  = null!, cmbRol      = null!;
        private CheckBox chkActivo = null!;

        public EmpleadoForm(Usuario? usuario)
        {
            _usuarioExistente = usuario;
            InitUI();
            if (usuario != null) CargarDatos(usuario);
        }

        private void InitUI()
        {
            this.Text            = _usuarioExistente == null ? "Nuevo Empleado" : "Editar Empleado";
            this.Size            = new Size(480, 650);
            this.MinimumSize     = new Size(480, 400);
            this.AutoScroll      = true;
            this.AutoScrollMargin = new Size(0, 20);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = Colores.Fondo;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Colores.Primario };
            new Label { Text = this.Text, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 15), AutoSize = true }.Parent = pnlHeader;

            int y = 80;
            Campo("Nombre *",           ref txtNombre!,   y); y += 60;
            Campo("Apellido *",         ref txtApellido!, y); y += 60;
            Campo("Email *",            ref txtEmail!,    y); y += 60;
            Campo("Usuario (Login) *",  ref txtLogin!,    y); y += 60;
            Campo("Contraseña *",       ref txtPassword!, y, isPassword: true); y += 60;

            // ── Departamento desde tabla SQL ──────────────────
            new Label { Text = "Departamento", Location = new Point(25, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }.Parent = this;
            cmbDepto = new ComboBox { Location = new Point(25, y + 20), Size = new Size(420, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbDepto.Items.Add("(Sin departamento)");
            foreach (var d in DataManager.ObtenerDepartamentos())
                cmbDepto.Items.Add(d.Nombre);
            cmbDepto.SelectedIndex = 0;
            this.Controls.Add(cmbDepto);
            y += 60;

            Campo("Cargo", ref txtCargo!, y); y += 60;

            // ── Rol ───────────────────────────────────────────
            new Label { Text = "Rol *", Location = new Point(25, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }.Parent = this;
            cmbRol = new ComboBox { Location = new Point(25, y + 20), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbRol.Items.AddRange(new object[] { "Empleado", "Administrador" });
            cmbRol.SelectedIndex = 0;
            this.Controls.Add(cmbRol);

            chkActivo = new CheckBox { Text = "Cuenta Activa", Location = new Point(250, y + 22), AutoSize = true, Font = new Font("Segoe UI", 10), Checked = true };
            this.Controls.Add(chkActivo);
            y += 65;

            var btnGuardar = new Button
            {
                Text = "💾  Guardar", Location = new Point(25, y + 15), Size = new Size(160, 42),
                BackColor = Colores.Acento, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            var btnCancelar = new Button
            {
                Text = "Cancelar", Location = new Point(200, y + 15), Size = new Size(110, 42),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand
            };
            btnCancelar.Click += (s, e) => this.Close();

            if (_usuarioExistente != null && _usuarioExistente.Id != Session.UsuarioActual?.Id)
            {
                var btnElim = new Button
                {
                    Text = "🗑️ Desactivar", Location = new Point(325, y + 15), Size = new Size(120, 42),
                    BackColor = Colores.Alerta, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand
                };
                btnElim.FlatAppearance.BorderSize = 0;
                btnElim.Click += (s, e) =>
                {
                    if (MessageBox.Show("¿Desactivar esta cuenta?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    { DataManager.EliminarUsuario(_usuarioExistente.Id); this.Close(); }
                };
                this.Controls.Add(btnElim);
            }

            this.Controls.Add(btnGuardar);
            this.Controls.Add(btnCancelar);
            this.Controls.Add(pnlHeader);
        }

        private void Campo(string label, ref TextBox txt, int y, bool isPassword = false)
        {
            new Label { Text = label, Location = new Point(25, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) }.Parent = this;
            txt = new TextBox { Location = new Point(25, y + 20), Size = new Size(420, 30), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            if (isPassword) txt.PasswordChar = '●';
            this.Controls.Add(txt);
        }

        private void CargarDatos(Usuario u)
        {
            txtNombre.Text   = u.Nombre;
            txtApellido.Text = u.Apellido;
            txtEmail.Text    = u.Email;
            txtLogin.Text    = u.Usuario_Login;
            txtPassword.Text = u.Password;
            txtCargo.Text    = u.Cargo;
            cmbRol.SelectedIndex = u.Rol == UserRole.Administrador ? 1 : 0;
            chkActivo.Checked    = u.Activo;

            if (!string.IsNullOrEmpty(u.Departamento))
            {
                int idx = cmbDepto.FindStringExact(u.Departamento);
                cmbDepto.SelectedIndex = idx >= 0 ? idx : 0;
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text)  ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Complete los campos obligatorios (*).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            var todos = DataManager.ObtenerUsuarios();
            if (todos.Any(u => u.Usuario_Login.ToLower() == txtLogin.Text.Trim().ToLower() && u.Id != (_usuarioExistente?.Id ?? "")))
            {
                MessageBox.Show("El nombre de usuario ya existe.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            string dept = cmbDepto.SelectedIndex == 0 ? "" : cmbDepto.SelectedItem?.ToString() ?? "";

            if (_usuarioExistente == null)
            {
                DataManager.AgregarUsuario(new Usuario
                {
                    Nombre = txtNombre.Text.Trim(), Apellido = txtApellido.Text.Trim(),
                    Email  = txtEmail.Text.Trim(),  Usuario_Login = txtLogin.Text.Trim(),
                    Password = txtPassword.Text,    Departamento  = dept,
                    Cargo    = txtCargo.Text.Trim(),
                    Rol      = cmbRol.SelectedIndex == 1 ? UserRole.Administrador : UserRole.Empleado,
                    Activo   = chkActivo.Checked
                });
                MessageBox.Show("✅ Empleado creado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _usuarioExistente.Nombre        = txtNombre.Text.Trim();
                _usuarioExistente.Apellido      = txtApellido.Text.Trim();
                _usuarioExistente.Email         = txtEmail.Text.Trim();
                _usuarioExistente.Usuario_Login = txtLogin.Text.Trim();
                _usuarioExistente.Password      = txtPassword.Text;
                _usuarioExistente.Departamento  = dept;
                _usuarioExistente.Cargo         = txtCargo.Text.Trim();
                _usuarioExistente.Rol           = cmbRol.SelectedIndex == 1 ? UserRole.Administrador : UserRole.Empleado;
                _usuarioExistente.Activo        = chkActivo.Checked;
                DataManager.ActualizarUsuario(_usuarioExistente);
                MessageBox.Show("✅ Empleado actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.Close();
        }
    }
}
