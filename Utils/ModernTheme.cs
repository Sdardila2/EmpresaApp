using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace EmpresaApp.Utils
{
    /// <summary>
    /// Sistema de diseño oscuro — zinc + violeta, sin gradientes recargados.
    /// </summary>
    public static class ModernTheme
    {
        public static readonly Font FontTitle = new("Segoe UI Semibold", 22f);
        public static readonly Font FontHeading = new("Segoe UI Semibold", 15f);
        public static readonly Font FontSubheading = new("Segoe UI Semibold", 11f);
        public static readonly Font FontBody = new("Segoe UI", 10f);
        public static readonly Font FontCaption = new("Segoe UI", 9f);
        public static readonly Font FontButton = new("Segoe UI Semibold", 10f);

        public static class Colors
        {
            public static readonly Color Bg = Color.FromArgb(9, 9, 11);
            public static readonly Color Surface = Color.FromArgb(24, 24, 27);
            public static readonly Color SurfaceHover = Color.FromArgb(39, 39, 42);
            public static readonly Color Elevated = Color.FromArgb(33, 33, 38);
            public static readonly Color Border = Color.FromArgb(63, 63, 70);
            public static readonly Color BorderFocus = Color.FromArgb(139, 92, 246);

            public static readonly Color Accent = Color.FromArgb(139, 92, 246);
            public static readonly Color AccentHover = Color.FromArgb(167, 139, 250);
            public static readonly Color AccentMuted = Color.FromArgb(46, 36, 78);

            public static readonly Color Text = Color.FromArgb(250, 250, 250);
            public static readonly Color TextMuted = Color.FromArgb(161, 161, 170);
            public static readonly Color TextDim = Color.FromArgb(113, 113, 122);

            public static readonly Color Success = Color.FromArgb(34, 197, 94);
            public static readonly Color Warning = Color.FromArgb(234, 179, 8);
            public static readonly Color Danger = Color.FromArgb(239, 68, 68);
            public static readonly Color DangerMuted = Color.FromArgb(69, 26, 26);

            // Compatibilidad con código existente
            public static Color Background => Bg;
            public static Color Sidebar => Surface;
            public static Color CardBackground => Elevated;
            public static Color Light => SurfaceHover;
            public static Color Primary => Accent;
            public static Color PrimaryGradient => AccentHover;
            public static Color TextPrimary => Text;
            public static Color TextSecondary => TextMuted;
            public static Color Dark => Text;
        }

        public static void ApplyToForm(Form form, bool fadeIn = true)
        {
            form.BackColor = Colors.Bg;
            form.ForeColor = Colors.Text;
            form.Font = FontBody;
            EnableDoubleBuffer(form);
            if (fadeIn)
                form.Load += (_, _) => AnimateFormFadeIn(form, 220);
        }

        public static void ApplyToUserControl(UserControl uc)
        {
            uc.BackColor = Colors.Bg;
            uc.ForeColor = Colors.Text;
            uc.Font = FontBody;
            EnableDoubleBuffer(uc);
        }

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(r); return path; }
            int d = radius * 2;
            int right = r.Right - d;
            int bottom = r.Bottom - d;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(right, r.Y, d, d, 270, 90);
            path.AddArc(right, bottom, d, d, 0, 90);
            path.AddArc(r.X, bottom, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Panel CreateInput(TextBox inner, int height = 44)
        {
            bool focused = false;
            var wrap = new Panel
            {
                Height = height,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 0, 14, 0)
            };
            inner.BorderStyle = BorderStyle.None;
            inner.BackColor = Colors.Surface;
            inner.ForeColor = Colors.Text;
            inner.Font = FontBody;
            inner.Dock = DockStyle.Fill;
            wrap.Controls.Add(inner);
            EnableDoubleBuffer(wrap);

            void Paint(object? _, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var box = new Rectangle(0, 0, wrap.Width - 1, wrap.Height - 1);
                using var path = RoundedRect(box, 8);
                using (var fill = new SolidBrush(Colors.Surface))
                    e.Graphics.FillPath(fill, path);
                var borderColor = focused ? Colors.BorderFocus : Colors.Border;
                using (var pen = new Pen(borderColor, focused ? 2f : 1f))
                    e.Graphics.DrawPath(pen, path);
            }

            wrap.Paint += Paint;
            inner.GotFocus += (_, _) => { focused = true; wrap.Invalidate(); };
            inner.LostFocus += (_, _) => { focused = false; wrap.Invalidate(); };
            return wrap;
        }

        public static Button CreateButton(string text, ButtonVariant variant = ButtonVariant.Primary)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                ForeColor = Colors.Text,
                Font = FontButton,
                Cursor = Cursors.Hand,
                Height = 40,
                AutoSize = false
            };
            bool hover = false;
            bool pressed = false;

            btn.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using var path = RoundedRect(r, 8);

                Color fill, border = Color.Transparent;
                Color text = Colors.Text;

                switch (variant)
                {
                    case ButtonVariant.Primary:
                        fill = pressed ? Colors.Accent : (hover ? Colors.AccentHover : Colors.Accent);
                        break;
                    case ButtonVariant.Danger:
                        fill = hover ? Color.FromArgb(120, 40, 40) : Colors.DangerMuted;
                        text = Color.FromArgb(252, 165, 165);
                        break;
                    case ButtonVariant.Ghost:
                        fill = hover ? Colors.SurfaceHover : Color.Transparent;
                        text = hover ? Colors.Text : Colors.TextMuted;
                        border = Colors.Border;
                        break;
                    default:
                        fill = hover ? Colors.SurfaceHover : Colors.Surface;
                        border = Colors.Border;
                        break;
                }

                using (var b = new SolidBrush(fill))
                    e.Graphics.FillPath(b, path);
                if (border.A > 0)
                    using (var p = new Pen(border))
                        e.Graphics.DrawPath(p, path);

                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, r, text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btn.MouseEnter += (_, _) => { hover = true; btn.Invalidate(); };
            btn.MouseLeave += (_, _) => { hover = false; pressed = false; btn.Invalidate(); };
            btn.MouseDown += (_, _) => { pressed = true; btn.Invalidate(); };
            btn.MouseUp += (_, _) => { pressed = false; btn.Invalidate(); };

            UiLayout.SizeButton(btn);
            return btn;
        }

        public static Button CreateWideButton(string text, int width, ButtonVariant variant = ButtonVariant.Primary)
        {
            var btn = CreateButton(text, variant);
            btn.Width = width;
            btn.Margin = new Padding(0, 12, 0, 0);
            return btn;
        }

        public static Button CreateNavItem(string text)
        {
            var btn = new Button
            {
                Text = "   " + text,
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                ForeColor = Colors.TextMuted,
                Font = FontBody,
                Height = 38,
                Width = 208,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 1, 0, 1)
            };

            bool hover = false;
            bool selected = false;

            btn.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 1, btn.Width - 1, btn.Height - 3);

                if (selected)
                {
                    using var bg = new SolidBrush(Colors.AccentMuted);
                    using var path = RoundedRect(r, 6);
                    e.Graphics.FillPath(bg, path);
                    using var bar = new SolidBrush(Colors.Accent);
                    e.Graphics.FillRectangle(bar, 0, 4, 3, btn.Height - 8);
                }
                else if (hover)
                {
                    using var bg = new SolidBrush(Colors.SurfaceHover);
                    using var path = RoundedRect(r, 6);
                    e.Graphics.FillPath(bg, path);
                }

                var tc = selected ? Colors.Text : (hover ? Colors.Text : Colors.TextMuted);
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, r, tc,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };

            btn.MouseEnter += (_, _) => { hover = true; btn.Invalidate(); };
            btn.MouseLeave += (_, _) => { hover = false; btn.Invalidate(); };
            btn.Tag = new NavState(() => selected, v => { selected = v; btn.Invalidate(); });
            return btn;
        }

        private sealed class NavState(Func<bool> get, Action<bool> set)
        {
            public bool Selected { get => get(); set => set(value); }
        }

        public static void SetNavSelected(Button btn, bool on)
        {
            if (btn.Tag is NavState s) s.Selected = on;
        }

        public static Panel CreateCard()
        {
            var p = new Panel { BackColor = Color.Transparent, Padding = new Padding(20) };
            EnableDoubleBuffer(p);
            p.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using var path = RoundedRect(r, 10);
                using var fill = new SolidBrush(Colors.Elevated);
                e.Graphics.FillPath(fill, path);
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawPath(pen, path);
            };
            return p;
        }

        public static Panel CreateStatTile(string label, string value, Color? accent = null)
        {
            var tile = new Panel
            {
                Size = new Size(160, 88),
                Margin = new Padding(0, 0, 12, 0),
                BackColor = Color.Transparent
            };
            var ac = accent ?? Colors.Accent;
            EnableDoubleBuffer(tile);
            tile.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, tile.Width - 1, tile.Height - 1);
                using var path = RoundedRect(r, 10);
                using var fill = new SolidBrush(Colors.Elevated);
                e.Graphics.FillPath(fill, path);
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawPath(pen, path);
                using var dot = new SolidBrush(ac);
                e.Graphics.FillEllipse(dot, 16, 16, 8, 8);
            };

            var lblVal = new Label
            {
                Text = value,
                Font = FontHeading,
                ForeColor = Colors.Text,
                Location = new Point(16, 28),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var lblKey = new Label
            {
                Text = label,
                Font = FontCaption,
                ForeColor = Colors.TextMuted,
                Location = new Point(16, 56),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            tile.Controls.Add(lblKey);
            tile.Controls.Add(lblVal);
            return tile;
        }

        public static Label CreateLabel(string text, LabelStyle style = LabelStyle.Body)
        {
            var lbl = new Label { Text = text, AutoSize = true, BackColor = Color.Transparent };
            ApplyLabel(lbl, style);
            return lbl;
        }

        public static void ApplyLabel(Label lbl, LabelStyle style)
        {
            (lbl.Font, lbl.ForeColor) = style switch
            {
                LabelStyle.Title => (FontTitle, Colors.Text),
                LabelStyle.Heading => (FontHeading, Colors.Text),
                LabelStyle.Subheading => (FontSubheading, Colors.Text),
                LabelStyle.Caption => (FontCaption, Colors.TextMuted),
                _ => (FontBody, Colors.Text)
            };
        }

        public static void ApplyModernLabelStyle(Label lbl, LabelStyle style) => ApplyLabel(lbl, style);

        public static void ApplyModernButtonStyle(Button btn, Color baseColor)
        {
            var variant = baseColor == Colors.Danger || baseColor == Colors.DangerMuted
                ? ButtonVariant.Danger
                : baseColor == Colors.Accent ? ButtonVariant.Primary
                : ButtonVariant.Secondary;
            bool hover = false, pressed = false;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Font = FontButton;
            btn.Cursor = Cursors.Hand;
            btn.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using var path = RoundedRect(r, 8);
                Color fill = variant switch
                {
                    ButtonVariant.Primary => pressed ? Colors.Accent : (hover ? Colors.AccentHover : Colors.Accent),
                    ButtonVariant.Danger => hover ? Color.FromArgb(120, 40, 40) : Colors.DangerMuted,
                    _ => hover ? Colors.SurfaceHover : Colors.Surface
                };
                using var b = new SolidBrush(fill);
                e.Graphics.FillPath(b, path);
                var tc = variant == ButtonVariant.Danger ? Color.FromArgb(252, 165, 165) : Colors.Text;
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, r, tc,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btn.MouseEnter += (_, _) => { hover = true; btn.Invalidate(); };
            btn.MouseLeave += (_, _) => { hover = false; pressed = false; btn.Invalidate(); };
            btn.MouseDown += (_, _) => { pressed = true; btn.Invalidate(); };
            btn.MouseUp += (_, _) => { pressed = false; btn.Invalidate(); };
        }

        public static void ApplyModernTextBoxStyle(TextBox txt)
        {
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = Colors.Surface;
            txt.ForeColor = Colors.Text;
            txt.Font = FontBody;
        }

        public static void ApplyModernCardStyle(Panel p, int _ = 10) { /* usa CreateCard */ }

        public static Panel WrapTextBox(TextBox txt, int _ = 8) => CreateInput(txt);

        public static Button CreateNavButton(string text, string _ = "") => CreateNavItem(text);

        public static void StyleDataGridView(DataGridView g)
        {
            g.BackgroundColor = Colors.Elevated;
            g.GridColor = Colors.Border;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersHeight = 40;
            g.RowTemplate.Height = 36;
            g.DefaultCellStyle.BackColor = Colors.Elevated;
            g.DefaultCellStyle.ForeColor = Colors.Text;
            g.DefaultCellStyle.SelectionBackColor = Colors.AccentMuted;
            g.DefaultCellStyle.SelectionForeColor = Colors.Text;
            g.DefaultCellStyle.Font = FontBody;
            g.ColumnHeadersDefaultCellStyle.BackColor = Colors.Surface;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Colors.TextMuted;
            g.ColumnHeadersDefaultCellStyle.Font = FontSubheading;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Colors.Surface;
            EnableDoubleBuffer(g);
        }

        public static void StyleListBox(ListBox list)
        {
            list.BackColor = Colors.Elevated;
            list.ForeColor = Colors.Text;
            list.BorderStyle = BorderStyle.None;
            list.Font = FontBody;
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = 40;

            list.DrawItem += (_, e) =>
            {
                if (e.Index < 0) return;
                bool sel = (e.State & DrawItemState.Selected) != 0;
                using var bg = new SolidBrush(sel ? Colors.AccentMuted : Colors.Elevated);
                e.Graphics.FillRectangle(bg, e.Bounds);
                if (sel)
                {
                    using var bar = new SolidBrush(Colors.Accent);
                    e.Graphics.FillRectangle(bar, e.Bounds.X, e.Bounds.Y + 8, 3, e.Bounds.Height - 16);
                }
                var text = list.Items[e.Index]?.ToString() ?? "";
                var textRect = new Rectangle(e.Bounds.X + 14, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, text, list.Font, textRect, Colors.Text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            };
        }

        public static void StyleComboBox(ComboBox cbo)
        {
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.BackColor = Colors.Surface;
            cbo.ForeColor = Colors.Text;
            cbo.Font = FontBody;
        }

        public static void StyleDateTimePicker(DateTimePicker dtp)
        {
            dtp.CalendarMonthBackground = Colors.Elevated;
            dtp.CalendarForeColor = Colors.Text;
            dtp.CalendarTitleBackColor = Colors.Surface;
            dtp.CalendarTitleForeColor = Colors.Text;
            dtp.BackColor = Colors.Surface;
            dtp.ForeColor = Colors.Text;
            dtp.Font = FontBody;
        }

        public static void StyleTabControl(TabControl tabs)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(100, 36);
            tabs.Padding = new Point(12, 6);
            tabs.BackColor = Colors.Bg;

            tabs.DrawItem += (_, e) =>
            {
                bool sel = e.Index == tabs.SelectedIndex;
                var rect = e.Bounds;
                using var bg = new SolidBrush(sel ? Colors.Surface : Colors.Bg);
                e.Graphics.FillRectangle(bg, rect);
                if (sel)
                {
                    using var accent = new SolidBrush(Colors.Accent);
                    e.Graphics.FillRectangle(accent, rect.X + 8, rect.Bottom - 2, rect.Width - 16, 2);
                }
                var tc = sel ? Colors.Text : Colors.TextMuted;
                TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, rect, tc,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
        }

        public static void StyleSplitContainer(SplitContainer split)
        {
            split.BackColor = Colors.Bg;
            split.Panel1.BackColor = Colors.Bg;
            split.Panel2.BackColor = Colors.Bg;
        }

        public static void SwapContent(Panel host, Control content)
        {
            content.Dock = DockStyle.Fill;
            host.Controls.Clear();
            host.Controls.Add(content);
        }

        public static void AnimateFormFadeIn(Form form, int ms = 220)
        {
            form.Opacity = 0;
            var timer = new System.Windows.Forms.Timer { Interval = 16 };
            int elapsed = 0;
            timer.Tick += (_, _) =>
            {
                elapsed += 16;
                float t = Math.Min(1f, (float)elapsed / ms);
                form.Opacity = 1f - (float)Math.Pow(1 - t, 2);
                if (t >= 1f) { form.Opacity = 1; timer.Stop(); timer.Dispose(); }
            };
            timer.Start();
        }

        public static void EnableDoubleBuffer(Control c)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, c, new object[] { true });
        }

        public enum LabelStyle { Title, Heading, Subheading, Body, Caption, Normal, Small }
        public enum ButtonVariant { Primary, Secondary, Ghost, Danger }

        // Obsoletos — no usar
        public static void AnimateContentSwap(Panel h, Control c) => SwapContent(h, c);
        public static void AnimateSlideIn(Control c, int _, int __ = 300) { }
    }
}
