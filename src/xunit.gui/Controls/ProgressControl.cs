using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Xunit.Gui
{
    public partial class ProgressControl : UserControl, IDisposable
    {
        int barValue = 50;
        int minimum = 0;
        int maximum = 100;
        ProgressStatus status = ProgressStatus.Unknown;

        public ProgressControl()
        {
            InitializeComponent();

            DoubleBuffered = true;
        }

        Color DarkBackColor
        {
            get { return Color.FromArgb(160, 160, 160); }
        }

        Color DarkFillColor
        {
            get
            {
                if (status == ProgressStatus.Passing)
                    return Color.FromArgb(0, 128, 0);
                if (status == ProgressStatus.Failing)
                    return Color.FromArgb(128, 0, 0);
                if (status == ProgressStatus.Skipping)
                    return Color.FromArgb(128, 128, 0);
                if (status == ProgressStatus.Cancelled)
                    return Color.FromArgb(0, 0, 128);

                return Color.FromArgb(192, 96, 0);
            }
        }

        protected override Size DefaultSize
        {
            get { return new Size(200, 23); }
        }

        Color LightBackColor
        {
            get { return Color.FromArgb(252, 252, 252); }
        }

        Color LightFillColor
        {
            get
            {
                if (status == ProgressStatus.Passing)
                    return Color.FromArgb(205, 255, 205);
                if (status == ProgressStatus.Failing)
                    return Color.FromArgb(255, 205, 205);
                if (status == ProgressStatus.Skipping)
                    return Color.FromArgb(255, 255, 205);
                if (status == ProgressStatus.Cancelled)
                    return Color.FromArgb(205, 205, 255);

                return Color.FromArgb(255, 230, 205);
            }
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        public int Minimum
        {
            get { return minimum; }
            set
            {
                if (value >= Maximum)
                    throw new ArgumentException("Minimum must be less than Maximum");

                minimum = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(100)]
        public int Maximum
        {
            get { return maximum; }
            set
            {
                if (value <= Minimum)
                    throw new ArgumentException("Maximum must be greater than Minimum");

                maximum = value;
                Invalidate();
            }
        }

        double Offset
        {
            get { return Math.Min(Value - Minimum, Range); }
        }

        double Range
        {
            get { return Math.Abs(Maximum - Minimum); }
        }

        [Category("Behavior")]
        [DefaultValue(ProgressStatus.Passing)]
        public ProgressStatus Status
        {
            get { return status; }
            set { status = value; Invalidate(); }
        }

        [Category("Behavior")]
        [DefaultValue(50)]
        public int Value
        {
            get { return barValue; }
            set
            {
                if (value < Minimum || value > Maximum)
                    throw new ArgumentException("Value must be between Minimum and Maximum");

                barValue = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, Width, Height);

            using (Brush borderBrush = new SolidBrush(Color.FromName("ActiveBorder")))
                e.Graphics.FillRectangle(borderBrush, rect);

            rect.Offset(1, 1);
            rect.Width -= 2;
            rect.Height -= 2;

            using (Brush backBrush = new LinearGradientBrush(rect,
                                                             LightBackColor,
                                                             DarkBackColor,
                                                             LinearGradientMode.Vertical))
                e.Graphics.FillRectangle(backBrush, rect);

            if (Range > 0.0)
            {
                rect.Width = (int)((double)rect.Width * Offset / Range);

                if (rect.Width > 0)
                    using (Brush fillBrush = new LinearGradientBrush(rect,
                                                                     LightFillColor,
                                                                     DarkFillColor,
                                                                     LinearGradientMode.Vertical))
                        e.Graphics.FillRectangle(fillBrush, rect);
            }
        }

        public void Increment()
        {
            if (Value < Maximum)
                Value++;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Invalidate();
        }

        public enum ProgressStatus
        {
            Passing,
            Skipping,
            Failing,
            Cancelled,
            Unknown,
        }
    }
}