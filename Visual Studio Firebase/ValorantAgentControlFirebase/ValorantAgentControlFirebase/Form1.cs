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
            this.Size = new Size(400, 250);
            this.Text = "Valorant Agent Control";

            statusLabel = new Label
            {
                Location = new Point(10, 20),
                Size = new Size(360, 20),
                Text = "Durum: Baðlý deðil"
            };

            pathTextBox = new TextBox
            {
                Location = new Point(10, 50),
                Size = new Size(280, 30),
                Text = "Kimlik dosyasýný seçin..."
            };

            var browseButton = new Button
            {
                Location = new Point(300, 50),
                Size = new Size(70, 30),
                Text = "Gözat"
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

            connectButton = new Button
            {
                Location = new Point(10, 90),
                Size = new Size(360, 30),
                Text = "Firebase'e Baðlan"
            };
            connectButton.Click += ConnectButton_Click;

            this.Controls.AddRange(new Control[] { statusLabel, pathTextBox, browseButton, connectButton });
        }

        private void InitializeFirebase(string credentialPath)
        {
            try
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

                _db = FirestoreDb.Create("valorantagentcontrol-1");

                // Test baðlantýsý
                var collection = _db.Collection("test");
                var document = collection.Document("test");
                var data = new Dictionary<string, object>
                {
                    { "test", true },
                    { "timestamp", DateTime.UtcNow }
                };
                document.SetAsync(data).Wait();

                MessageBox.Show("Firebase baþarýyla baþlatýldý!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firebase baþlatma hatasý: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
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
                    if (statusLabel != null) statusLabel.Text = "Durum: Baðlandý";
                    if (connectButton != null) connectButton.Text = "Baðlantýyý Kes";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Baðlantý hatasý: {ex.Message}");
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
                    }
                }
                catch { }

                _db = null;
                isConnected = false;
                if (statusLabel != null) statusLabel.Text = "Durum: Baðlý deðil";
                if (connectButton != null) connectButton.Text = "Firebase'e Baðlan";
            }
        }

        private async Task StartListening()
        {
            if (_db == null) return;

            var collection = _db.Collection("commands");
            var listener = collection.Listen(snapshot =>
            {
                foreach (var change in snapshot.Changes)
                {
                    if (change.ChangeType == DocumentChange.Type.Added ||
                        change.ChangeType == DocumentChange.Type.Modified)
                    {
                        var command = change.Document.ConvertTo<Command>();
                        ProcessCommand(command);
                    }
                }
            });

            await Task.Delay(100); // Prevent method from completing immediately
        }

        private void ProcessCommand(Command command)
        {
            if (command == null || string.IsNullOrEmpty(command.Action)) return;

            switch (command.Action.ToLower())
            {
                case "select":
                    ClickAtPosition(command.X, command.Y);
                    break;
                case "lock":
                    ClickAtPosition(950, 867); // Lock button coordinates
                    break;
            }
        }

        private void ClickAtPosition(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            System.Threading.Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
    }

    [FirestoreData]
    public class Command
    {
        [FirestoreProperty]
        public string? Action { get; set; }

        [FirestoreProperty]
        public int X { get; set; }

        [FirestoreProperty]
        public int Y { get; set; }
    }
}