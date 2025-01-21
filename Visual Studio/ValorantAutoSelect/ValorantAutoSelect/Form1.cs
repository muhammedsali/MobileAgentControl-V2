using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ValorantAutoSelect
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private bool isListening = false;
        private Label statusLabel;
        private TextBox ipTextBox;
        private Button startButton;
        private Label clickPositionLabel;
        private bool isTrackingMouse = false;

        // Mouse kontrolü için Win32 API fonksiyonları
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                isTrackingMouse = !isTrackingMouse;
                clickPositionLabel.Text = isTrackingMouse ? 
                    "Mouse Pozisyonu Takip Ediliyor (F8 ile kapat)" : 
                    "Mouse Pozisyonu Takibi Kapalı (F8 ile aç)";
            }
        }

        private void InitializeUI()
        {
            this.Text = "Valorant Karakter Seçici";
            this.Size = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 25, 35);
            this.ForeColor = Color.White;

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                RowStyles = {
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40)
                }
            };

            statusLabel = new Label
            {
                Text = "Sunucu Durumu: Kapalı",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            ipTextBox = new TextBox
            {
                Text = GetLocalIPAddress(),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Consolas", 12),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            startButton = new Button
            {
                Text = "Sunucuyu Başlat",
                BackColor = Color.FromArgb(255, 70, 85),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill
            };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;

            clickPositionLabel = new Label
            {
                Text = "Mouse Pozisyonu Takibi Kapalı (F8 ile aç)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            mainLayout.Controls.Add(statusLabel, 0, 0);
            mainLayout.Controls.Add(ipTextBox, 0, 1);
            mainLayout.Controls.Add(startButton, 0, 2);
            mainLayout.Controls.Add(clickPositionLabel, 0, 3);

            this.Controls.Add(mainLayout);

            // Mouse pozisyonu takibi için timer
            System.Windows.Forms.Timer mouseTracker = new System.Windows.Forms.Timer();
            mouseTracker.Interval = 100;
            mouseTracker.Tick += (s, e) =>
            {
                if (isTrackingMouse)
                {
                    GetCursorPos(out Point p);
                    clickPositionLabel.Text = $"Mouse Pozisyonu - X: {p.X}, Y: {p.Y} (F8 ile kapat)";
                }
            };
            mouseTracker.Start();
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            if (!isListening)
            {
                try
                {
                    server = new TcpListener(IPAddress.Any, 12345);
                    server.Start();
                    isListening = true;
                    startButton.Text = "Sunucuyu Durdur";
                    startButton.BackColor = Color.FromArgb(239, 68, 68);
                    statusLabel.Text = "Sunucu Durumu: Çalışıyor";
                    await ListenForClients();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                StopServer();
            }
        }

        private async Task ListenForClients()
        {
            while (isListening)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    statusLabel.Text = "Sunucu Durumu: İstemci Bağlandı";
                    _ = HandleClientAsync(client);
                }
                catch (Exception)
                {
                    if (isListening)
                    {
                        MessageBox.Show("Bağlantı hatası oluştu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                while (client.Connected)
                {
                    try
                    {
                        string message = await reader.ReadLineAsync();
                        if (message != null)
                        {
                            ProcessCommand(message);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            statusLabel.Text = "Sunucu Durumu: İstemci Bağlantısı Kesildi";
        }

        private void ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');
            if (parts[0] == "CLICK" && parts.Length == 3)
            {
                if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        statusLabel.Text = $"Komut Alındı: X={x}, Y={y}";
                        SelectCharacter(x, y);
                    });
                }
            }
        }

        private void SelectCharacter(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            System.Threading.Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private void StopServer()
        {
            isListening = false;
            server?.Stop();
            startButton.Text = "Sunucuyu Başlat";
            startButton.BackColor = Color.FromArgb(255, 70, 85);
            statusLabel.Text = "Sunucu Durumu: Kapalı";
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopServer();
            base.OnFormClosing(e);
        }
    }
}
