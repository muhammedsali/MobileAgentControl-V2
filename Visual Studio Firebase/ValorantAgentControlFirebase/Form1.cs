using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ValorantAgentControlFirebase
{
    public partial class Form1 : Form
    {
        private FirestoreDb? _db;
        private bool isConnected = false;
        private Label? statusLabel;
        private Button? connectButton;
        private TextBox? pathTextBox;
        private ListBox? logListBox;

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Size = new Size(600, 400);
            this.Text = "Valorant Agent Control";

            statusLabel = new Label
            {
                Location = new Point(10, 20),
                Size = new Size(360, 20),
                Text = "Durum: Bağlı değil"
            };

            pathTextBox = new TextBox
            {
                Location = new Point(10, 50),
                Size = new Size(280, 30),
                Text = "Kimlik dosyasını seçin..."
            };

            var browseButton = new Button
            {
                Location = new Point(300, 50),
                Size = new Size(70, 30),
                Text = "Gözat"
            };

            connectButton = new Button
            {
                Location = new Point(10, 90),
                Size = new Size(360, 30),
                Text = "Firebase'e Bağlan"
            };

            logListBox = new ListBox
            {
                Location = new Point(10, 130),
                Size = new Size(560, 200),
                ScrollAlwaysVisible = true
            };

            browseButton.Click += (s, e) =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;

                    if (openFileDialog.ShowDialog() == DialogResult.OK && pathTextBox != null)
                    {
                        pathTextBox.Text = openFileDialog.FileName;
                    }
                }
            };

            connectButton.Click += ConnectButton_Click;

            this.Controls.AddRange(new Control[] { statusLabel, pathTextBox, browseButton, connectButton, logListBox });
        }

        private void AddLog(string message)
        {
            if (logListBox != null && !IsDisposed)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AddLog(message)));
                    return;
                }

                logListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                logListBox.SelectedIndex = logListBox.Items.Count - 1;
            }
        }

        private void InitializeFirebase(string credentialPath)
        {
            try
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
                AddLog("Kimlik dosyası ayarlandı");

                _db = FirestoreDb.Create("valorantagentcontrol-1");
                AddLog("Firebase veritabanı bağlantısı oluşturuldu");

                var collection = _db.Collection("test");
                var document = collection.Document("test");
                var data = new Dictionary<string, object>
                {
                    { "test", true },
                    { "timestamp", DateTime.UtcNow }
                };
                document.SetAsync(data).Wait();
                AddLog("Test bağlantısı başarılı");

                MessageBox.Show("Firebase başarıyla başlatıldı!");
            }
            catch (Exception ex)
            {
                AddLog($"HATA: {ex.Message}");
                MessageBox.Show($"Firebase başlatma hatası: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            }
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            if (!isConnected && pathTextBox != null)
            {
                try
                {
                    InitializeFirebase(pathTextBox.Text);
                    await StartListening();
                    isConnected = true;
                    if (statusLabel != null) statusLabel.Text = "Durum: Bağlandı";
                    if (connectButton != null) connectButton.Text = "Bağlantıyı Kes";
                    AddLog("Firebase'e başarıyla bağlandı");
                }
                catch (Exception ex)
                {
                    AddLog($"Bağlantı hatası: {ex.Message}");
                    MessageBox.Show($"Bağlantı hatası: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    var app = FirebaseApp.DefaultInstance;
                    if (app != null)
                    {
                        app.Delete();
                        AddLog("Firebase bağlantısı kesildi");
                    }
                }
                catch { }

                _db = null;
                isConnected = false;
                if (statusLabel != null) statusLabel.Text = "Durum: Bağlı değil";
                if (connectButton != null) connectButton.Text = "Firebase'e Bağlan";
            }
        }

        private async Task StartListening()
        {
            if (_db == null) return;

            AddLog("Komut dinlemeye başlandı");
            var collection = _db.Collection("commands");
            var listener = collection.Listen(snapshot =>
            {
                foreach (var change in snapshot.Changes)
                {
                    if (change.ChangeType == DocumentChange.Type.Added ||
                        change.ChangeType == DocumentChange.Type.Modified)
                    {
                        var command = change.Document.ConvertTo<Command>();
                        AddLog($"Yeni komut alındı: {command.Action}, X: {command.X}, Y: {command.Y}");
                        ProcessCommand(command);
                    }
                }
            });

            await Task.Delay(100);
        }

        private void ProcessCommand(Command command)
        {
            if (command == null || string.IsNullOrEmpty(command.Action)) return;

            // Debug için ham veriyi yazdıralım
            var rawData = command.ToString();
            AddLog($"Ham veri: {rawData}");
            AddLog($"Komut detayları - Action: {command.Action}, X: {command.X}, Y: {command.Y}");

            switch (command.Action.ToLower())
            {
                case "select":
                    ClickAtPosition((int)command.X, (int)command.Y);
                    break;
                case "lock":
                    ClickAtPosition(950, 867);
                    break;
            }
        }

        private void ClickAtPosition(int x, int y)
        {
            try 
            {
                AddLog($"Fare tıklaması başlatılıyor: X: {x}, Y: {y}");
                SetCursorPos(x, y);
                mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                System.Threading.Thread.Sleep(100);
                mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                AddLog($"Fare tıklaması tamamlandı: X: {x}, Y: {y}");
            }
            catch (Exception ex)
            {
                AddLog($"Fare tıklama hatası: {ex.Message}");
            }
        }
    }

    [FirestoreData]
    public class Command
    {
        [FirestoreProperty("action")]
        public string? Action { get; set; }

        [FirestoreProperty("x")]
        public long X { get; set; }

        [FirestoreProperty("y")]
        public long Y { get; set; }
    }
} 