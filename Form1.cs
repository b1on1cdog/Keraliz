using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;

namespace Keraliz
{
    public class AppSettings
    {
        public SerializableDictionary<string, string> appPreferences { get; set; }
        public SerializableDictionary<float, int> horizontalLimit { get; set; }
        public SerializableDictionary<float, int> verticalLimit { get; set; }

        public AppSettings()
        {
            appPreferences = new SerializableDictionary<string, string>();
            horizontalLimit = new SerializableDictionary<float, int>();
            verticalLimit = new SerializableDictionary<float, int>();
        }

        public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
        {
            public XmlSchema GetSchema() => null;

            public void ReadXml(XmlReader reader)
            {
                var keySerializer = new XmlSerializer(typeof(TKey));
                var valueSerializer = new XmlSerializer(typeof(TValue));

                bool wasEmpty = reader.IsEmptyElement;
                reader.Read();

                if (wasEmpty)
                    return;

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("Item");

                    reader.ReadStartElement("Key");
                    TKey key = (TKey)keySerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    reader.ReadStartElement("Value");
                    TValue value = (TValue)valueSerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    this.Add(key, value);

                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }

            public void WriteXml(XmlWriter writer)
            {
                var keySerializer = new XmlSerializer(typeof(TKey));
                var valueSerializer = new XmlSerializer(typeof(TValue));

                foreach (TKey key in this.Keys)
                {
                    writer.WriteStartElement("Item");

                    writer.WriteStartElement("Key");
                    keySerializer.Serialize(writer, key);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Value");
                    TValue value = this[key];
                    valueSerializer.Serialize(writer, value);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
            }

        }
    }

    public class kDisplay
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Dpi { get; set; }
        public decimal scaleF { get; set; }
        public int scaleFactor { get; set; }
        public string Resolution { get; set; }
    }

    public partial class Keraliz : Form
    {
        private Rectangle captureArea;
        private Timer captureTimer;
        private Bitmap capturedImage;
        private PictureBox pictureBox;

        private bool definingCaptureArea = true;
        private bool follow_mouse = true;
        private float zoomLevel = 1.2f;
        private float zoomStep = 0.2f;
        private float zoomMin = 1.2f;
        private float zoomMax = 2.4f;
        private int cursor_correction_x = -80;
        private int cursor_correction_y = -80;

      //  private int cursor_acceleration_x = 0;
       // private int cursor_acceleration_y = 0;

        int last_mouse_position_x = 0;

        private kDisplay primaryDisplay = new kDisplay();
        private kDisplay secondaryDisplay = new kDisplay();

        private Screen primaryScreen = Screen.PrimaryScreen;
        private Screen secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);

        public Keraliz()
        {
            InitializeComponent();
            InitializePictureBox();
        }

        private void InitializePictureBox()
        {
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.BorderStyle = BorderStyle.None;
            Controls.Add(pictureBox); 
        }

      //  int max_location_x = 0;
       // int max_location_y = 0;

        Dictionary<float, int> limit_x = new Dictionary<float, int>();
        Dictionary<float, int> limit_y = new Dictionary<float, int>();
        AppSettings settings = new AppSettings();

        private void CaptureTimer_Tick(object sender, EventArgs e)
        {
            capturedImage = CaptureScreenArea(captureArea);
            Invalidate();

            Screen CursorScreen = Screen.FromPoint(Cursor.Position);

            if (follow_mouse && Cursor.Position.X != last_mouse_position_x && CursorScreen.Primary)
            {
               
                int x = Cursor.Position.X + cursor_correction_x;
                int y = Cursor.Position.Y + cursor_correction_y;

                int x_limit = primaryScreen.Bounds.Width;
                int y_limit = primaryScreen.Bounds.Height;

                if (limit_x.ContainsKey(zoomLevel)) {
                    x_limit = limit_x[zoomLevel];
                }

                if (limit_y.ContainsKey(zoomLevel))
                {
                    y_limit = limit_y[zoomLevel];
                }

                last_mouse_position_x = Cursor.Position.X;
                 
                int width = captureArea.Width;
                int height = captureArea.Height;


                if (x > x_limit)
                {
                    x = x_limit;
                }
                else if (x < 0) {
                   x = 0;
                }

                if (y > y_limit)
                {
                    y = y_limit;
                }
                else if (y < 0) {
                    y = 0;
                }

               
                captureArea = new Rectangle(x, y, width, height);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        { 
            primaryScreen = Screen.PrimaryScreen;
            secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Equals(primaryScreen));
            StartFullScreenCapture();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            /*Getting monitor info*/
            primaryDisplay.Dpi = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "LogPixels", 96);
            primaryDisplay.scaleFactor = (int)((decimal)primaryDisplay.Dpi * 100 / 96);
            primaryDisplay.scaleF = ((decimal)primaryDisplay.scaleFactor / 100);
            primaryDisplay.Width = (int)(primaryScreen.Bounds.Width * primaryDisplay.scaleF);
            primaryDisplay.Height = (int)(primaryScreen.Bounds.Height * primaryDisplay.scaleF);
            primaryDisplay.Resolution = primaryDisplay.Width + "x" + primaryDisplay.Height;

            Console.WriteLine("Display1");
            Console.WriteLine("scale: " + primaryDisplay.scaleFactor + "%");
            Console.WriteLine("resolution: " + primaryDisplay.Resolution);

            secondaryDisplay.Dpi = this.DeviceDpi;//secondayScreen DPI
            secondaryDisplay.scaleFactor = ((secondaryDisplay.Dpi / 96) * 100);
            secondaryDisplay.scaleF = ((decimal)secondaryDisplay.scaleFactor / 100);
            secondaryDisplay.Width = (int)(secondaryScreen.Bounds.Width * secondaryDisplay.scaleF);
            secondaryDisplay.Height = (int)(secondaryScreen.Bounds.Height * secondaryDisplay.scaleF);
            secondaryDisplay.Resolution = secondaryDisplay.Width + "x" + secondaryDisplay.Height;

            Console.WriteLine("Display2");
            Console.WriteLine("scale: " + secondaryDisplay.scaleFactor + "%");
            Console.WriteLine("resolution: " + secondaryDisplay.Resolution);
            
            settings = LoadSettings() ?? new AppSettings();

            limit_y = settings.verticalLimit;
            limit_x = settings.horizontalLimit;

            if (settings.appPreferences.ContainsKey("cursor_correction_x"))
            {
                cursor_correction_x = int.Parse(settings.appPreferences["cursor_correction_x"]);
            }

            if (settings.appPreferences.ContainsKey("cursor_correction_y"))
            {
                cursor_correction_y = int.Parse(settings.appPreferences["cursor_correction_y"]);
            }

            if (follow_mouse) {
                zoomLevel = 1.3f;
            }

            captureTimer = new Timer();
            captureTimer.Interval = 60;
            captureTimer.Tick += CaptureTimer_Tick;
            captureTimer.Start();

        }

        public static AppSettings LoadSettings()
        {
            string filePath = "keraliz_preferences.xml";

            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    return (AppSettings)serializer.Deserialize(fileStream);
                }
            }
            else
            {
                return null;
            }
        }
        public static void SaveSettings(AppSettings settings)
        {
            string filePath = "keraliz_preferences.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, settings);
            }
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
                
                 //to-do: fix this crap 
                int x_limit = (int)((primaryDisplay.Width - secondaryDisplay.Width)*(zoomLevel)*1.3);
                int y_limit = (int)((primaryDisplay.Height - secondaryDisplay.Height)*(zoomLevel)*1.25);

                switch (e.KeyCode)
                {
                    case Keys.W:
                        y = Math.Max(0, y - stepSize);
                        break;
                    case Keys.S:
                        if (y < y_limit)
                        {
                            y += stepSize;
                        }
                        else
                        {
                            y = y_limit;
                        }
                        break;
                    case Keys.A:
                        x = Math.Max(0, x - stepSize);
                        break;
                    case Keys.D:
                       if (x < x_limit)
                        {
                            x += stepSize;
                          } else {
                            x = x_limit;
                          }
                        break;
                    case Keys.X:
                        limit_x[zoomLevel] = x;
                        settings.horizontalLimit = (AppSettings.SerializableDictionary<float, int>)limit_x;
                        break;
                    case Keys.Y:
                        limit_y[zoomLevel] = y;
                        settings.verticalLimit = (AppSettings.SerializableDictionary<float, int>)limit_y;
                        break;

                    case Keys.Q:
                        cursor_correction_x = cursor_correction_x - 20;
                        break;
                    case Keys.E:
                        cursor_correction_x = cursor_correction_x + 20;
                        break;

                    case Keys.R:
                        cursor_correction_y = cursor_correction_y - 20;
                        break;
                    case Keys.T:
                        cursor_correction_y = cursor_correction_y + 20;
                        break;

                    case Keys.K:
                        settings.appPreferences["cursor_correction_x"] = cursor_correction_x.ToString();
                        settings.appPreferences["cursor_correction_y"] = cursor_correction_y.ToString();
                        SaveSettings(settings);
                        break;
                    case Keys.C:
                        limit_x.Remove(zoomLevel);
                        limit_y.Remove(zoomLevel);
                        break;

                    case Keys.L:
                        limit_x = new Dictionary<float, int>();
                        limit_y = new Dictionary<float, int>();
                        break;
                }

                //2560x1440, 1920x1080
                //640 360

                switch (e.KeyCode)
                {
                    case Keys.Up:
                        zoomLevel += zoomStep;
                        if (zoomLevel > zoomMax)
                            zoomLevel = zoomMax;
                        break;
                    case Keys.Down:
                        zoomLevel -= zoomStep;
                        if (zoomLevel < zoomStep)
                            zoomLevel = zoomStep;
                        if (zoomLevel < zoomMin)
                            zoomLevel = zoomMin;
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

