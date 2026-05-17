// ───────────────────────────────────────────────────────────────
//  ModernTheme.cs - Premium Dark UI theme with transitions
// ───────────────────────────────────────────────────────────────
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EmpresaApp.Utils
{
    /// <summary>
    /// Premium Dark Theme with smooth animations and rounded components
    /// </summary>
    public static class ModernTheme
    {
        // Premium Dark Color palette
        public static class Colors
        {
            public static Color Primary = Color.FromArgb(99, 102, 241);      // Indigo (Accent)
            public static Color PrimaryGradient = Color.FromArgb(6, 182, 212); // Cyan
            public static Color Secondary = Color.FromArgb(148, 163, 184);   // Slate 400
            public static Color Success = Color.FromArgb(16, 185, 129);      // Emerald
            public static Color Warning = Color.FromArgb(245, 158, 11);      // Amber
            public static Color Danger = Color.FromArgb(239, 68, 68);        // Red
            
            // Dark Mode Suraces
            public static Color Background = Color.FromArgb(11, 17, 32);     // Slate 950 (Deep Background)
            public static Color CardBackground = Color.FromArgb(30, 41, 59); // Slate 800 (Surface)
            public static Color Light = Color.FromArgb(51, 65, 85);          // Slate 700 (Input / Hover)
            public static Color Border = Color.FromArgb(71, 85, 105);        // Slate 600 (Borders)
            
            // Text Colors
            public static Color TextPrimary = Color.FromArgb(248, 250, 252); // Slate 50
            public static Color TextSecondary = Color.FromArgb(148, 163, 184);// Slate 400
            
            // Legacy mapping properties to keep compatibility without breaking too much
            public static Color Dark = Color.FromArgb(248, 250, 252); // Repurposed for primary text
        }

        /// <summary>
        /// Creates a rounded rectangle path
        /// </summary>
        public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r2 = radius / 2f;
            int xw = rect.X + rect.Width;
            int yh = rect.Y + rect.Height;
            int xvw = xw - radius;
            int yvh = yh - radius;

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(xvw, rect.Y, radius, radius, 270, 90);
            path.AddArc(xvw, yvh, radius, radius, 0, 90);
            path.AddArc(rect.X, yvh, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Apply premium button hover effect with rounded corners
        /// </summary>
        public static void ApplyModernButtonStyle(Button btn, Color baseColor, int radius = 8)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Colors.TextPrimary;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            bool isHovering = false;

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                using (var path = GetRoundedPath(rect, radius))
                {
                    // Gradient fill if Primary
                    if (baseColor == Colors.Primary)
                    {
                        using (var brush = new LinearGradientBrush(rect, Colors.Primary, Colors.PrimaryGradient, 45f))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        using (var brush = new SolidBrush(isHovering ? ControlPaint.Light(baseColor, 0.2f) : baseColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }

                    if (isHovering && baseColor == Colors.Primary)
                    {
                        using (var glowBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                        {
                            e.Graphics.FillPath(glowBrush, path);
                        }
                    }
                }

                // Draw Text
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, rect, btn.ForeColor, 
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            };

            btn.MouseEnter += (s, e) =>
            {
                isHovering = true;
                btn.Invalidate();
            };
            btn.MouseLeave += (s, e) =>
            {
                isHovering = false;
                btn.Invalidate();
            };
        }

        /// <summary>
        /// Apply modern card style with shadow effect
        /// </summary>
        public static void ApplyModernCardStyle(Panel panel, int radius = 12)
        {
            panel.BackColor = Color.Transparent;
            
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                
                using (var path = GetRoundedPath(rect, radius))
                {
                    using (var brush = new SolidBrush(Colors.CardBackground))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    using (var pen = new Pen(Colors.Border, 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };
        }

        /// <summary>
        /// Apply modern text input style
        /// </summary>
        public static void ApplyModernTextBoxStyle(TextBox txt)
        {
            txt.BorderStyle = BorderStyle.None;
            txt.Font = new Font("Segoe UI", 11);
            txt.BackColor = Colors.Light;
            txt.ForeColor = Colors.TextPrimary;

            // We wrap it in a custom paint panel visually in forms, 
            // but for the control itself we just set colors
            txt.MouseEnter += (s, e) => { txt.BackColor = ControlPaint.Light(Colors.Light, 0.1f); };
            txt.MouseLeave += (s, e) => { txt.BackColor = Colors.Light; };
        }

        /// <summary>
        /// Create animated fade-in effect
        /// </summary>
        public static void AnimateFadeIn(Control control, int durationMs = 300)
        {
            control.Visible = false;
            control.Visible = true;
            var timer = new System.Windows.Forms.Timer();
            int elapsed = 0;
            int steps = 30;
            int stepDuration = durationMs / steps;

            timer.Interval = stepDuration;
            timer.Tick += (s, e) =>
            {
                elapsed += stepDuration;
                float progress = Math.Min(1f, (float)elapsed / durationMs);

                // Simulate fade with alpha in BackColor
                if (control.BackColor != Color.Transparent)
                {
                    control.Refresh();
                }

                if (progress >= 1f)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Create animated slide-in effect
        /// </summary>
        public static void AnimateSlideIn(Control control, int fromX, int duration = 300)
        {
            int targetX = control.Left;
            int startX = fromX;
            var timer = new System.Windows.Forms.Timer();
            int elapsed = 0;
            int steps = 30;

            timer.Interval = duration / steps;
            timer.Tick += (s, e) =>
            {
                elapsed += timer.Interval;
                float progress = Math.Min(1f, (float)elapsed / duration);

                // Easing function (ease-out)
                progress = 1f - (float)Math.Pow(1f - progress, 3f);

                control.Left = (int)(startX + (targetX - startX) * progress);

                if (progress >= 1f)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Apply modern group box style
        /// </summary>
        public static void ApplyModernGroupBoxStyle(GroupBox gb)
        {
            gb.ForeColor = Colors.TextPrimary;
            gb.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gb.Paint += (s, e) =>
            {
                var size = e.Graphics.MeasureString(gb.Text, gb.Font);
                var borderRect = new Rectangle(0, (int)(size.Height / 2),
                    gb.Width - 1, gb.Height - (int)(size.Height / 2) - 1);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedPath(borderRect, 8))
                {
                    using (var pen = new Pen(Colors.Border, 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                var textRect = new Rectangle(10, 0, (int)size.Width + 4, (int)size.Height);
                e.Graphics.FillRectangle(new SolidBrush(gb.BackColor), textRect);
                e.Graphics.DrawString(gb.Text, gb.Font,
                    new SolidBrush(gb.ForeColor), textRect);
            };
        }

        /// <summary>
        /// Create a modern progress bar
        /// </summary>
        public static void ApplyModernProgressBarStyle(ProgressBar pb)
        {
            pb.BackColor = Colors.Light;
            pb.ForeColor = Colors.Primary;
            pb.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, pb.Width - 1, pb.Height - 1);
                
                using (var path = GetRoundedPath(rect, pb.Height / 2))
                {
                    e.Graphics.FillPath(new SolidBrush(Colors.Light), path);
                    
                    float percentage = (float)pb.Value / (pb.Maximum - pb.Minimum);
                    int width = (int)(pb.Width * percentage);

                    if (width > 0)
                    {
                        var fillRect = new Rectangle(0, 0, width, pb.Height - 1);
                        using (var fillPath = GetRoundedPath(fillRect, pb.Height / 2))
                        {
                            using (var brush = new LinearGradientBrush(rect, Colors.Primary, Colors.PrimaryGradient, 0f))
                            {
                                e.Graphics.FillPath(brush, fillPath);
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create pulse animation for notifications
        /// </summary>
        public static void AnimatePulse(Control control, int durationMs = 1500)
        {
            var originalForeColor = control.ForeColor;
            var timer = new System.Windows.Forms.Timer();
            int elapsed = 0;

            timer.Interval = 50;
            timer.Tick += (s, e) =>
            {
                elapsed += timer.Interval;
                float progress = ((float)(elapsed % (durationMs / 2)) / (durationMs / 2));

                int r = originalForeColor.R;
                int g = originalForeColor.G;
                int b = originalForeColor.B;

                control.ForeColor = Color.FromArgb(
                    (int)(r + (255 - r) * progress * 0.3f),
                    (int)(g + (255 - g) * progress * 0.3f),
                    (int)(b + (255 - b) * progress * 0.3f)
                );
            };
            timer.Start();
        }

        /// <summary>
        /// Create smooth size transition
        /// </summary>
        public static void AnimateSizeChange(Control control, Size targetSize, int durationMs = 300)
        {
            Size startSize = control.Size;
            var timer = new System.Windows.Forms.Timer();
            int elapsed = 0;
            int steps = 30;

            timer.Interval = durationMs / steps;
            timer.Tick += (s, e) =>
            {
                elapsed += timer.Interval;
                float progress = Math.Min(1f, (float)elapsed / durationMs);

                // Easing
                progress = progress < 0.5f
                    ? 2 * progress * progress
                    : 1 - (float)Math.Pow(-2 * progress + 2, 2) / 2;

                control.Width = (int)(startSize.Width + (targetSize.Width - startSize.Width) * progress);
                control.Height = (int)(startSize.Height + (targetSize.Height - startSize.Height) * progress);

                if (progress >= 1f)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Apply modern label style
        /// </summary>
        public static void ApplyModernLabelStyle(Label lbl, LabelStyle style = LabelStyle.Normal)
        {
            lbl.Font = style switch
            {
                LabelStyle.Title => new Font("Segoe UI", 22, FontStyle.Bold),
                LabelStyle.Heading => new Font("Segoe UI", 16, FontStyle.Bold),
                LabelStyle.Subheading => new Font("Segoe UI", 12, FontStyle.Bold),
                LabelStyle.Normal => new Font("Segoe UI", 10),
                LabelStyle.Small => new Font("Segoe UI", 8.5f),
                _ => new Font("Segoe UI", 10)
            };

            lbl.ForeColor = style switch
            {
                LabelStyle.Title or LabelStyle.Heading or LabelStyle.Subheading => Colors.TextPrimary,
                LabelStyle.Normal => Colors.TextPrimary,
                LabelStyle.Small => Colors.TextSecondary,
                _ => Colors.TextPrimary
            };
        }

        public enum LabelStyle { Title, Heading, Subheading, Normal, Small }
    }
}
