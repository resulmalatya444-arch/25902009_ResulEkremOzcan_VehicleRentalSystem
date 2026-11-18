using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Services;

namespace VehicleRentalSystem.Forms
{
    public class CustomersForm : Form
    {
        private readonly RentalManager _rentalManager;

        private DataGridView dgvCustomers = null!;
        private TextBox txtFirstName = null!;
        private TextBox txtLastName = null!;
        private TextBox txtPhone = null!;
        private TextBox txtLicense = null!;
        private Label lblSelectedId = null!;
        private bool _isLoading = false;

        public CustomersForm(RentalManager rentalManager)
        {
            _rentalManager = rentalManager;
            InitializeCustomComponents();
            this.Load += CustomersForm_Load;
        }

        private void CustomersForm_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadCustomersToGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri listesi yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Müşteri Yönetimi | Customer Management";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Başlık Paneli
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(155, 89, 182)
            };
            Label lblHeader = new Label
            {
                Text = "Müşteri Kayıt, Düzenleme ve Listeleme",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeader);

            // Sol Panel - Form Girişleri
            GroupBox gbInput = new GroupBox
            {
                Text = "Müşteri Bilgileri",
                Dock = DockStyle.Left,
                Width = 320,
                Padding = new Padding(15),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            int curY = 30;
            int stepY = 56;

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

            // Ad
            Label lblFirstName = new Label { Text = "Müşteri Adı *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtFirstName = new TextBox { Location = new Point(15, curY + 18), Width = 280 };
            gbInput.Controls.Add(lblFirstName);
            gbInput.Controls.Add(txtFirstName);
            curY += stepY;

            // Soyad
            Label lblLastName = new Label { Text = "Müşteri Soyadı *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtLastName = new TextBox { Location = new Point(15, curY + 18), Width = 280, CharacterCasing = CharacterCasing.Upper };
            gbInput.Controls.Add(lblLastName);
            gbInput.Controls.Add(txtLastName);
            curY += stepY;

            // Telefon
            Label lblPhone = new Label { Text = "Telefon Numarası *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtPhone = new TextBox { Location = new Point(15, curY + 18), Width = 280 };
            gbInput.Controls.Add(lblPhone);
            gbInput.Controls.Add(txtPhone);
            curY += stepY;

            // Ehliyet No
            Label lblLicense = new Label { Text = "Ehliyet Numarası *", Location = new Point(15, curY), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular) };
            txtLicense = new TextBox { Location = new Point(15, curY + 18), Width = 280, CharacterCasing = CharacterCasing.Upper };
            gbInput.Controls.Add(lblLicense);
            gbInput.Controls.Add(txtLicense);
            curY += stepY + 10;

            // Butonlar
            Button btnSave = new Button
            {
                Text = "Müşteriyi Kaydet (Ekle)",
                Location = new Point(15, curY),
                Width = 280,
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
                Width = 135,
                Height = 35,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdate.Click += BtnUpdate_Click;

            Button btnDelete = new Button
            {
                Text = "Seçileni Sil",
                Location = new Point(160, curY),
                Width = 135,
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
                Width = 280,
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

            // Sağ Panel - GridView
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            dgvCustomers = new DataGridView
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
            // Event bağlama Load'dan sonra yapılacak

            pnlGrid.Controls.Add(dgvCustomers);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(gbInput);
            this.Controls.Add(pnlHeader);
        }

        private void LoadCustomersToGrid()
        {
            _isLoading = true;
            try
            {
                dgvCustomers.SelectionChanged -= DgvCustomers_SelectionChanged;
                dgvCustomers.DataSource = null;

                var list = _rentalManager.GetAllCustomers().Select(c => new
                {
                    c.Id,
                    AdSoyad = c.FullName,
                    Telefon = c.PhoneNumber,
                    EhliyetNo = c.DriverLicenseNumber
                }).ToList();

                dgvCustomers.DataSource = list;

                if (dgvCustomers.Columns.Count > 0)
                {
                    if (dgvCustomers.Columns.Contains("Id"))
                        dgvCustomers.Columns["Id"].Width = 80;
                    if (dgvCustomers.Columns.Contains("AdSoyad"))
                        dgvCustomers.Columns["AdSoyad"].HeaderText = "Müşteri Ad Soyad";
                    if (dgvCustomers.Columns.Contains("Telefon"))
                        dgvCustomers.Columns["Telefon"].HeaderText = "Telefon";
                    if (dgvCustomers.Columns.Contains("EhliyetNo"))
                        dgvCustomers.Columns["EhliyetNo"].HeaderText = "Ehliyet Numarası";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadCustomersToGrid hatası: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
                dgvCustomers.SelectionChanged += DgvCustomers_SelectionChanged;
            }
        }

        private void DgvCustomers_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;

            try
            {
                if (dgvCustomers == null || dgvCustomers.IsDisposed) return;
                if (lblSelectedId == null || txtFirstName == null || txtLastName == null || txtPhone == null || txtLicense == null) return;
                if (dgvCustomers.Columns == null || dgvCustomers.Columns.Count == 0) return;

                if (dgvCustomers.SelectedRows.Count > 0)
                {
                    var selectedRow = dgvCustomers.SelectedRows[0];
                    if (selectedRow == null) return;
                    if (!dgvCustomers.Columns.Contains("Id")) return;

                    var cell = selectedRow.Cells["Id"];
                    if (cell == null || cell.Value == null) return;

                    string id = cell.Value.ToString() ?? "";
                    if (string.IsNullOrEmpty(id)) return;

                    var customer = _rentalManager.GetAllCustomers().FirstOrDefault(c => c.Id == id);
                    if (customer != null)
                    {
                        lblSelectedId.Text = $"Seçili ID: {customer.Id}";
                        txtFirstName.Text = customer.FirstName;
                        txtLastName.Text = customer.LastName;
                        txtPhone.Text = customer.PhoneNumber;
                        txtLicense.Text = customer.DriverLicenseNumber;
                    }
                }
            }
            catch
            {
                // UI eventlerinde sessiz ve güvenli geçiş
            }
        }

        /// <summary>
        /// Form düzeyinde girdi doğrulama. Boş, özel karakter, rakam-only girişleri engeller.
        /// </summary>
        private bool ValidateInputs(out string errorMessage)
        {
            errorMessage = "";

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string license = txtLicense.Text.Trim();

            // Ad kontrolü: boş olamaz, en az 2 karakter, sadece harf ve boşluk
            if (string.IsNullOrWhiteSpace(firstName))
            {
                errorMessage = "Müşteri adı boş bırakılamaz!";
                txtFirstName.Focus();
                return false;
            }
            if (firstName.Length < 2)
            {
                errorMessage = "Müşteri adı en az 2 karakter olmalıdır!";
                txtFirstName.Focus();
                return false;
            }
            if (!Regex.IsMatch(firstName, @"^[a-zA-ZçÇğĞıİöÖşŞüÜâÂîÎûÛ\s]+$"))
            {
                errorMessage = "Müşteri adı yalnızca harflerden oluşmalıdır! (Rakam veya özel karakter kullanılamaz)";
                txtFirstName.Focus();
                return false;
            }

            // Soyad kontrolü
            if (string.IsNullOrWhiteSpace(lastName))
            {
                errorMessage = "Müşteri soyadı boş bırakılamaz!";
                txtLastName.Focus();
                return false;
            }
            if (lastName.Length < 2)
            {
                errorMessage = "Müşteri soyadı en az 2 karakter olmalıdır!";
                txtLastName.Focus();
                return false;
            }
            if (!Regex.IsMatch(lastName, @"^[a-zA-ZçÇğĞıİöÖşŞüÜâÂîÎûÛ\s]+$"))
            {
                errorMessage = "Müşteri soyadı yalnızca harflerden oluşmalıdır! (Rakam veya özel karakter kullanılamaz)";
                txtLastName.Focus();
                return false;
            }

            // Telefon kontrolü: sadece rakam, boşluk ve - olabilir, minimum 10 rakam
            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Telefon numarası boş bırakılamaz!\n\n📌 Örnek Geçerli Format: 0532 111 22 33 veya 0555 444 33 22";
                txtPhone.Focus();
                return false;
            }
            string phoneDigitsOnly = Regex.Replace(phone, @"[^0-9]", "");
            if (phoneDigitsOnly.Length < 10)
            {
                errorMessage = $"Geçersiz telefon numarası! Girdiğiniz numara eksik ({phoneDigitsOnly.Length} hane).\nTelefon numarası en az 10 rakamdan oluşmalıdır.\n\n📌 Örnek Geçerli Format: 0532 111 22 33";
                txtPhone.Focus();
                return false;
            }
            // Sadece sıfırlardan veya aynı rakamın tekrarından mı oluşuyor kontrolü (Örn: 0000000000)
            if (phoneDigitsOnly.Distinct().Count() <= 1)
            {
                errorMessage = "Geçersiz telefon numarası! Numara sahte veya tekrarlayan tek bir rakamdan oluşamaz.\n\n📌 Örnek Geçerli Format: 0542 333 44 55";
                txtPhone.Focus();
                return false;
            }
            if (!Regex.IsMatch(phone, @"^[0-9\s\-\+\(\)]+$"))
            {
                errorMessage = "Telefon numarasında geçersiz karakter tespit edildi! Numara harf veya özel karakter içeremez.\n\n📌 Örnek Geçerli Format: 0532 111 22 33";
                txtPhone.Focus();
                return false;
            }

            // Ehliyet kontrolü: alfanumerik, minimum 6 karakter
            if (string.IsNullOrWhiteSpace(license))
            {
                errorMessage = "Ehliyet numarası boş bırakılamaz!";
                txtLicense.Focus();
                return false;
            }
            if (license.Length < 6)
            {
                errorMessage = "Geçersiz ehliyet numarası! Ehliyet numarası en az 6 karakter olmalıdır! ('000' vb. kabul edilmez)";
                txtLicense.Focus();
                return false;
            }
            if (license.Replace("-", "").Distinct().Count() <= 1)
            {
                errorMessage = "Geçersiz ehliyet numarası! Ehliyet numarası sahte veya tek bir karakterin tekrarından oluşamaz!";
                txtLicense.Focus();
                return false;
            }
            if (!Regex.IsMatch(license, @"^[a-zA-Z0-9\-]+$"))
            {
                errorMessage = "Ehliyet numarası sadece harf, rakam ve tire içerebilir! (Özel karakter kullanılamaz)";
                txtLicense.Focus();
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

                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string license = txtLicense.Text.Trim();

                var customer = new Customer(firstName, lastName, phone, license);
                _rentalManager.AddCustomer(customer);

                MessageBox.Show("Müşteri başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadCustomersToGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvCustomers.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen güncellenecek müşteriyi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ValidateInputs(out string errorMsg))
                {
                    MessageBox.Show(errorMsg, "Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!dgvCustomers.Columns.Contains("Id")) return;
                var cell = dgvCustomers.SelectedRows[0].Cells["Id"];
                string id = cell?.Value?.ToString() ?? "";
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string license = txtLicense.Text.Trim();

                _rentalManager.UpdateCustomer(id, firstName, lastName, phone, license);
                MessageBox.Show("Müşteri bilgileri güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCustomersToGrid();
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
                if (dgvCustomers.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen silinecek müşteriyi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!dgvCustomers.Columns.Contains("Id")) return;
                var cell = dgvCustomers.SelectedRows[0].Cells["Id"];
                string id = cell?.Value?.ToString() ?? "";
                var confirm = MessageBox.Show("Bu müşteriyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    _rentalManager.DeleteCustomer(id);
                    MessageBox.Show("Müşteri başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadCustomersToGrid();
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
                txtFirstName.Clear();
                txtLastName.Clear();
                txtPhone.Clear();
                txtLicense.Clear();
                if (dgvCustomers != null && !dgvCustomers.IsDisposed)
                    dgvCustomers.ClearSelection();
            }
            catch { }
        }
    }
}
