using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Services;

namespace VehicleRentalSystem.Forms
{
    public class MainForm : Form
    {
        private readonly RentalManager _rentalManager;

        // Dashboard etiketleri
        private Label lblTotalVehiclesVal = null!;
        private Label lblAvailableVehiclesVal = null!;
        private Label lblRentedVehiclesVal = null!;
        private Label lblTotalCustomersVal = null!;
        private Label lblActiveRentalsVal = null!;

        // Hızlı özet listeleri
        private ListView lvActiveRentals = null!;
        private ListView lvAvailableVehicles = null!;

        public MainForm()
        {
            _rentalManager = new RentalManager();
            InitializeCustomComponents();
            
            _rentalManager.DataChanged += UpdateDashboard;
            UpdateDashboard();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Araç Kiralama Yönetim Sistemi | Vehicle Rental Management System";
            this.Size = new Size(1150, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 1. Üst Başlık Paneli (Header)
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(26, 41, 64)
            };

            Label lblTitle = new Label
            {
                Text = "ARAÇ KİRALAMA YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 15),
                AutoSize = true
            };

            Label lblSubTitle = new Label
            {
                Text = "BGY203 / YBS223 Nesne Tabanlı Programlama Proje Ödevi | Dr. Öğr. Üyesi Tohid YOUSEFİ | Resul Ekrem ÖZCAN (25902009)",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 200, 220),
                Location = new Point(27, 48),
                AutoSize = true
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubTitle);

            // 2. Sol Menü Paneli (Sidebar Navigation)
            Panel sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(34, 45, 50)
            };

            int btnY = 20;
            int btnHeight = 50;
            int spacing = 8;

            Button btnNavDashboard = CreateNavButton("Dashboard (Ana Sayfa)", btnY);
            btnNavDashboard.BackColor = Color.FromArgb(41, 128, 185);
            btnY += btnHeight + spacing;

            Button btnNavVehicles = CreateNavButton("Araç Yönetimi", btnY);
            btnNavVehicles.Click += (s, e) => OpenVehiclesForm();
            btnY += btnHeight + spacing;

            Button btnNavCustomers = CreateNavButton("Müşteri Yönetimi", btnY);
            btnNavCustomers.Click += (s, e) => OpenCustomersForm();
            btnY += btnHeight + spacing;

            Button btnNavRent = CreateNavButton("Araç Kiralama", btnY);
            btnNavRent.Click += (s, e) => OpenRentalForm();
            btnY += btnHeight + spacing;

            Button btnNavReturn = CreateNavButton("Araç İade İşlemi", btnY);
            btnNavReturn.Click += (s, e) => OpenReturnForm();
            btnY += btnHeight + spacing + 30;

            Button btnAbout = CreateNavButton("Proje & OOP Bilgisi", btnY);
            btnAbout.Click += (s, e) => ShowAboutInfo();

            sidebarPanel.Controls.Add(btnNavDashboard);
            sidebarPanel.Controls.Add(btnNavVehicles);
            sidebarPanel.Controls.Add(btnNavCustomers);
            sidebarPanel.Controls.Add(btnNavRent);
            sidebarPanel.Controls.Add(btnNavReturn);
            sidebarPanel.Controls.Add(btnAbout);

            // 3. Ana İçerik Paneli (Dashboard İçeriği)
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25),
                AutoScroll = true
            };

            // İstatistik Kartları Konteyneri
            FlowLayoutPanel cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 135,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false
            };

            lblTotalVehiclesVal = new Label();
            lblAvailableVehiclesVal = new Label();
            lblRentedVehiclesVal = new Label();
            lblTotalCustomersVal = new Label();
            lblActiveRentalsVal = new Label();

            cardsPanel.Controls.Add(CreateStatCard("Toplam Araç", lblTotalVehiclesVal, Color.FromArgb(52, 152, 219)));
            cardsPanel.Controls.Add(CreateStatCard("Müsait Araç", lblAvailableVehiclesVal, Color.FromArgb(46, 204, 113)));
            cardsPanel.Controls.Add(CreateStatCard("Kiradaki Araç", lblRentedVehiclesVal, Color.FromArgb(231, 76, 60)));
            cardsPanel.Controls.Add(CreateStatCard("Toplam Müşteri", lblTotalCustomersVal, Color.FromArgb(155, 89, 182)));
            cardsPanel.Controls.Add(CreateStatCard("Aktif Kiralama", lblActiveRentalsVal, Color.FromArgb(243, 156, 18)));

            // Alt Kısım: Tablolar ve Hızlı Bakış
            TableLayoutPanel tablesLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 15, 0, 0)
            };
            tablesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tablesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Sol Kutu: Aktif Kiralamalar
            GroupBox gbActiveRentals = new GroupBox
            {
                Text = "Aktif Kiralamalar (Canlı Takip)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            lvActiveRentals = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            lvActiveRentals.Columns.Add("Kiralama ID", 85);
            lvActiveRentals.Columns.Add("Müşteri", 140);
            lvActiveRentals.Columns.Add("Araç / Plaka", 120);
            lvActiveRentals.Columns.Add("Bitiş Tarihi", 95);
            lvActiveRentals.Columns.Add("Tutar", 85);
            gbActiveRentals.Controls.Add(lvActiveRentals);

            // Sağ Kutu: Kiralanabilir Müsait Araçlar
            GroupBox gbAvailableVehicles = new GroupBox
            {
                Text = "Kiralanabilir Müsait Araçlar",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            lvAvailableVehicles = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };
            lvAvailableVehicles.Columns.Add("Plaka", 90);
            lvAvailableVehicles.Columns.Add("Tür", 85);
            lvAvailableVehicles.Columns.Add("Marka Model", 140);
            lvAvailableVehicles.Columns.Add("Yıl", 60);
            lvAvailableVehicles.Columns.Add("Günlük Ücret", 100);
            gbAvailableVehicles.Controls.Add(lvAvailableVehicles);

            tablesLayout.Controls.Add(gbActiveRentals, 0, 0);
            tablesLayout.Controls.Add(gbAvailableVehicles, 1, 0);

            contentPanel.Controls.Add(tablesLayout);
            contentPanel.Controls.Add(cardsPanel);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(headerPanel);
        }

        private Button CreateNavButton(string text, int yPos)
        {
            return new Button
            {
                Text = text,
                Location = new Point(10, yPos),
                Size = new Size(200, 48),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.White,
                BackColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
        }

        private Panel CreateStatCard(string title, Label lblValue, Color accentColor)
        {
            Panel card = new Panel
            {
                Width = 165,
                Height = 110,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 15, 0)
            };

            Panel accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accentColor
            };

            Label lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(120, 140, 160),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(12, 18),
                AutoSize = true
            };

            lblValue.Text = "0";
            lblValue.ForeColor = Color.FromArgb(40, 50, 60);
            lblValue.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblValue.Location = new Point(12, 45);
            lblValue.AutoSize = true;

            card.Controls.Add(accentBar);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);

            return card;
        }

        private void UpdateDashboard()
        {
            try
            {
                // Form oluşturulmamışsa veya disposed ise çık
                if (this.IsDisposed || !this.IsHandleCreated) return;
                if (lblTotalVehiclesVal == null || lvActiveRentals == null || lvAvailableVehicles == null) return;

                // Cross-thread güvenliği: event farklı context'ten gelebilir
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(UpdateDashboard));
                    return;
                }

                lblTotalVehiclesVal.Text = _rentalManager.GetTotalVehiclesCount().ToString();
                lblAvailableVehiclesVal.Text = _rentalManager.GetAvailableVehiclesCount().ToString();
                lblRentedVehiclesVal.Text = _rentalManager.GetRentedVehiclesCount().ToString();
                lblTotalCustomersVal.Text = _rentalManager.GetTotalCustomersCount().ToString();
                lblActiveRentalsVal.Text = _rentalManager.GetActiveRentalsCount().ToString();

                lvActiveRentals.Items.Clear();
                foreach (var r in _rentalManager.GetActiveRentals())
                {
                    if (r == null || r.Customer == null || r.Vehicle == null) continue;
                    var item = new ListViewItem(r.Id ?? "");
                    item.SubItems.Add(r.Customer.FullName ?? "");
                    item.SubItems.Add($"{r.Vehicle.Plate} ({r.Vehicle.Brand})");
                    item.SubItems.Add(r.EndDate.ToString("dd.MM.yyyy"));
                    item.SubItems.Add(r.TotalPrice.ToString("C2"));
                    lvActiveRentals.Items.Add(item);
                }

                lvAvailableVehicles.Items.Clear();
                foreach (var v in _rentalManager.GetAvailableVehicles())
                {
                    if (v == null) continue;
                    var item = new ListViewItem(v.Plate ?? "");
                    item.SubItems.Add(v.VehicleType ?? "");
                    item.SubItems.Add($"{v.Brand} {v.Model}");
                    item.SubItems.Add(v.Year.ToString());
                    item.SubItems.Add(v.DailyRate.ToString("C2"));
                    lvAvailableVehicles.Items.Add(item);
                }
            }
            catch
            {
                // Sessizce yut
            }
        }

        private void OpenVehiclesForm()
        {
            try
            {
                using var form = new VehiclesForm(_rentalManager);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Araç Yönetimi açılırken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenCustomersForm()
        {
            try
            {
                using var form = new CustomersForm(_rentalManager);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri Yönetimi açılırken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenRentalForm()
        {
            try
            {
                using var form = new RentalForm(_rentalManager);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kiralama Formu açılırken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenReturnForm()
        {
            try
            {
                using var form = new ReturnForm(_rentalManager);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("İade Formu açılırken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAboutInfo()
        {
            string info = "=====================================================\n" +
                          "ARAÇ KİRALAMA YÖNETİM SİSTEMİ (OOP PROJESİ)\n" +
                          "=====================================================\n" +
                          "Ders: BGY203 / YBS223 - Nesne Tabanlı Programlama\n" +
                          "Öğretim Elemanı: Dr. Öğr. Üyesi Tohid YOUSEFİ\n" +
                          "Geliştirici: Resul Ekrem ÖZCAN (25902009)\n\n" +
                          "OOP İLKELERİNİN UYGULANMASI:\n" +
                          "1. Kapsülleme (Encapsulation): Private alanlar, public property'ler, setter içi doğrulama kuralları (örn: günlük ücret > 0).\n" +
                          "2. Kalıtım (Inheritance): Vehicle soyut sınıfından türetilen Car ve Suv sınıfları (base() yapıcı çağrımı).\n" +
                          "3. Çok Biçimlilik (Polymorphism): CalculateRentalPrice(int days) metodu Car ve Suv sınıflarında farklı kampanya/kasko mantıklarıyla override edilmiştir.\n" +
                          "4. Soyutlama (Abstraction): IRentable arayüzü ve abstract Vehicle sınıfı.\n" +
                          "5. Bellek İçi Yönetim: List<T> generic koleksiyonları ve LINQ sorguları.";

            MessageBox.Show(info, "Proje ve Mimari Hakkında", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
