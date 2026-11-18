using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Services;

namespace VehicleRentalSystem.Forms
{
    public class RentalForm : Form
    {
        private readonly RentalManager _rentalManager;

        private ComboBox cmbCustomers = null!;
        private ComboBox cmbVehicles = null!;
        private DateTimePicker dtpStartDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private Label lblSelectedVehicleDetails = null!;
        private Label lblTotalDays = null!;
        private Label lblCalculatedPrice = null!;

        public RentalForm(RentalManager rentalManager)
        {
            _rentalManager = rentalManager;
            InitializeCustomComponents();
            LoadDropdownData();
            CalculatePreviewPrice();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Araç Kiralama İşlemi | Vehicle Rental";
            this.Size = new Size(620, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Başlık
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(243, 156, 18)
            };
            Label lblHeader = new Label
            {
                Text = "Yeni Araç Kiralama Kaydı Oluştur",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(25, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeader);

            // Gövde Kartı
            Panel pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30)
            };

            int curY = 20;
            int stepY = 58;

            // 1. Müşteri Seçimi
            Label lblCust = new Label { Text = "Müşteri Seçiniz *", Location = new Point(30, curY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbCustomers = new ComboBox { Location = new Point(30, curY + 22), Width = 540, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlBody.Controls.Add(lblCust);
            pnlBody.Controls.Add(cmbCustomers);
            curY += stepY;

            // 2. Müsait Araç Seçimi
            Label lblVeh = new Label { Text = "Müsait Araç Seçiniz *", Location = new Point(30, curY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbVehicles = new ComboBox { Location = new Point(30, curY + 22), Width = 540, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVehicles.SelectedIndexChanged += (s, e) =>
            {
                UpdateVehicleInfoLabel();
                CalculatePreviewPrice();
            };
            pnlBody.Controls.Add(lblVeh);
            pnlBody.Controls.Add(cmbVehicles);
            curY += stepY;

            // Seçilen araç detay kutucuğu
            lblSelectedVehicleDetails = new Label
            {
                Text = "Seçilen Araç: -",
                Location = new Point(30, curY),
                Width = 540,
                ForeColor = Color.FromArgb(41, 128, 185),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic)
            };
            pnlBody.Controls.Add(lblSelectedVehicleDetails);
            curY += 28;

            // 3. Tarihler (Başlangıç ve Bitiş)
            Label lblStart = new Label { Text = "Başlangıç Tarihi * (Geçmişe kiralama yapılamaz)", Location = new Point(30, curY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            dtpStartDate = new DateTimePicker 
            { 
                Location = new Point(30, curY + 22), 
                Width = 255, 
                Format = DateTimePickerFormat.Short, 
                MinDate = DateTime.Today,
                Value = DateTime.Today 
            };
            dtpStartDate.ValueChanged += (s, e) => 
            {
                if (dtpEndDate.Value.Date < dtpStartDate.Value.Date)
                {
                    dtpEndDate.MinDate = dtpStartDate.Value.Date;
                    dtpEndDate.Value = dtpStartDate.Value.Date.AddDays(1);
                }
                else
                {
                    dtpEndDate.MinDate = dtpStartDate.Value.Date;
                }
                CalculatePreviewPrice();
            };

            Label lblEnd = new Label { Text = "Bitiş Tarihi *", Location = new Point(315, curY), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            dtpEndDate = new DateTimePicker 
            { 
                Location = new Point(315, curY + 22), 
                Width = 255, 
                Format = DateTimePickerFormat.Short, 
                MinDate = DateTime.Today,
                Value = DateTime.Today.AddDays(2) 
            };
            dtpEndDate.ValueChanged += (s, e) => CalculatePreviewPrice();

            pnlBody.Controls.Add(lblStart);
            pnlBody.Controls.Add(dtpStartDate);
            pnlBody.Controls.Add(lblEnd);
            pnlBody.Controls.Add(dtpEndDate);
            curY += stepY + 10;

            // 4. Hesaplama Bilgisi Paneli (Toplam Gün ve Tutar)
            Panel pnlSummary = new Panel
            {
                Location = new Point(30, curY),
                Width = 540,
                Height = 65,
                BackColor = Color.FromArgb(235, 245, 251)
            };

            lblTotalDays = new Label
            {
                Text = "Toplam Kiralama: 2 Gün",
                Location = new Point(15, 12),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true
            };

            lblCalculatedPrice = new Label
            {
                Text = "Tahmini Ücret: 0,00 ₺",
                Location = new Point(15, 34),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                AutoSize = true
            };

            pnlSummary.Controls.Add(lblTotalDays);
            pnlSummary.Controls.Add(lblCalculatedPrice);
            pnlBody.Controls.Add(pnlSummary);
            curY += 80;

            // 5. Onay Butonu
            Button btnConfirmRental = new Button
            {
                Text = "Kiralama İşlemini Onayla ve Başlat",
                Location = new Point(30, curY),
                Width = 540,
                Height = 45,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConfirmRental.Click += BtnConfirmRental_Click;
            pnlBody.Controls.Add(btnConfirmRental);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);
        }

        private void LoadDropdownData()
        {
            try
            {
                cmbCustomers.Items.Clear();
                var customers = _rentalManager.GetAllCustomers();
                foreach (var c in customers)
                {
                    if (c != null)
                        cmbCustomers.Items.Add(new ComboBoxItem(c.Id, $"{c.FullName} (Ehliyet: {c.DriverLicenseNumber})"));
                }
                if (cmbCustomers.Items.Count > 0) cmbCustomers.SelectedIndex = 0;

                cmbVehicles.Items.Clear();
                var availVehicles = _rentalManager.GetAvailableVehicles();
                foreach (var v in availVehicles)
                {
                    if (v != null)
                        cmbVehicles.Items.Add(new ComboBoxItem(v.Id, $"{v.Plate} - {v.Brand} {v.Model} ({v.VehicleType}) - {v.DailyRate:C2}/Gün"));
                }
                if (cmbVehicles.Items.Count > 0) cmbVehicles.SelectedIndex = 0;

                UpdateVehicleInfoLabel();
            }
            catch { }
        }

        private void UpdateVehicleInfoLabel()
        {
            try
            {
                if (lblSelectedVehicleDetails == null) return;

                if (cmbVehicles != null && cmbVehicles.SelectedItem is ComboBoxItem item)
                {
                    var vehicle = _rentalManager.GetAllVehicles().FirstOrDefault(v => v.Id == item.Value);
                    if (vehicle != null)
                    {
                        lblSelectedVehicleDetails.Text = vehicle.GetDetails();
                        return;
                    }
                }
                lblSelectedVehicleDetails.Text = "Seçilen araç bilgisi yok.";
            }
            catch { }
        }

        private void CalculatePreviewPrice()
        {
            try
            {
                if (dtpEndDate == null || dtpStartDate == null || lblTotalDays == null || lblCalculatedPrice == null) return;

                if (dtpEndDate.Value.Date < dtpStartDate.Value.Date)
                {
                    lblTotalDays.Text = "Hata: Bitiş tarihi başlangıçtan önce!";
                    lblCalculatedPrice.Text = "Geçersiz Tarih";
                    lblCalculatedPrice.ForeColor = Color.Red;
                    return;
                }

                int days = (dtpEndDate.Value.Date - dtpStartDate.Value.Date).Days;
                if (days == 0) days = 1;

                lblTotalDays.Text = $"Toplam Kiralama Süresi: {days} Gün";

                if (cmbVehicles != null && cmbVehicles.SelectedItem is ComboBoxItem item)
                {
                    var vehicle = _rentalManager.GetAllVehicles().FirstOrDefault(v => v.Id == item.Value);
                    if (vehicle != null)
                    {
                        decimal price = vehicle.CalculateRentalPrice(days);
                        lblCalculatedPrice.Text = $"Toplam Hesaplanan Ücret: {price:C2} {(vehicle is Car && days >= 7 ? "(%10 İndirimli)" : "")} {(vehicle is Suv ? "(Arazi Kaskolu)" : "")}";
                        lblCalculatedPrice.ForeColor = Color.FromArgb(39, 174, 96);
                        return;
                    }
                }

                lblCalculatedPrice.Text = "Tahmini Ücret: -";
            }
            catch
            {
                if (lblCalculatedPrice != null)
                    lblCalculatedPrice.Text = "Hesaplama Hatası";
            }
        }

        private void BtnConfirmRental_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbCustomers.SelectedItem is not ComboBoxItem custItem)
                {
                    MessageBox.Show("Lütfen bir müşteri seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbVehicles.SelectedItem is not ComboBoxItem vehItem)
                {
                    MessageBox.Show("Lütfen müsait bir araç seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime start = dtpStartDate.Value.Date;
                DateTime end = dtpEndDate.Value.Date;

                if (start < DateTime.Today)
                {
                    MessageBox.Show("Kiralama başlangıç tarihi bugünden önceki bir tarih olamaz!", "Geçersiz Tarih", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (end < start)
                {
                    MessageBox.Show("Bitiş tarihi başlangıç tarihinden önce olamaz!", "Tarih Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var rental = _rentalManager.CreateRental(custItem.Value, vehItem.Value, start, end);

                MessageBox.Show($"Kiralama başarıyla tamamlandı!\n\n" +
                                $"Kiralama Kodu: {rental.Id}\n" +
                                $"Müşteri: {rental.Customer.FullName}\n" +
                                $"Araç: {rental.Vehicle.Plate} ({rental.Vehicle.Brand} {rental.Vehicle.Model})\n" +
                                $"Süre: {rental.TotalDays} Gün\n" +
                                $"Toplam Tutar: {rental.TotalPrice:C2}",
                                "İşlem Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kiralama Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ComboBoxItem
        {
            public string Value { get; set; }
            public string Text { get; set; }

            public ComboBoxItem(string value, string text)
            {
                Value = value;
                Text = text;
            }

            public override string ToString() => Text;
        }
    }
}
