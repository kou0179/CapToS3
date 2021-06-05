using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CapToS3
{
    public partial class CaptureAreaForm : Form
    {
        private readonly CursorSideForm _cursorSideForm = new();
        private readonly SelectedAreaForm _selectedAreaForm = new();
        private readonly Screen _leftMostScreen = Screen.AllScreens.OrderBy(_s => _s.Bounds.X).First();
        private readonly Screen _topMostScreen = Screen.AllScreens.OrderBy(_s => _s.Bounds.Y).First();
        private readonly int _screenWidth = Screen.AllScreens.Sum(s => s.Bounds.Width);
        private readonly int _screenHeight = Screen.AllScreens.Sum(s => s.Bounds.Height);
        private readonly IImageRepository _imageRepository;

        private int _cursorSideFormImageScale = 2;
        private bool _mouseClickHolding = false;
        public CaptureAreaForm(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;

            Hide();
            InitializeComponent();
            Show();
        }

        private void InitializeComponent()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(_leftMostScreen.Bounds.X, _topMostScreen.Bounds.Y);
            ClientSize = new Size(_screenWidth, _screenHeight);
            BackColor = Color.White;
            Opacity = .01;
            Cursor = Cursors.Cross;
            Text = "Form1";

            _cursorSideForm.Location = new Point(_leftMostScreen.Bounds.X, _topMostScreen.Bounds.Y);
            _cursorSideForm.Show();


            _selectedAreaForm.Opacity = 0.01;
            _selectedAreaForm.Show();
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!_mouseClickHolding)
            {
                _selectedAreaForm.Opacity = .25;
                _selectedAreaForm.Bounds = new Rectangle(new Point(_leftMostScreen.Bounds.X + e.X,
                _topMostScreen.Bounds.Y + e.Y), new Size(0, 0));
                _mouseClickHolding = true;
            }

            if (e.Button == MouseButtons.Right)
            {
                Close();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_mouseClickHolding) {
                _mouseClickHolding = false;

                _selectedAreaForm.Hide();
                Bitmap image = new(_selectedAreaForm.Size.Width, _selectedAreaForm.Size.Height);
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.CopyFromScreen(_selectedAreaForm.Bounds.X,
                        _selectedAreaForm.Bounds.Y,
                        0, 0,
                        _selectedAreaForm.Size);
                }
                image.Save("result.png", ImageFormat.Png);

                _cursorSideForm.Hide();
                Close();
                Clipboard.SetText(
                    _imageRepository.SaveAsync(image).GetAwaiter().GetResult()
                    );
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            CursorSideFormUpdate(e);
            if (_mouseClickHolding)
            {
                _selectedAreaForm.Width = _leftMostScreen.Bounds.X + e.X - _selectedAreaForm.Location.X;
                _selectedAreaForm.Height = _topMostScreen.Bounds.Y + e.Y - _selectedAreaForm.Location.Y;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            var newScale = _cursorSideFormImageScale + e.Delta / SystemInformation.MouseWheelScrollDelta;
            if (0 < newScale  && newScale <= 3)
            {
                _cursorSideFormImageScale = newScale;
                CursorSideFormUpdate(e);
            }
        }

        private void CursorSideFormUpdate(MouseEventArgs e)
        {
            int scale = _cursorSideFormImageScale;

            _cursorSideForm.Location = new Point(_leftMostScreen.Bounds.X + e.X + _cursorSideForm.Width / 4, 
                _topMostScreen.Bounds.Y + e.Y - _cursorSideForm.Height / 2);

            Bitmap image = new(_cursorSideForm.Bounds.Width / scale, _cursorSideForm.Bounds.Height / scale);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.CopyFromScreen(_leftMostScreen.Bounds.X + e.X - _cursorSideForm.Bounds.Width / scale / 2,
                    _topMostScreen.Bounds.Y + e.Y - _cursorSideForm.Bounds.Height / scale / 2,
                    0, 0,
                    new Size(_cursorSideForm.Bounds.Width / scale, _cursorSideForm.Bounds.Height / scale));
            }

            Bitmap scaledImage = new(_cursorSideForm.Bounds.Width, _cursorSideForm.Bounds.Height);
            using (Graphics g = Graphics.FromImage(scaledImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(image,
                    0, 0, _cursorSideForm.Bounds.Width, _cursorSideForm.Bounds.Height);
                g.DrawLine(new Pen(Brushes.Blue), new Point(0, scaledImage.Height / 2), new Point(scaledImage.Width, scaledImage.Height / 2));
                g.DrawLine(new Pen(Brushes.Blue), new Point(scaledImage.Height / 2, 0), new Point(scaledImage.Height / 2, scaledImage.Width));
                g.DrawString($"{e.X} , {e.Y} (x{_cursorSideFormImageScale})", new Font(new FontFamily("Arial"), 8), Brushes.Pink, 0, 0);
            }
            _cursorSideForm.BackgroundImage = scaledImage;
#if DEBUG
            image.Save("test.bmp");
#endif
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Refresh();
        }
    }
}