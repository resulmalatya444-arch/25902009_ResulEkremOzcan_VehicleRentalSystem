using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Services;

namespace VehicleRentalSystem.Forms
{
    public class ReturnForm : Form
    {
        private readonly RentalManager _rentalManager;

        private DataGridView dgvActiveRentals = null!;
        private Label lblSelectedInfo = null!;
        private Button btnCompleteReturn = null!;
        private bool _isLoading = false;

        public ReturnForm(RentalManager rentalManager)
        {
            _rentalManager = rentalManager;
            InitializeCustomComponents();
            this.Load += ReturnForm_Load;
        }

        private void ReturnForm_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadActiveRentals();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Aktif kiralamalar yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Araç İade İşlemleri | Vehicle Return Operations";
            this.Size = new Size(880, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Başlık
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(231, 76, 60)
            };
            Label lblHeader = new Label
            {
                Text = "Kiralanan Araçların İade İşlemi",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(25, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeader);

            // Alt İşlem Paneli
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                Padding = new Padding(25),
                BackColor = Color.White
            };

            lblSelectedInfo = new Label
            {
                Text = "Lütfen iade almak istediğiniz aktif kiralama kaydını yukarıdaki listeden seçiniz.",
                Location = new Point(25, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(50, 60, 70)
            };

            btnCompleteReturn = new Button
            {
                Text = "Seçili Kiralama İçin İadeyi Tamamla (Aracı Müsait Yap)",
                Location = new Point(25, 45),
                Size = new Size(450, 42),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnCompleteReturn.Click += BtnCompleteReturn_Click;

            Button btnClose = new Button
            {
                Text = "Kapat",
                Location = new Point(490, 45),
                Size = new Size(130, 42),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            pnlBottom.Controls.Add(lblSelectedInfo);
            pnlBottom.Controls.Add(btnCompleteReturn);
            pnlBottom.Controls.Add(btnClose);

            // Orta Tablo Alanı
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25)
            };

            dgvActiveRentals = new DataGridView
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
            // SelectionChanged event bağlama LoadActiveRentals sonrası yapılacak

            pnlGrid.Controls.Add(dgvActiveRentals);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);
        }

        private void LoadActiveRentals()
        {
            _isLoading = true;
            try
            {
                dgvActiveRentals.SelectionChanged -= DgvActiveRentals_SelectionChanged;
                dgvActiveRentals.DataSource = null;

                var activeRentals = _rentalManager.GetActiveRentals();

                // Null kontrolü ile güvenli LINQ
                var activeList = activeRentals
                    .Where(r => r != null && r.Customer != null && r.Vehicle != null)
                    .Select(r => new
                    {
                        r.Id,
                        Müşteri = r.Customer.FullName ?? "?",
                        Telefon = r.Customer.PhoneNumber ?? "?",
                        Araç = $"{r.Vehicle.Plate ?? "?"} - {r.Vehicle.Brand ?? "?"} {r.Vehicle.Model ?? "?"}",
                        Tür = r.Vehicle.VehicleType ?? "?",
                        Başlangıç = r.StartDate.ToString("dd.MM.yyyy"),
                        Bitiş = r.EndDate.ToString("dd.MM.yyyy"),
                        Gün = r.TotalDays,
                        Tutar = r.TotalPrice.ToString("C2")
                    }).ToList();

                dgvActiveRentals.DataSource = activeList;

                if (dgvActiveRentals.Columns.Count > 0)
                {
                    if (dgvActiveRentals.Columns.Contains("Id"))
                        dgvActiveRentals.Columns["Id"].Width = 90;
                    if (dgvActiveRentals.Columns.Contains("Gün"))
                        dgvActiveRentals.Columns["Gün"].Width = 60;
                    if (dgvActiveRentals.Columns.Contains("Tutar"))
                        dgvActiveRentals.Columns["Tutar"].Width = 100;
                }

                if (activeList.Count == 0)
                {
                    lblSelectedInfo.Text = "Şu anda iade bekleyen aktif bir kiralama kaydı bulunmuyor.";
                    btnCompleteReturn.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadActiveRentals hatası: " + ex.Message);
            }
            finally
            {
                _isLoading = false;
                dgvActiveRentals.SelectionChanged += DgvActiveRentals_SelectionChanged;
            }
        }

        private void DgvActiveRentals_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isLoading) return;

            try
            {
                if (dgvActiveRentals == null || dgvActiveRentals.IsDisposed) return;
                if (lblSelectedInfo == null || btnCompleteReturn == null) return;
                if (dgvActiveRentals.Columns == null || dgvActiveRentals.Columns.Count == 0) return;

                if (dgvActiveRentals.SelectedRows.Count > 0)
                {
                    var selectedRow = dgvActiveRentals.SelectedRows[0];
                    if (selectedRow == null) return;
                    if (!dgvActiveRentals.Columns.Contains("Id")) return;

                    var cell = selectedRow.Cells["Id"];
                    if (cell == null || cell.Value == null) return;

                    string id = cell.Value.ToString() ?? "";
                    if (string.IsNullOrEmpty(id)) return;

                    var rental = _rentalManager.GetAllRentals().FirstOrDefault(r => r.Id == id);
                    if (rental != null && rental.Customer != null && rental.Vehicle != null)
                    {
                        lblSelectedInfo.Text = $"Seçilen Kayıt: {rental.Id} | {rental.Customer.FullName} | Araç: {rental.Vehicle.Plate} ({rental.Vehicle.Brand}) | Tutar: {rental.TotalPrice:C2}";
                        btnCompleteReturn.Enabled = true;
                        return;
                    }
                }

                btnCompleteReturn.Enabled = false;
            }
            catch
            {
                // UI event koruması
            }
        }

        private void BtnCompleteReturn_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvActiveRentals.SelectedRows.Count == 0) return;
                if (!dgvActiveRentals.Columns.Contains("Id")) return;

                var cell = dgvActiveRentals.SelectedRows[0].Cells["Id"];
                string rentalId = cell?.Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(rentalId)) return;

                var rental = _rentalManager.GetAllRentals().FirstOrDefault(r => r.Id == rentalId);

                if (rental == null)
                {
                    MessageBox.Show("Kayıt bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (rental.Vehicle == null || rental.Customer == null)
                {
                    MessageBox.Show("Kiralama kaydında eksik araç veya müşteri bilgisi!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var confirm = MessageBox.Show($"'{rental.Vehicle.Plate}' plakalı aracın iade işlemini onaylıyor musunuz?\n\nAraç kiralama durumu 'Tamamlandı' yapılacak ve araç yeniden 'Müsait' durumuna getirilecektir.",
                                              "İade Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    _rentalManager.ReturnRental(rentalId);
                    MessageBox.Show("İade işlemi başarıyla tamamlandı!\nAraç artık yeni kiralamalar için Müsait durumdadır.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadActiveRentals();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İade İşlemi Sırasında Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
