using System.Drawing;
using System.Windows.Forms;

namespace EmpresaApp.Utils
{
    public static class MinimalUi
    {
        public static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            ModernTheme.StyleDataGridView(grid);
            return grid;
        }

        public static FlowLayoutPanel CreateTopBar() => UiLayout.CreateTopBar();

        public static void AddToBar(FlowLayoutPanel bar, Control control) =>
            UiLayout.AddBarControl(bar, control);

        public static Button CreateButton(string text, bool primary = false)
        {
            var btn = ModernTheme.CreateButton(text,
                primary ? ModernTheme.ButtonVariant.Primary : ModernTheme.ButtonVariant.Secondary);
            UiLayout.SizeButton(btn);
            return btn;
        }

        public static Label CreateLabel(string text, ModernTheme.LabelStyle style = ModernTheme.LabelStyle.Body) =>
            ModernTheme.CreateLabel(text, style);

        public static TextBox CreateTextBox(bool multiline = false, bool password = false)
        {
            var txt = new TextBox
            {
                Multiline = multiline,
                UseSystemPasswordChar = password,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ModernTheme.Colors.Surface,
                ForeColor = ModernTheme.Colors.Text,
                Font = ModernTheme.FontBody
            };
            if (multiline)
            {
                txt.ScrollBars = ScrollBars.Vertical;
                txt.MinimumSize = new Size(200, 100);
            }
            else
            {
                txt.Height = UiLayout.ControlHeight;
            }
            return txt;
        }

        public static ListBox CreateListBox()
        {
            var list = new ListBox { IntegralHeight = false, BorderStyle = BorderStyle.None };
            ModernTheme.StyleListBox(list);
            return list;
        }

        public static Panel CreateCard() => ModernTheme.CreateCard();
    }
}
