using System;
using System.Drawing;
using System.Windows.Forms;

namespace EmpresaApp.Utils
{
    /// <summary>
    /// Helpers de posicionamiento consistente para barras de herramientas, pies de formulario, etc.
    /// </summary>
    public static class UiLayout
    {
        public const int ToolbarHeight = 52;
        public const int FooterHeight = 56;
        public const int ControlHeight = 36;
        public const int ButtonHeight = 36;

        public static int MeasureButtonWidth(string text, int padding = 28) =>
            TextRenderer.MeasureText(text, ModernTheme.FontButton, Size.Empty,
                TextFormatFlags.SingleLine).Width + padding;

        public static void SizeButton(Button btn, int? width = null)
        {
            btn.AutoSize = false;
            btn.Height = ButtonHeight;
            btn.Width = width ?? Math.Max(84, MeasureButtonWidth(btn.Text));
            btn.Margin = new Padding(0, 8, 10, 8);
        }

        public static FlowLayoutPanel CreateTopBar()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                MinimumSize = new Size(0, ToolbarHeight),
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 16),
                BackColor = Color.Transparent
            };
        }

        public static void AddBarControl(FlowLayoutPanel bar, Control control)
        {
            switch (control)
            {
                case Button b:
                    SizeButton(b);
                    break;
                case Label lbl:
                    lbl.AutoSize = true;
                    lbl.Margin = new Padding(0, 12, 8, 8);
                    break;
                case TextBox txt:
                    txt.Height = ControlHeight;
                    txt.Margin = new Padding(0, 8, 12, 8);
                    break;
                case ComboBox cbo:
                    cbo.Height = ControlHeight;
                    cbo.Margin = new Padding(0, 8, 12, 8);
                    break;
                case DateTimePicker dtp:
                    dtp.Height = ControlHeight;
                    dtp.Margin = new Padding(0, 8, 12, 8);
                    break;
                default:
                    control.Margin = new Padding(0, 8, 12, 8);
                    break;
            }
            bar.Controls.Add(control);
        }

        public static FlowLayoutPanel CreateFooterBar()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = FooterHeight,
                Padding = new Padding(16, 10, 16, 12),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ModernTheme.Colors.Bg
            };
        }

        public static void AddFooterButton(FlowLayoutPanel footer, Button button)
        {
            SizeButton(button, Math.Max(96, MeasureButtonWidth(button.Text)));
            button.Margin = new Padding(10, 0, 0, 0);
            footer.Controls.Add(button);
        }

        /// <summary>Centra un control en el área disponible (p. ej. login).</summary>
        public static void AddCentered(TableLayoutPanel host, Control content, int maxWidth = 400)
        {
            host.Dock = DockStyle.Fill;
            host.ColumnCount = 3;
            host.RowCount = 1;
            host.BackColor = Color.Transparent;
            host.ColumnStyles.Clear();
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, maxWidth));
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            content.Dock = DockStyle.Fill;
            content.MaximumSize = new Size(maxWidth, 0);
            host.Controls.Add(content, 1, 0);
        }

        public static TableLayoutPanel CreateFormStack(int labelWidth = 0)
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                BackColor = ModernTheme.Colors.Bg,
                Padding = new Padding(8, 4, 8, 16)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            return t;
        }

        public static void StackRow(TableLayoutPanel stack, Control control, float height = 0)
        {
            int row = stack.RowCount++;
            stack.RowStyles.Add(height > 0
                ? new RowStyle(SizeType.Absolute, height)
                : new RowStyle(SizeType.AutoSize));
            control.Dock = DockStyle.Fill;
            stack.Controls.Add(control, 0, row);
        }

        public static void StackLabel(TableLayoutPanel stack, string text)
        {
            var lbl = ModernTheme.CreateLabel(text, ModernTheme.LabelStyle.Caption);
            lbl.Margin = new Padding(0, stack.RowCount > 0 ? 12 : 0, 0, 4);
            StackRow(stack, lbl);
        }

        public static Panel CreateButtonRow(Button button)
        {
            SizeButton(button);
            var row = new Panel
            {
                Dock = DockStyle.Fill,
                Height = ButtonHeight + 16,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = Color.Transparent
            };
            button.Dock = DockStyle.Left;
            button.Margin = Padding.Empty;
            row.Controls.Add(button);
            return row;
        }
    }
}
