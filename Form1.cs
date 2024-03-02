using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keraliz
{
    public partial class Keraliz : Form
    {
        private Rectangle captureArea;
        private Timer captureTimer;
        private Bitmap capturedImage;
        private PictureBox pictureBox;

        private bool definingCaptureArea = true;
        private bool follow_mouse = true;
        private float zoomLevel = 1.0f;
        private float zoomStep = 0.2f;

        int last_mouse_position_x = 0;

        public Keraliz()
        {
            InitializeComponent();
            InitializePictureBox();
        }

        private void InitializePictureBox()
        {
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill; 
            Controls.Add(pictureBox); 
        }


        private void CaptureTimer_Tick(object sender, EventArgs e)
        {
            capturedImage = CaptureScreenArea(captureArea);
            Invalidate();


            Screen CursorScreen = Screen.FromPoint(Cursor.Position);

            if (follow_mouse && Cursor.Position.X != last_mouse_position_x && CursorScreen.Primary)
            {
                Console.WriteLine("following Cursor..");
                //si el zoom es 1.0 el multiplicador X es 0.315 y el Y 0.3
                int x = (int)(Cursor.Position.X*(zoomLevel*0.415)-1);
                last_mouse_position_x = Cursor.Position.X;
                int y = (int)(Cursor.Position.Y*(zoomLevel * 0.4));
                int width = captureArea.Width;
                int height = captureArea.Height;
                captureArea = new Rectangle(x, y, width, height);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartFullScreenCapture();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            if (follow_mouse) {
                zoomLevel = 1.3f;
            }

            captureTimer = new Timer();
            captureTimer.Interval = 60;
            captureTimer.Tick += CaptureTimer_Tick;
            captureTimer.Start();

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (capturedImage != null)
            {

                int scaledWidth = (int)(capturedImage.Width * zoomLevel);
                int scaledHeight = (int)(capturedImage.Height * zoomLevel);

                int x = (ClientSize.Width - scaledWidth) / 2;
                int y = (ClientSize.Height - scaledHeight) / 2;
                try
                {
                    pictureBox.Image = new Bitmap(capturedImage, new Size(scaledWidth, scaledHeight));
                    System.GC.Collect();

                } catch {
                
                }

            }
        }


        private void StartFullScreenCapture()
        {
           // definingCaptureArea = false;
            Screen primaryScreen = Screen.PrimaryScreen;
            captureArea = primaryScreen.Bounds;

            if (Screen.FromControl(this).Equals(primaryScreen))
            {
                Screen secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Equals(primaryScreen));

                if (secondaryScreen != null)
                {
                    this.Location = new Point(secondaryScreen.Bounds.X, secondaryScreen.Bounds.Y);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int stepSize = 40;

            if (definingCaptureArea)
            {
                int x = captureArea.X;
                int y = captureArea.Y;
                int width = captureArea.Width;
                int height = captureArea.Height;

                Screen secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);


                Screen primaryScreen = Screen.PrimaryScreen;
                //Rectangle bounds = primaryScreen.WorkingArea;

               // int x_limit = Screen.PrimaryScreen.Bounds.Width - secondaryScreen.Bounds.Width;
               // Console.WriteLine("x:"+x_limit);


                switch (e.KeyCode)
                {
                    case Keys.W:
                        y = Math.Max(0, y - stepSize);
                        break;
                    case Keys.S:
                        //primary height - secondary height
                        if (y < 400*zoomLevel)
                        {
                            y += stepSize;
                            Console.WriteLine(y);
                        }
                        break;
                    case Keys.A:
                        x = Math.Max(0, x - stepSize);
                        break;
                    case Keys.D:
                        //primary width - secondary width
                        if (x < 640*zoomLevel)
                        {
                            x += stepSize;
                        }
                        Console.WriteLine(x);
                        break;
                }
                //2560x1440
                //1920x1080
                //640 360

                switch (e.KeyCode)
                {
                    case Keys.Up:
                        zoomLevel += zoomStep;
                        Console.WriteLine("Increasing zoom");
                        break;
                    case Keys.Down:
                        zoomLevel -= zoomStep;
                        if (zoomLevel < zoomStep)
                            zoomLevel = zoomStep;
                        Console.WriteLine("Decreasing zoom");
                        break;
                }
                captureArea = new Rectangle(x, y, width, height);
                Invalidate();
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private Bitmap CaptureScreenArea(Rectangle area)
        {
            try
            {
                Bitmap bitmap = new Bitmap(area.Width, area.Height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(area.Left, area.Top, 0, 0, area.Size);
                }
                return bitmap;
            }
            catch (Exception ex) {
                return null;
            }
        }
            

    }
}
