using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EmpresaApp.Data;
using EmpresaApp.Models;
using EmpresaApp.Utils;

namespace EmpresaApp.Forms
{
    public class MensajesControl : UserControl
    {
        private readonly TipoMensaje_Enum _tipoFiltro;
        private ListBox _lista = null!;
        private TextBox _detalle = null!;
        private List<Mensaje> _mensajes = new();

        public MensajesControl(TipoMensaje_Enum tipo)
        {
            _tipoFiltro = tipo;
            Dock = DockStyle.Fill;
            BuildUi();
            Cargar();
        }

        private void BuildUi()
        {
            var top = MinimalUi.CreateTopBar();
            if (Session.EsAdmin)
            {
                var btnNuevo = MinimalUi.CreateButton(_tipoFiltro == TipoMensaje_Enum.Tarea ? "Nueva tarea" : "Nuevo", primary: true);
                btnNuevo.Click += (_, _) => AbrirEnviar();
                MinimalUi.AddToBar(top, btnNuevo);
                var btnGrafo = MinimalUi.CreateButton("Grafo");
                btnGrafo.Click += (_, _) => new GrafoMensajeriaForm().ShowDialog();
                MinimalUi.AddToBar(top, btnGrafo);
            }

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 280 };
            ModernTheme.StyleSplitContainer(split);
            _lista = MinimalUi.CreateListBox();
            _lista.Dock = DockStyle.Fill;
            _lista.SelectedIndexChanged += (_, _) => MostrarDetalle();
            _detalle = MinimalUi.CreateTextBox(multiline: true);
            _detalle.ReadOnly = true;
            _detalle.Font = new System.Drawing.Font("Consolas", 9.5f);
            _detalle.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(_lista);
            split.Panel2.Controls.Add(_detalle);

            Controls.Add(split);
            Controls.Add(top);
        }

        private void Cargar()
        {
            var uid = Session.UsuarioActual?.Id ?? "";
            _mensajes = DataManager.ObtenerMensajesDeUsuario(uid)
                .Where(m => _tipoFiltro == TipoMensaje_Enum.Tarea
                    ? m.Tipo == TipoMensaje.Tarea
                    : m.Tipo == TipoMensaje.Mensaje || m.Tipo == TipoMensaje.Alerta)
                .OrderByDescending(m => m.FechaEnvio)
                .ToList();

            _lista.Items.Clear();
            foreach (var m in _mensajes)
                _lista.Items.Add($"{m.FechaEnvio:dd/MM HH:mm} | {m.RemitenteNombre} | {m.Asunto}");
            if (_lista.Items.Count == 0)
                _detalle.Text = _tipoFiltro == TipoMensaje_Enum.Tarea ? "Sin tareas." : "Sin mensajes.";
            else
            {
                _lista.SelectedIndex = 0;
                MostrarDetalle();
            }
        }

        private void MostrarDetalle()
        {
            if (_lista.SelectedIndex < 0 || _lista.SelectedIndex >= _mensajes.Count)
            {
                _detalle.Text = "Seleccione un elemento.";
                return;
            }

            var m = _mensajes[_lista.SelectedIndex];
            DataManager.MarcarMensajeLeido(m.Id);

            _detalle.Text =
                $"Tipo: {m.Tipo}\r\nDe: {m.RemitenteNombre}\r\nFecha: {m.FechaEnvio:yyyy-MM-dd HH:mm}\r\n" +
                $"Asunto: {m.Asunto}\r\n\r\n{m.Contenido}";

            if (m.Tipo == TipoMensaje.Tarea && m.Estado != EstadoMensaje.Completado)
            {
                var r = MessageBox.Show("Marcar tarea como completada?", "Tarea",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    DataManager.MarcarTareaCompletada(m.Id);
                    Cargar();
                }
            }
        }

        private void AbrirEnviar()
        {
            var tipo = _tipoFiltro == TipoMensaje_Enum.Tarea ? TipoMensaje.Tarea : TipoMensaje.Mensaje;
            new EnviarMensajeForm(null, null, null, tipo).ShowDialog();
            Cargar();
        }
    }

    public class EnviarMensajeForm : Form
    {
        private RadioButton _rbIndividual = null!, _rbDepto = null!;
        private ComboBox _cmbUser = null!, _cmbDepto = null!, _cmbTipo = null!;
        private TextBox _txtAsunto = null!, _txtCuerpo = null!;
        private CheckBox _chkVence = null!;
        private DateTimePicker _dtpVence = null!;

        public EnviarMensajeForm(string? destId, string? destNombre, string? asunto, TipoMensaje tipoInicial)
        {
            Text = "Enviar";
            Size = new System.Drawing.Size(460, 440);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ModernTheme.ApplyToForm(this);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 16, 20, 8),
                ColumnCount = 2,
                RowCount = 7
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            _rbIndividual = new RadioButton { Text = "Usuario", Checked = true, AutoSize = true, ForeColor = ModernTheme.Colors.Text };
            _rbDepto = new RadioButton { Text = "Departamento", AutoSize = true, ForeColor = ModernTheme.Colors.Text };
            var pRadio = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            pRadio.Controls.Add(_rbIndividual);
            pRadio.Controls.Add(_rbDepto);
            layout.Controls.Add(pRadio, 0, 0);
            layout.SetColumnSpan(pRadio, 2);

            _cmbUser = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cmbUser);
            foreach (var u in DataManager.ObtenerUsuarios().Where(u => u.Activo && u.Id != Session.UsuarioActual?.Id))
                _cmbUser.Items.Add(new ComboItem(u.Id, u.NombreCompleto));
            _cmbDepto = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            ModernTheme.StyleComboBox(_cmbDepto);
            foreach (var d in DataManager.ObtenerDepartamentos()) _cmbDepto.Items.Add(d.Nombre);
            if (_cmbDepto.Items.Count > 0) _cmbDepto.SelectedIndex = 0;

            var pPara = new Panel { Dock = DockStyle.Fill };
            _cmbUser.Dock = DockStyle.Fill;
            _cmbDepto.Dock = DockStyle.Fill;
            pPara.Controls.Add(_cmbUser);
            pPara.Controls.Add(_cmbDepto);
            layout.Controls.Add(ModernTheme.CreateLabel("Para", ModernTheme.LabelStyle.Caption), 0, 1);
            layout.Controls.Add(pPara, 1, 1);

            _cmbTipo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            ModernTheme.StyleComboBox(_cmbTipo);
            _cmbTipo.Items.AddRange(new object[] { "Mensaje", "Tarea", "Alerta" });
            _cmbTipo.SelectedIndex = tipoInicial == TipoMensaje.Tarea ? 1 : tipoInicial == TipoMensaje.Alerta ? 2 : 0;
            layout.Controls.Add(ModernTheme.CreateLabel("Tipo", ModernTheme.LabelStyle.Caption), 0, 2);
            layout.Controls.Add(_cmbTipo, 1, 2);

            _txtAsunto = MinimalUi.CreateTextBox();
            _txtAsunto.Dock = DockStyle.Fill;
            if (asunto != null) _txtAsunto.Text = asunto;
            layout.Controls.Add(ModernTheme.CreateLabel("Asunto", ModernTheme.LabelStyle.Caption), 0, 3);
            layout.Controls.Add(_txtAsunto, 1, 3);

            _txtCuerpo = MinimalUi.CreateTextBox(multiline: true);
            _txtCuerpo.Dock = DockStyle.Fill;
            layout.Controls.Add(ModernTheme.CreateLabel("Texto", ModernTheme.LabelStyle.Caption), 0, 4);
            layout.Controls.Add(_txtCuerpo, 1, 4);

            _chkVence = new CheckBox { Text = "Vence", AutoSize = true, ForeColor = ModernTheme.Colors.Text };
            _dtpVence = new DateTimePicker { Enabled = false, Width = 140 };
            ModernTheme.StyleDateTimePicker(_dtpVence);
            _chkVence.CheckedChanged += (_, _) => _dtpVence.Enabled = _chkVence.Checked;
            var pV = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            pV.Controls.Add(_chkVence);
            pV.Controls.Add(_dtpVence);
            layout.Controls.Add(pV, 0, 5);
            layout.SetColumnSpan(pV, 2);

            _rbIndividual.CheckedChanged += (_, _) => Alternar();
            _rbDepto.CheckedChanged += (_, _) => Alternar();
            Alternar();

            if (destId != null)
                for (int i = 0; i < _cmbUser.Items.Count; i++)
                    if ((_cmbUser.Items[i] as ComboItem)?.Id == destId)
                        _cmbUser.SelectedIndex = i;

            var footer = UiLayout.CreateFooterBar();
            var btnOk = MinimalUi.CreateButton("Enviar", primary: true);
            var btnNo = MinimalUi.CreateButton("Cancelar");
            btnNo.Click += (_, _) => Close();
            btnOk.Click += (_, _) => Enviar();
            UiLayout.AddFooterButton(footer, btnNo);
            UiLayout.AddFooterButton(footer, btnOk);

            Controls.Add(footer);
            Controls.Add(layout);
        }

        private void Alternar()
        {
            _cmbUser.Visible = _rbIndividual.Checked;
            _cmbDepto.Visible = _rbDepto.Checked;
        }

        private void Enviar()
        {
            if (string.IsNullOrWhiteSpace(_txtAsunto.Text) || string.IsNullOrWhiteSpace(_txtCuerpo.Text))
            {
                MessageBox.Show("Complete asunto y texto.");
                return;
            }
            int t = _cmbTipo.SelectedIndex;
            var tipo = t == 1 ? TipoMensaje.Tarea : t == 2 ? TipoMensaje.Alerta : TipoMensaje.Mensaje;
            DateTime? vence = _chkVence.Checked ? _dtpVence.Value : null;

            if (_rbDepto.Checked)
            {
                if (_cmbDepto.SelectedItem == null) return;
                string dept = _cmbDepto.SelectedItem.ToString()!;
                var miembros = DataManager.ObtenerUsuarios()
                    .Where(u => u.Activo && u.Departamento == dept && u.Id != Session.UsuarioActual?.Id).ToList();
                if (miembros.Count == 0)
                {
                    MessageBox.Show("Sin usuarios en ese departamento.");
                    return;
                }
                foreach (var m in miembros)
                    DataManager.AgregarMensaje(CrearMensaje(m.Id, m.NombreCompleto, TipoDestino.Departamento, dept, tipo, vence));
            }
            else
            {
                if (_cmbUser.SelectedItem is not ComboItem dest)
                {
                    MessageBox.Show("Seleccione destinatario.");
                    return;
                }
                DataManager.AgregarMensaje(CrearMensaje(dest.Id, dest.Nombre, TipoDestino.Individual, null, tipo, vence));
            }
            Close();
        }

        private Mensaje CrearMensaje(string destId, string destNombre, TipoDestino td, string? dept, TipoMensaje tipo, DateTime? vence) =>
            new Mensaje
            {
                RemitenteId = Session.UsuarioActual!.Id,
                RemitenteNombre = Session.UsuarioActual.NombreCompleto,
                TipoDestino = td,
                DestinatarioId = destId,
                DestinatarioNombre = destNombre,
                DepartamentoDestino = dept,
                Asunto = _txtAsunto.Text.Trim(),
                Contenido = _txtCuerpo.Text.Trim(),
                Tipo = tipo,
                FechaVencimiento = vence
            };
    }

    public class GrafoMensajeriaForm : Form
    {
        public GrafoMensajeriaForm()
        {
            Text = "Grafo";
            Size = new System.Drawing.Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;

            var grid = MinimalUi.CreateGrid();
            grid.Columns.Add("De", "De");
            grid.Columns.Add("A", "A");
            grid.Columns.Add("Peso", "Peso");
            grid.Columns.Add("Fecha", "Fecha");
            foreach (var a in DataManager.ObtenerGrafo())
                grid.Rows.Add(a.RemitenteNombre, a.DestinatarioNombre, a.Peso, a.UltimaInteraccion.ToString("dd/MM/yyyy"));
            Controls.Add(grid);
        }
    }

    public class ComboItem
    {
        public string Id { get; }
        public string Nombre { get; }
        public ComboItem(string id, string nombre) { Id = id; Nombre = nombre; }
        public override string ToString() => Nombre;
    }

    public class EnviarMensajeControl : UserControl
    {
        public EnviarMensajeControl()
        {
            Dock = DockStyle.Fill;
            if (!Session.EsAdmin)
            {
                Controls.Add(new Label { Text = "Solo administradores.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
                return;
            }
            var f = new EnviarMensajeForm(null, null, null, TipoMensaje.Mensaje)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            Controls.Add(f);
            f.Show();
        }
    }
}
