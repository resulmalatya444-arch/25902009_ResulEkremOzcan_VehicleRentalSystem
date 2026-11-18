using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Services;

namespace VehicleRentalSystem.Forms
{
    public class VehiclesForm : Form
    {
        private readonly RentalManager _rentalManager;

        private DataGridView dgvVehicles = null!;
        private TextBox txtPlate = null!;
        private TextBox txtBrand = null!;
        private TextBox txtModel = null!;
        private TextBox txtYear = null!;
        private NumericUpDown numDailyRate = null!;
        private ComboBox cmbVehicleType = null!;
        private NumericUpDown numCapacity = null!;
        private ComboBox cmbTransmission = null!;
        private CheckBox chk4x4 = null!;
        private Label lblSelectedId = null!;
        private bool _isLoading = false;

        public VehiclesForm(RentalManager rentalManager)
        {
            _rentalManager = rentalManager;
            InitializeCustomComponents();
            this.Load += VehiclesForm_Load;
        }

        private void VehiclesForm_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadVehiclesToGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Araç listesi yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Araç Yönetimi | Vehicle Management";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Başlık Paneli
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(41, 128, 185)
            };
            Label lblHeader = new Label
            {
                Text = "Araç Kayıt, Güncelleme ve Listeleme",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeader);

            // Sol Panel - Form Alanları (Kayıt / Güncelleme)
            GroupBox gbInput = new GroupBox
            {
                Text = "Araç Bilgileri",
                Dock = DockStyle.Left,
                Width = 340,
                Padding = new Padding(15),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            int curY = 30;
            int stepY = 52;

            lblSelectedId = new Label
            {
                Text = "Seçili ID: Yok (Yeni Kayıt)",
                ForeColor = Color.FromArgb(100, 110, 120),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(15, curY),
                AutoSize = true
            };
            gbInput.Controls.Add(lblSelectedId);
            curY += 25;

            // Plaka
            Label lblPlate = new Label { Text = "Plaka *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtPlate = new TextBox { Location = new Point(15, curY + 18), Width = 300, CharacterCasing = CharacterCasing.Upper };
            gbInput.Controls.Add(lblPlate);
            gbInput.Controls.Add(txtPlate);
            curY += stepY;

            // Tür
            Label lblType = new Label { Text = "Araç Türü *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            cmbVehicleType = new ComboBox { Location = new Point(15, curY + 18), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVehicleType.Items.AddRange(new object[] { "Otomobil (Car)", "SUV / Arazi (Suv)" });
            cmbVehicleType.SelectedIndex = 0;
            cmbVehicleType.SelectedIndexChanged += CmbVehicleType_SelectedIndexChanged;
            gbInput.Controls.Add(lblType);
            gbInput.Controls.Add(cmbVehicleType);
            curY += stepY;

            // Marka
            Label lblBrand = new Label { Text = "Marka *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtBrand = new TextBox { Location = new Point(15, curY + 18), Width = 300 };
            gbInput.Controls.Add(lblBrand);
            gbInput.Controls.Add(txtBrand);
            curY += stepY;

            // Model
            Label lblModel = new Label { Text = "Model *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtModel = new TextBox { Location = new Point(15, curY + 18), Width = 300 };
            gbInput.Controls.Add(lblModel);
            gbInput.Controls.Add(txtModel);
            curY += stepY;

            // Yıl ve Günlük Ücret (Yan Yana)
            Label lblYear = new Label { Text = "Üretim Yılı *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtYear = new TextBox { Location = new Point(15, curY + 18), Width = 140, MaxLength = 4, Text = "2023" };

            Label lblRate = new Label { Text = "Günlük Kira (₺) *", Location = new Point(165, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            numDailyRate = new NumericUpDown { Location = new Point(165, curY + 18), Width = 150, Minimum = 1, Maximum = 100000, Value = 1000, DecimalPlaces = 2 };
            gbInput.Controls.Add(lblYear);
            gbInput.Controls.Add(txtYear);
            gbInput.Controls.Add(lblRate);
            gbInput.Controls.Add(numDailyRate);
            curY += stepY;

            // Alt Sınıf Ekstra Özellikleri
            Label lblCap = new Label { Text = "Koltuk", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            numCapacity = new NumericUpDown { Location = new Point(15, curY + 18), Width = 70, Minimum = 1, Maximum = 9, Value = 5 };

            Label lblTrans = new Label { Text = "Vites", Location = new Point(95, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            cmbTransmission = new ComboBox { Location = new Point(95, curY + 18), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTransmission.Items.AddRange(new object[] { "Otomatik", "Manuel" });
            cmbTransmission.SelectedIndex = 0;

            chk4x4 = new CheckBox { Text = "4x4 AWD", Location = new Point(220, curY + 18), AutoSize = true, Enabled = false, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            gbInput.Controls.Add(lblCap);
            gbInput.Controls.Add(numCapacity);
            gbInput.Controls.Add(lblTrans);
            gbInput.Controls.Add(cmbTransmission);
            gbInput.Controls.Add(chk4x4);
            curY += stepY + 10;

            // Butonlar: Ekle, Güncelle, Temizle, Sil
            Button btnSave = new Button
            {
                Text = "Aracı Kaydet (Ekle)",
                Location = new Point(15, curY),
                Width = 300,
                Height = 35,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            curY += 40;

            Button btnUpdate = new Button
            {
                Text = "Seçileni Güncelle",
                Location = new Point(15, curY),
                Width = 145,
                Height = 35,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new Button
            {
                Text = "Seçileni Sil",
                Location = new Point(170, curY),
                Width = 145,
                Height = 35,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDelete_Click;
            curY += 40;

            Button btnClear = new Button
            {
                Text = "Formu Temizle / Yeni",
                Location = new Point(15, curY),
                Width = 300,
                Height = 30,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += (s, e) => ClearForm();

            gbInput.Controls.Add(btnSave);
            gbInput.Controls.Add(btnUpdate);
            gbInput.Controls.Add(btnDelete);
            gbInput.Controls.Add(btnClear);

            // Sağ Panel - DataGridView Araç Tablosu
            Panel pnlTable = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            dgvVehicles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            // SelectionChanged event Load sonrası bağlanacak

            pnlTable.Controls.Add(dgvVehicles);

            this.Controls.Add(pnlTable);
            this.Controls.Add(gbInput);
            this.Controls.Add(pnlHeader);
        }

        private void CmbVehicleType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbVehicleType == null || chk4x4 == null || numCapacity == null || cmbTransmission == null) return;
            bool isSuv = cmbVehicleType.SelectedIndex == 1;
            chk4x4.Enabled = isSuv;
            numCapacity.Enabled = !isSuv;
            cmbTransmission.Enabled = !isSuv;
        }

        private void LoadVehiclesToGrid()
        {
            _isLoading = true;
            try
            {
                dgvVehicles.SelectionChanged -= DgvVehicles_SelectionChanged;
                dgvVehicles.DataSource = null;

                var list = _rentalManager.GetAllVehicles();
                var displayList = list.Select(v => new
                {
                    v.Id,
                    v.Plate,
                    v.VehicleType,
                    v.Brand,
                    v.Model,
                    v.Year,
                    GünlükKira = v.DailyRate.ToString("C2"),
                    Durum = v.Status == VehicleStatus.Available ? "Müsait" : "Kirada"
                }).ToList();

                dgvVehicles.DataSource = displayList;

                if (dgvVehicles.Columns.Count > 0)
                {
                    if (dgvVehicles.Columns.Contains("Id"))
                        dgvVehicles.Columns["Id"].Width = 70;
                    if (dgvVehicles.Columns.Contains("Plate"))
                        dgvVehicles.Columns["Plate"].HeaderText = "Plaka";
                    if (dgvVehicles.Columns.Contains("VehicleType"))
                        dgvVehicles.Columns["VehicleType"].HeaderText = "Tür";
                    if (dgvVehicles.Columns.Contains("Brand"))
                        dgvVehicles.Columns["Brand"].HeaderText = "Marka";
                    if (dgvVehicles.Columns.Contains("Model"))
                        dgvVehicles.Columns["Model"].HeaderText = "Model";
                    if (dgvVehicles.Columns.Contains("Year"))
                        dgvVehicles.Columns["Year"].HeaderText = "Yıl";
                    if (dgvVehicles.Columns.Contains("GünlükKira"))
                        dgvVehicles.Columns["GünlükKira"].HeaderText = "Günlük Kira";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadVehiclesToGrid hatası: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
                dgvVehicles.SelectionChanged += DgvVehicles_SelectionChanged;
            }
        }

        private void DgvVehicles_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;

            try
            {
                if (dgvVehicles == null || dgvVehicles.IsDisposed) return;
                if (lblSelectedId == null || txtPlate == null || txtBrand == null || txtModel == null || txtYear == null || numDailyRate == null) return;
                if (dgvVehicles.Columns == null || dgvVehicles.Columns.Count == 0) return;

                if (dgvVehicles.SelectedRows.Count > 0)
                {
                    var selectedRow = dgvVehicles.SelectedRows[0];
                    if (selectedRow == null) return;
                    if (!dgvVehicles.Columns.Contains("Id")) return;

                    var cell = selectedRow.Cells["Id"];
                    if (cell == null || cell.Value == null) return;

                    string id = cell.Value.ToString() ?? "";
                    if (string.IsNullOrEmpty(id)) return;

                    var vehicle = _rentalManager.GetAllVehicles().FirstOrDefault(v => v.Id == id);
                    if (vehicle != null)
                    {
                        lblSelectedId.Text = $"Seçili ID: {vehicle.Id}";
                        txtPlate.Text = vehicle.Plate;
                        txtBrand.Text = vehicle.Brand;
                        txtModel.Text = vehicle.Model;
                        txtYear.Text = vehicle.Year.ToString();

                        if (vehicle.DailyRate >= numDailyRate.Minimum && vehicle.DailyRate <= numDailyRate.Maximum)
                            numDailyRate.Value = vehicle.DailyRate;

                        if (vehicle is Car car)
                        {
                            cmbVehicleType.SelectedIndex = 0;
                            if (car.SeatingCapacity >= numCapacity.Minimum && car.SeatingCapacity <= numCapacity.Maximum)
                                numCapacity.Value = car.SeatingCapacity;
                            cmbTransmission.SelectedItem = car.TransmissionType;
                        }
                        else if (vehicle is Suv suv)
                        {
                            cmbVehicleType.SelectedIndex = 1;
                            chk4x4.Checked = suv.FourWheelDrive;
                        }
                    }
                }
            }
            catch
            {
                // UI eventlerinde sessiz ve güvenli geçiş
            }
        }

        /// <summary>
        /// Form düzeyinde girdi doğrulama. Boş, özel karakter, geçersiz plaka girişleri engeller.
        /// </summary>
        private bool ValidateInputs(out string errorMessage)
        {
            errorMessage = "";

            string plate = txtPlate.Text.Trim();
            string brand = txtBrand.Text.Trim();
            string model = txtModel.Text.Trim();

            // Plaka kontrolü: boş olamaz, en az 5 karakter, alfanumerik + boşluk
            if (string.IsNullOrWhiteSpace(plate))
            {
                errorMessage = "Araç plakası boş bırakılamaz!";
                txtPlate.Focus();
                return false;
            }
            if (plate.Length < 5)
            {
                errorMessage = "Plaka en az 5 karakter olmalıdır! (Örnek: 34ABC12)";
                txtPlate.Focus();
                return false;
            }
            if (!Regex.IsMatch(plate, @"^[A-Za-z0-9\s]+$"))
            {
                errorMessage = "Plaka sadece harf, rakam ve boşluk içerebilir! (Özel karakter kullanılamaz)";
                txtPlate.Focus();
                return false;
            }

            // Marka kontrolü: harf + boşluk, en az 2 karakter
            if (string.IsNullOrWhiteSpace(brand))
            {
                errorMessage = "Araç markası boş bırakılamaz!";
                txtBrand.Focus();
                return false;
            }
            if (brand.Length < 2)
            {
                errorMessage = "Marka en az 2 karakter olmalıdır!";
                txtBrand.Focus();
                return false;
            }
            if (!Regex.IsMatch(brand, @"^[a-zA-ZçÇğĞıİöÖşŞüÜâÂîÎûÛ\s\-]+$"))
            {
                errorMessage = "Marka sadece harflerden oluşmalıdır! (Rakam veya özel karakter kullanılamaz: !, @, #, 0 vb.)";
                txtBrand.Focus();
                return false;
            }

            // Model kontrolü: harf + rakam + boşluk + tire (örn: 3008, Egea, Corolla)
            if (string.IsNullOrWhiteSpace(model))
            {
                errorMessage = "Araç modeli boş bırakılamaz!";
                txtModel.Focus();
                return false;
            }
            if (model.Length < 1)
            {
                errorMessage = "Model en az 1 karakter olmalıdır!";
                txtModel.Focus();
                return false;
            }
            if (!Regex.IsMatch(model, @"^[a-zA-Z0-9çÇğĞıİöÖşŞüÜâÂîÎûÛ\s\-\.]+$"))
            {
                errorMessage = "Model sadece harf, rakam, tire ve nokta içerebilir! (Özel karakter kullanılamaz: !, @, # vb.)";
                txtModel.Focus();
                return false;
            }

            // Üretim Yılı Kontrolü (0000, harf, negatif veya saçma yıllar için net hata)
            string yearText = txtYear.Text.Trim();
            if (string.IsNullOrWhiteSpace(yearText))
            {
                errorMessage = "Üretim yılı boş bırakılamaz!";
                txtYear.Focus();
                return false;
            }
            if (!int.TryParse(yearText, out int parsedYear))
            {
                errorMessage = "Üretim yılı yalnızca 4 haneli sayısal bir yıl olmalıdır! (Örn: 2022)";
                txtYear.Focus();
                return false;
            }
            int maxYear = DateTime.Now.Year + 1;
            if (parsedYear < 1990 || parsedYear > maxYear)
            {
                errorMessage = $"Geçersiz üretim yılı! Araç üretim yılı 1990 ile {maxYear} arasında olmalıdır. ('{yearText}' kabul edilmez)";
                txtYear.Focus();
                return false;
            }

            // Günlük ücret kontrolü: NumericUpDown zaten minimum 1 ama ekstra kontrol
            if (numDailyRate.Value <= 0)
            {
                errorMessage = "Günlük kiralama ücreti 0'dan büyük olmalıdır!";
                numDailyRate.Focus();
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs(out string errorMsg))
                {
                    MessageBox.Show(errorMsg, "Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string plate = txtPlate.Text.Trim();
                string brand = txtBrand.Text.Trim();
                string model = txtModel.Text.Trim();
                int year = int.Parse(txtYear.Text.Trim());
                decimal dailyRate = numDailyRate.Value;

                Vehicle newVehicle;
                if (cmbVehicleType.SelectedIndex == 0) // Car
                {
                    int cap = (int)numCapacity.Value;
                    string trans = cmbTransmission.SelectedItem?.ToString() ?? "Otomatik";
                    newVehicle = new Car(plate, brand, model, year, dailyRate, cap, trans);
                }
                else // SUV
                {
                    bool is4x4 = chk4x4.Checked;
                    newVehicle = new Suv(plate, brand, model, year, dailyRate, is4x4);
                }

                _rentalManager.AddVehicle(newVehicle);
                MessageBox.Show("Yeni araç başarıyla sisteme eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadVehiclesToGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Doğrulama / Kayıt Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvVehicles.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen tablodan güncellenecek bir araç seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ValidateInputs(out string errorMsg))
                {
                    MessageBox.Show(errorMsg, "Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!dgvVehicles.Columns.Contains("Id")) return;
                var cell = dgvVehicles.SelectedRows[0].Cells["Id"];
                string id = cell?.Value?.ToString() ?? "";
                string plate = txtPlate.Text.Trim();
                string brand = txtBrand.Text.Trim();
                string model = txtModel.Text.Trim();
                int year = int.Parse(txtYear.Text.Trim());
                decimal dailyRate = numDailyRate.Value;

                _rentalManager.UpdateVehicle(id, plate, brand, model, year, dailyRate);
                MessageBox.Show("Araç bilgileri başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadVehiclesToGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvVehicles.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen silmek istediğiniz aracı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!dgvVehicles.Columns.Contains("Id")) return;
                var cell = dgvVehicles.SelectedRows[0].Cells["Id"];
                string id = cell?.Value?.ToString() ?? "";
                var confirm = MessageBox.Show("Seçili aracı silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    _rentalManager.DeleteVehicle(id);
                    MessageBox.Show("Araç başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadVehiclesToGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme Engellendi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            try
            {
                lblSelectedId.Text = "Seçili ID: Yok (Yeni Kayıt)";
                txtPlate.Clear();
                txtBrand.Clear();
                txtModel.Clear();
                txtYear.Text = "2023";
                numDailyRate.Value = 1000;
                cmbVehicleType.SelectedIndex = 0;
                numCapacity.Value = 5;
                cmbTransmission.SelectedIndex = 0;
                chk4x4.Checked = false;
                if (dgvVehicles != null && !dgvVehicles.IsDisposed)
                    dgvVehicles.ClearSelection();
            }
            catch { }
        }
    }
}
