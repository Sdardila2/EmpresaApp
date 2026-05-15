// ═══════════════════════════════════════════════════════════════
//  MensajesControl.cs
//  — Empleados NO pueden enviar mensajes ni tareas (solo recibir)
//  — Destinatario individual agrupado por departamento en ComboBox
//  — Destinatario grupal: ComboBox de departamentos desde SQL
// ═══════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Panel _panelLista   = null!;
        private Panel _panelDetalle = null!;

        public MensajesControl(TipoMensaje_Enum tipo)
        {
            _tipoFiltro = tipo;
            this.BackColor = Colores.Fondo;
            InitUI();
            CargarMensajes();
        }

        private void InitUI()
        {
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(
                new Pen(Color.FromArgb(226, 232, 240)), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            // Solo administradores pueden enviar mensajes / tareas
            if (Session.EsAdmin)
            {
                var btnNuevo = new Button
                {
                    Text      = _tipoFiltro == TipoMensaje_Enum.Tarea ? "➕ Nueva Tarea" : "✉️ Nuevo Mensaje",
                    Location  = new Point(10, 10), Size = new Size(155, 36),
                    BackColor = Colores.Secundario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btnNuevo.FlatAppearance.BorderSize = 0;
                btnNuevo.Click += (s, e) => AbrirEnviarMensaje();
                toolbar.Controls.Add(btnNuevo);

                var btnGrafo = new Button
                {
                    Text      = "📊 Ver Grafo",
                    Location  = new Point(175, 10), Size = new Size(120, 36),
                    BackColor = Colores.Primario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btnGrafo.FlatAppearance.BorderSize = 0;
                btnGrafo.Click += (s, e) => new GrafoMensajeriaForm().ShowDialog();
                toolbar.Controls.Add(btnGrafo);
            }
            else
            {
                // Mensaje informativo para empleados
                toolbar.Controls.Add(new Label
                {
                    Text      = _tipoFiltro == TipoMensaje_Enum.Tarea
                                    ? "📋 Tus tareas asignadas (solo lectura)"
                                    : "📨 Tus mensajes recibidos (solo lectura)",
                    Location  = new Point(12, 18),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Colores.TextoSecundario
                });
            }

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill, SplitterDistance = 340, BorderStyle = BorderStyle.None
            };

            _panelLista = new Panel
            { Dock = DockStyle.Fill, BackColor = Color.White, AutoScroll = true };
            _panelDetalle = new Panel
            { Dock = DockStyle.Fill, BackColor = Colores.Fondo, AutoScroll = true, Padding = new Padding(20) };

            split.Panel1.Controls.Add(_panelLista);
            split.Panel2.Controls.Add(_panelDetalle);
            MostrarBienvenidaDetalle();

            this.Controls.Add(split);
            this.Controls.Add(toolbar);
        }

        private void CargarMensajes()
        {
            _panelLista.Controls.Clear();
            var uid      = Session.UsuarioActual?.Id ?? "";
            var mensajes = DataManager.ObtenerMensajesDeUsuario(uid)
                .Where(m => _tipoFiltro == TipoMensaje_Enum.Tarea
                    ? m.Tipo == TipoMensaje.Tarea
                    : m.Tipo == TipoMensaje.Mensaje || m.Tipo == TipoMensaje.Alerta)
                .ToList();

            if (mensajes.Count == 0)
            {
                _panelLista.Controls.Add(new Label
                {
                    Text      = _tipoFiltro == TipoMensaje_Enum.Tarea ? "No tienes tareas asignadas." : "No tienes mensajes.",
                    Font      = new Font("Segoe UI", 10), ForeColor = Colores.TextoSecundario,
                    Location  = new Point(15, 20), AutoSize = true
                });
                return;
            }

            int y = 0;
            foreach (var m in mensajes)
            {
                var card = CrearCardMensaje(m, y);
                _panelLista.Controls.Add(card);
                y += 72;
            }
        }

        private Panel CrearCardMensaje(Mensaje m, int y)
        {
            bool  esNuevo    = m.Estado == EstadoMensaje.Nuevo;
            Color colorBorde = m.Tipo == TipoMensaje.Alerta ? Colores.Alerta : m.Tipo == TipoMensaje.Tarea ? Colores.Advertencia : Colores.Secundario;
            string icono     = m.Tipo == TipoMensaje.Tarea ? "✅" : m.Tipo == TipoMensaje.Alerta ? "🚨" : "📩";
            string badge     = m.Estado == EstadoMensaje.Completado ? "HECHO" : m.Estado == EstadoMensaje.Nuevo ? "NUEVO" : "";
            string sufijo    = m.TipoDestino == TipoDestino.Departamento ? $"  [📢 {m.DepartamentoDestino}]" : "";

            var card = new Panel
            {
                Location = new Point(0, y), Size = new Size(_panelLista.Width > 0 ? _panelLista.Width - 1 : 340, 70),
                BackColor = esNuevo ? Color.FromArgb(239, 246, 255) : Color.White,
                Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(colorBorde), 0, 0, 4, card.Height);
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, card.Height - 1, card.Width, card.Height - 1);
            };

            card.Controls.Add(new Label { Text = $"{icono} {m.RemitenteNombre}{sufijo}", Font = new Font("Segoe UI", 9.5f, esNuevo ? FontStyle.Bold : FontStyle.Regular), ForeColor = Colores.TextoPrimario, Location = new Point(12, 8), AutoSize = true });
            card.Controls.Add(new Label { Text = m.Asunto.Length > 38 ? m.Asunto[..35] + "..." : m.Asunto, Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(12, 28), AutoSize = true });
            card.Controls.Add(new Label { Text = m.FechaEnvio.ToString("dd/MM HH:mm"), Font = new Font("Segoe UI", 7.5f), ForeColor = Colores.TextoSecundario, Location = new Point(12, 50), AutoSize = true });

            if (!string.IsNullOrEmpty(badge))
                card.Controls.Add(new Label { Text = badge, Font = new Font("Segoe UI", 7, FontStyle.Bold), ForeColor = Color.White, BackColor = m.Estado == EstadoMensaje.Completado ? Colores.Acento : colorBorde, Size = new Size(50, 18), Location = new Point(card.Width - 65, 8), TextAlign = ContentAlignment.MiddleCenter, Anchor = AnchorStyles.Right | AnchorStyles.Top });

            card.Click += (s, e) => MostrarDetalleMensaje(m);
            foreach (Control c in card.Controls) c.Click += (s, e) => MostrarDetalleMensaje(m);
            return card;
        }

        private void MostrarBienvenidaDetalle()
        {
            _panelDetalle.Controls.Clear();
            _panelDetalle.Controls.Add(new Label { Text = "← Selecciona un mensaje para verlo", Font = new Font("Segoe UI", 11), ForeColor = Colores.TextoSecundario, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
        }

        private void MostrarDetalleMensaje(Mensaje m)
        {
            DataManager.MarcarMensajeLeido(m.Id);
            m.Estado = EstadoMensaje.Leido;
            CargarMensajes();

            _panelDetalle.Controls.Clear();
            string icono     = m.Tipo == TipoMensaje.Tarea ? "✅" : m.Tipo == TipoMensaje.Alerta ? "🚨" : "📩";
            Color  colorTipo = m.Tipo == TipoMensaje.Alerta ? Colores.Alerta : m.Tipo == TipoMensaje.Tarea ? Colores.Advertencia : Colores.Secundario;

            var pnlDetalle = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(25), AutoScroll = true };
            pnlDetalle.Controls.Add(new Label { Text = $"{icono} {m.Tipo}", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = colorTipo, Location = new Point(25, 20), AutoSize = true });
            pnlDetalle.Controls.Add(new Label { Text = m.Asunto, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Colores.TextoPrimario, Location = new Point(25, 45), Size = new Size(500, 40), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });

            var pnlMeta = new Panel { Location = new Point(25, 95), Size = new Size(500, m.TipoDestino == TipoDestino.Departamento ? 80 : 60), BackColor = Color.FromArgb(248, 250, 252), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlMeta.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, pnlMeta.Width - 1, pnlMeta.Height - 1);
            pnlMeta.Controls.Add(new Label { Text = $"👤 De: {m.RemitenteNombre}", Font = new Font("Segoe UI", 9.5f), ForeColor = Colores.TextoPrimario, Location = new Point(12, 8), AutoSize = true });
            if (m.TipoDestino == TipoDestino.Departamento)
                pnlMeta.Controls.Add(new Label { Text = $"📢 Enviado al departamento: {m.DepartamentoDestino}", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.Secundario, Location = new Point(12, 30), AutoSize = true });
            pnlMeta.Controls.Add(new Label { Text = $"📅 {m.FechaEnvio:dddd, dd 'de' MMMM yyyy 'a las' HH:mm}", Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(12, m.TipoDestino == TipoDestino.Departamento ? 54 : 30), AutoSize = true });

            pnlDetalle.Controls.Add(pnlMeta);
            pnlDetalle.Controls.Add(new Label { Text = "Contenido:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario, Location = new Point(25, 190), AutoSize = true });
            pnlDetalle.Controls.Add(new RichTextBox { Text = m.Contenido, ReadOnly = true, Location = new Point(25, 215), Size = new Size(500, 160), Font = new Font("Segoe UI", 10.5f), BackColor = Color.FromArgb(248, 250, 252), BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.Vertical, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });

            // Botón responder SOLO para administradores
            if (Session.EsAdmin)
            {
                var btnResponder = new Button
                {
                    Text = "↩️  Responder", Location = new Point(25, 390), Size = new Size(140, 38),
                    BackColor = Colores.Secundario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand,
                    Enabled = m.TipoDestino == TipoDestino.Individual
                };
                btnResponder.FlatAppearance.BorderSize = 0;
                btnResponder.Click += (s, e) => AbrirEnviarMensaje(m.RemitenteId, m.RemitenteNombre, $"Re: {m.Asunto}");
                pnlDetalle.Controls.Add(btnResponder);
            }

            if (m.Tipo == TipoMensaje.Tarea && m.Estado != EstadoMensaje.Completado)
            {
                var btnCompletar = new Button
                {
                    Text = "✅ Marcar Completada",
                    Location = Session.EsAdmin ? new Point(180, 390) : new Point(25, 390),
                    Size = new Size(175, 38),
                    BackColor = Colores.Acento, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
                };
                btnCompletar.FlatAppearance.BorderSize = 0;
                btnCompletar.Click += (s, e) =>
                {
                    DataManager.MarcarTareaCompletada(m.Id);
                    MessageBox.Show("✅ Tarea marcada como completada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarMensajes();
                    MostrarBienvenidaDetalle();
                };
                pnlDetalle.Controls.Add(btnCompletar);
            }

            _panelDetalle.Controls.Add(pnlDetalle);
        }

        private void AbrirEnviarMensaje(string? destId = null, string? destNombre = null, string? asunto = null)
        {
            var tipo = _tipoFiltro == TipoMensaje_Enum.Tarea ? TipoMensaje.Tarea : TipoMensaje.Mensaje;
            new EnviarMensajeForm(destId, destNombre, asunto, tipo).ShowDialog();
            CargarMensajes();
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Formulario de redacción (solo accesible para admins)
    //  Destinatario individual agrupado por departamento
    // ─────────────────────────────────────────────────────────
    public class EnviarMensajeForm : Form
    {
        private RadioButton  _rbIndividual    = null!;
        private RadioButton  _rbDepartamento  = null!;
        private ComboBox     _cmbDestinatario = null!;
        private ComboBox     _cmbDepartamento = null!;
        private Panel        _pnlIndividual   = null!;
        private Panel        _pnlDepartamento = null!;
        private TextBox      _txtAsunto       = null!;
        private RichTextBox  _txtContenido    = null!;
        private ComboBox     _cmbTipo         = null!;
        private DateTimePicker _dtpVencimiento= null!;
        private CheckBox     _chkVencimiento  = null!;
        private readonly TipoMensaje _tipoInicial;

        public EnviarMensajeForm(string? destId = null, string? destNombre = null,
                                  string? asunto = null, TipoMensaje tipo = TipoMensaje.Mensaje)
        {
            _tipoInicial = tipo;
            InitUI(destId, destNombre, asunto);
        }

        private void InitUI(string? destId, string? destNombre, string? asunto)
        {
            this.Text = "Nuevo Mensaje / Tarea";
            this.Size = new Size(560, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Colores.Fondo;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Colores.Primario };
            new Label { Text = "✉️  Redactar Mensaje / Tarea", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 14), AutoSize = true }.Parent = pnlHeader;

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(0, 55, 0, 0) };
            int y = 0;

            // ── Tipo de destino ────────────────────────────────
            new Label { Text = "Tipo de envío:", Location = new Point(20, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = scroll;
            _rbIndividual   = new RadioButton { Text = "👤 Individual",       Location = new Point(20,  y + 20), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10) };
            _rbDepartamento = new RadioButton { Text = "📢 Por departamento", Location = new Point(145, y + 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _rbIndividual.CheckedChanged   += (s, e) => AlternarDestino();
            _rbDepartamento.CheckedChanged += (s, e) => AlternarDestino();
            scroll.Controls.Add(_rbIndividual);
            scroll.Controls.Add(_rbDepartamento);
            y += 52;

            // ── Panel individual: ComboBox agrupado por depto ──
            _pnlIndividual = new Panel { Location = new Point(20, y), Size = new Size(510, 80), BackColor = Color.Transparent };
            new Label { Text = "Para (usuario):", Location = new Point(0, 0), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = _pnlIndividual;
            _cmbDestinatario = new ComboBox { Location = new Point(0, 20), Size = new Size(510, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            // Agregar usuarios agrupados por departamento
            var usuarios = DataManager.ObtenerUsuarios()
                .Where(u => u.Activo && u.Id != Session.UsuarioActual?.Id)
                .OrderBy(u => string.IsNullOrWhiteSpace(u.Departamento) ? "zzz" : u.Departamento)
                .ThenBy(u => u.NombreCompleto)
                .ToList();

            var grupos = usuarios.GroupBy(u => string.IsNullOrWhiteSpace(u.Departamento) ? "Sin departamento" : u.Departamento);
            foreach (var grupo in grupos)
            {
                _cmbDestinatario.Items.Add(new ComboSeparador($"── {grupo.Key} ──"));
                foreach (var u in grupo)
                    _cmbDestinatario.Items.Add(new ComboItem(u.Id, $"{u.NombreCompleto} ({u.Cargo})"));
            }

            // Evitar seleccionar separadores
            _cmbDestinatario.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbDestinatario.SelectedItem is ComboSeparador)
                    _cmbDestinatario.SelectedIndex = -1;
            };

            if (destId != null)
                for (int i = 0; i < _cmbDestinatario.Items.Count; i++)
                    if ((_cmbDestinatario.Items[i] as ComboItem)?.Id == destId)
                    { _cmbDestinatario.SelectedIndex = i; break; }

            _pnlIndividual.Controls.Add(_cmbDestinatario);
            scroll.Controls.Add(_pnlIndividual);

            // ── Panel departamento: ComboBox desde SQL ─────────
            _pnlDepartamento = new Panel { Location = new Point(20, y), Size = new Size(510, 80), BackColor = Color.Transparent, Visible = false };
            new Label { Text = "Para (departamento):", Location = new Point(0, 0), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = _pnlDepartamento;
            _cmbDepartamento = new ComboBox { Location = new Point(0, 20), Size = new Size(300, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            foreach (var d in DataManager.ObtenerDepartamentos()) _cmbDepartamento.Items.Add(d.Nombre);
            if (_cmbDepartamento.Items.Count > 0) _cmbDepartamento.SelectedIndex = 0;
            _pnlDepartamento.Controls.Add(_cmbDepartamento);
            scroll.Controls.Add(_pnlDepartamento);
            y += 88;

            // ── Tipo de mensaje ────────────────────────────────
            new Label { Text = "Tipo:", Location = new Point(20, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = scroll;
            _cmbTipo = new ComboBox { Location = new Point(20, y + 20), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            _cmbTipo.Items.AddRange(new object[] { "Mensaje", "Tarea", "Alerta" });
            _cmbTipo.SelectedIndex = _tipoInicial == TipoMensaje.Tarea ? 1 : _tipoInicial == TipoMensaje.Alerta ? 2 : 0;
            scroll.Controls.Add(_cmbTipo);
            y += 62;

            // ── Asunto ─────────────────────────────────────────
            new Label { Text = "Asunto:", Location = new Point(20, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = scroll;
            _txtAsunto = new TextBox { Location = new Point(20, y + 20), Size = new Size(510, 30), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            if (asunto != null) _txtAsunto.Text = asunto;
            scroll.Controls.Add(_txtAsunto);
            y += 62;

            // ── Fecha límite ───────────────────────────────────
            _chkVencimiento = new CheckBox { Text = "Fecha límite:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Colores.TextoSecundario };
            _dtpVencimiento = new DateTimePicker { Location = new Point(145, y - 3), Size = new Size(200, 28), Format = DateTimePickerFormat.Short, Enabled = false };
            _chkVencimiento.CheckedChanged += (s, e) => _dtpVencimiento.Enabled = _chkVencimiento.Checked;
            scroll.Controls.Add(_chkVencimiento);
            scroll.Controls.Add(_dtpVencimiento);
            y += 38;

            // ── Contenido ──────────────────────────────────────
            new Label { Text = "Contenido:", Location = new Point(20, y), AutoSize = true, ForeColor = Colores.TextoSecundario, Font = new Font("Segoe UI", 9, FontStyle.Bold) }.Parent = scroll;
            _txtContenido = new RichTextBox { Location = new Point(20, y + 22), Size = new Size(510, 140), Font = new Font("Segoe UI", 10.5f), BorderStyle = BorderStyle.FixedSingle, ScrollBars = RichTextBoxScrollBars.Vertical };
            scroll.Controls.Add(_txtContenido);
            y += 180;

            // ── Botones ────────────────────────────────────────
            var btnEnviar = new Button { Text = "📤  Enviar", Location = new Point(20, y + 10), Size = new Size(130, 42), BackColor = Colores.Secundario, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += BtnEnviar_Click;
            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(165, y + 10), Size = new Size(100, 42), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand };
            btnCancelar.Click += (s, e) => this.Close();
            scroll.Controls.Add(btnEnviar);
            scroll.Controls.Add(btnCancelar);

            this.Controls.Add(scroll);
            this.Controls.Add(pnlHeader);
        }

        private void AlternarDestino()
        {
            _pnlIndividual.Visible   = _rbIndividual.Checked;
            _pnlDepartamento.Visible = _rbDepartamento.Checked;
        }

        private void BtnEnviar_Click(object? sender, EventArgs e)
        {
            bool esGrupal = _rbDepartamento.Checked;

            if (esGrupal && _cmbDepartamento.SelectedItem == null)
            { MessageBox.Show("Seleccione un departamento.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!esGrupal && (_cmbDestinatario.SelectedItem is not ComboItem))
            { MessageBox.Show("Seleccione un destinatario válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(_txtAsunto.Text) || string.IsNullOrWhiteSpace(_txtContenido.Text))
            { MessageBox.Show("Complete todos los campos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int tipoIdx = _cmbTipo.SelectedIndex;
            TipoMensaje tipo = tipoIdx == 1 ? TipoMensaje.Tarea : tipoIdx == 2 ? TipoMensaje.Alerta : TipoMensaje.Mensaje;

            if (esGrupal)
            {
                string dept    = _cmbDepartamento.SelectedItem!.ToString()!;
                var miembros   = DataManager.ObtenerUsuarios()
                    .Where(u => u.Activo && u.Departamento == dept && u.Id != Session.UsuarioActual?.Id).ToList();

                if (miembros.Count == 0)
                { MessageBox.Show("No hay usuarios activos en ese departamento.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                foreach (var miembro in miembros)
                    DataManager.AgregarMensaje(new Mensaje
                    {
                        RemitenteId = Session.UsuarioActual!.Id, RemitenteNombre = Session.UsuarioActual.NombreCompleto,
                        TipoDestino = TipoDestino.Departamento,
                        DestinatarioId = miembro.Id, DestinatarioNombre = miembro.NombreCompleto,
                        DepartamentoDestino = dept, Asunto = _txtAsunto.Text.Trim(),
                        Contenido = _txtContenido.Text.Trim(), Tipo = tipo,
                        FechaVencimiento = _chkVencimiento.Checked ? _dtpVencimiento.Value : null
                    });

                MessageBox.Show($"✅ Mensaje enviado a {miembros.Count} miembro(s) de {dept}.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var dest = (_cmbDestinatario.SelectedItem as ComboItem)!;
                DataManager.AgregarMensaje(new Mensaje
                {
                    RemitenteId = Session.UsuarioActual!.Id, RemitenteNombre = Session.UsuarioActual.NombreCompleto,
                    TipoDestino = TipoDestino.Individual,
                    DestinatarioId = dest.Id, DestinatarioNombre = dest.Nombre,
                    Asunto = _txtAsunto.Text.Trim(), Contenido = _txtContenido.Text.Trim(),
                    Tipo = tipo, FechaVencimiento = _chkVencimiento.Checked ? _dtpVencimiento.Value : null
                });
                MessageBox.Show("✅ Mensaje enviado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.Close();
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Visor del grafo
    // ─────────────────────────────────────────────────────────
    public class GrafoMensajeriaForm : Form
    {
        public GrafoMensajeriaForm()
        {
            this.Text = "📊 Grafo de Mensajería"; this.Size = new Size(900, 640);
            this.StartPosition = FormStartPosition.CenterParent; this.BackColor = Colores.Fondo;
            this.Font = new Font("Segoe UI", 9.5f);
            InitUI();
        }

        private void InitUI()
        {
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Colores.Primario };
            new Label { Text = "📊 Grafo Dirigido Ponderado — Mensajería entre Usuarios", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 15), AutoSize = true }.Parent = pnlHeader;

            var dgv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 42, RowTemplate = { Height = 36 }, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ScrollBars = ScrollBars.Both };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colores.Primario;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Remitente (A)",        FillWeight = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dept. Remitente",      FillWeight = 130 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Destinatario (B)",     FillWeight = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dept. Destinatario",   FillWeight = 130 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "⚖️ Peso (A→B)",       FillWeight = 110 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Última interacción",   FillWeight = 150 });

            var aristas = DataManager.ObtenerGrafo();
            foreach (var a in aristas)
                dgv.Rows.Add(a.RemitenteNombre, a.RemitenteDepartamento,
                             a.DestinatarioNombre, a.DestinatarioDepartamento,
                             a.Peso, a.UltimaInteraccion.ToString("dd/MM/yyyy HH:mm"));
            if (aristas.Count == 0) dgv.Rows.Add("—", "—", "—", "—", "Sin datos", "—");

            var pnlStats = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.White };
            pnlStats.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, pnlStats.Width, 0);
            int totalMsgs = aristas.Sum(a => a.Peso);
            new Label { Text = $"  Aristas: {aristas.Count}   |   Mensajes en grafo: {totalMsgs}", Font = new Font("Segoe UI", 9), ForeColor = Colores.TextoSecundario, Location = new Point(10, 15), AutoSize = true }.Parent = pnlStats;

            this.Controls.Add(dgv);
            this.Controls.Add(pnlStats);
            this.Controls.Add(pnlHeader);
        }
    }

    // ─────────────────────────────────────────────────────────
    //  Helpers compartidos
    // ─────────────────────────────────────────────────────────
    public class ComboItem
    {
        public string Id     { get; }
        public string Nombre { get; }
        public ComboItem(string id, string nombre) { Id = id; Nombre = nombre; }
        public override string ToString() => Nombre;
    }

    /// <summary>Ítem separador visual en el ComboBox (no seleccionable).</summary>
    public class ComboSeparador
    {
        public string Texto { get; }
        public ComboSeparador(string texto) { Texto = texto; }
        public override string ToString() => Texto;
    }

    public class EnviarMensajeControl : UserControl
    {
        public EnviarMensajeControl()
        {
            this.BackColor = Colores.Fondo;
            if (!Session.EsAdmin)
            {
                this.Controls.Add(new Label
                {
                    Text = "⛔ Solo los administradores pueden enviar comunicados.",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Colores.Alerta,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                });
                return;
            }
            var form = new EnviarMensajeForm(null, null, null, TipoMensaje.Mensaje)
            { TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill, Visible = true };
            this.Controls.Add(form);
        }
    }
}
