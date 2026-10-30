using System.Drawing;
using System.Windows.Forms;

namespace regexFinder
{
    internal static class AppTheme
    {
        internal static readonly Color Canvas = Color.FromArgb(245, 247, 249);
        internal static readonly Color Ink = Color.FromArgb(27, 39, 51);
        internal static readonly Color Muted = Color.FromArgb(82, 98, 112);
        internal static readonly Color Accent = Color.FromArgb(24, 88, 145);
        internal static readonly Color Border = Color.FromArgb(192, 202, 212);

        internal static void StyleButton(Button button, bool primary = false)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = primary ? Accent : Border;
            button.BackColor = primary ? Accent : Color.White;
            button.ForeColor = primary ? Color.White : Ink;
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(18, 71, 119) : Color.FromArgb(232, 239, 246);
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(14, 56, 95) : Color.FromArgb(216, 228, 240);
            button.UseVisualStyleBackColor = false;
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.MinimumSize = new Size(0, 32);
            button.Padding = new Padding(10, 3, 10, 3);
        }

        internal static Label Label(string text, float size = 10, bool bold = false)
        {
            return new Label {
                Text = text, AutoSize = true, Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = bold ? Ink : Muted, Margin = new Padding(0, 0, 0, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        internal static void Apply(Control root)
        {
            foreach (Control control in root.Controls)
            {
                if (control is Button button) StyleButton(button);
                if (control is TextBox box) box.BorderStyle = BorderStyle.FixedSingle;
                if (control is ListBox list) list.BorderStyle = BorderStyle.FixedSingle;
                Apply(control);
            }
        }
    }
}
