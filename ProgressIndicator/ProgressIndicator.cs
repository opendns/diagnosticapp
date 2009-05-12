using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace ProgressControls
{
    /// <summary>
    /// Firefox like circular progress indicator.
    /// </summary>
    public partial class ProgressIndicator : Control
    {
        #region Constructor

        /// <summary>
        /// Default constructor for the ProgressIndicator.
        /// </summary>
        public ProgressIndicator()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            ResizeRedraw = true;

            if (AutoStart)
                timerAnimation.Start();
        }

        #endregion

        #region Private Fields

        private int _value = 1;

        private int _interval = 100;

        private Color _circleColor = Color.FromArgb(20, 20, 20);

        private bool _autoStart;

        private bool _stopped = true;

        private float _circleSize = 1.0F;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the base color for the circles.
        /// </summary>
        [DefaultValue(typeof(Color), "20, 20, 20")]
        [Description("Gets or sets the base color for the circles.")]
        [Category("Appearance")]
        public Color CircleColor
        {
            get { return _circleColor; }
            set
            {
                _circleColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the animation should start automatically.
        /// </summary>
        [DefaultValue(false)]
        [Description("Gets or sets a value indicating if the animation should start automatically.")]
        [Category("Behavior")]
        public bool AutoStart
        {
            get { return _autoStart; }
            set
            {
                _autoStart = value;

                if (_autoStart && !DesignMode)
                    Start();
                else 
                    Stop();
            }
        }

        /// <summary>
        /// Gets or sets the scale for the circles raging from 0.1 to 1.0.
        /// </summary>
        [DefaultValue(1.0F)]
        [Description("Gets or sets the scale for the circles raging from 0.1 to 1.0.")]
        [Category("Appearance")]
        public float CircleSize
        {
            get { return _circleSize; }
            set
            {
                if (value <= 0.0F)
                    _circleSize = 0.1F;
                else 
                    _circleSize = value > 1.0F ? 1.0F : value;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the animation speed.
        /// </summary>
        [DefaultValue(75)]
        [Description("Gets or sets the animation speed.")]
        [Category("Behavior")]
        public int AnimationSpeed
        {
            get { return (-_interval + 400) / 4; }
            set
            {
                checked
                {
                    int interval = 400 - (value * 4);

                    if (interval < 10)
                        _interval = 10;
                    else
                        _interval = interval > 400 ? 400 : interval;

                    timerAnimation.Interval = _interval;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the animation.
        /// </summary>
        public void Start()
        {
            timerAnimation.Interval = _interval;
            _stopped = false;
            timerAnimation.Start();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            timerAnimation.Stop();
            _value = 1;
            _stopped = true;
            Invalidate();
        }

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            const float angle = 360.0F / 8;

            GraphicsState oldState = e.Graphics.Save();

            e.Graphics.TranslateTransform(Width / 2.0F, Height / 2.0F);
            e.Graphics.RotateTransform(angle * _value);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 1; i <= 8; i++)
            {
                int alpha = _stopped ? (int)(255.0F * (1.0F / 8.0F)) : (int)(255.0F * (i / 8.0F));

                Color drawColor = Color.FromArgb(alpha, _circleColor);

                using (SolidBrush brush = new SolidBrush(drawColor))
                {
                    float sizeRate = 4.5F / _circleSize;
                    float size = Width / sizeRate;

                    float diff = (Width / 4.5F) - size;

                    float x = (Width / 9.0F) + diff;
                    float y = (Height / 9.0F) + diff;
                    e.Graphics.FillEllipse(brush, x, y, size, size);
                    e.Graphics.RotateTransform(angle);
                }
            }

            e.Graphics.Restore(oldState);
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            SetNewSize();
            base.OnResize(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            SetNewSize();
            base.OnSizeChanged(e);
        }

        #endregion

        #region Private Methods

        private void SetNewSize()
        {
            int size = Math.Max(Width, Height);
            Size = new Size(size, size);
        }

        private void IncreaseValue()
        {
            if (_value + 1 <= 8)
                _value++;
            else
                _value = 1;
        }

        #endregion

        #region Timer

        private void timerAnimation_Tick(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                IncreaseValue();
                Invalidate();
            }
        }

        #endregion
    }
}
