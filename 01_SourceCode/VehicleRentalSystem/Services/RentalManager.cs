using System;
using System.Collections.Generic;
using System.Linq;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Services
{
    /// <summary>
    /// Kiralama, araç ve müşteri işlemlerinin yönetildiği servis sınıfı (Business Logic Layer).
    /// Generic List&lt;T&gt; koleksiyonları ile bellek içi yönetim sağlar.
    /// Event (Olay) ve Exception Handling yapılarını içerir.
    /// </summary>
    public class RentalManager
    {
        // Generic List koleksiyonları ile bellek içi veri yönetimi
        private readonly List<Vehicle> _vehicles = new List<Vehicle>();
        private readonly List<Customer> _customers = new List<Customer>();
        private readonly List<Rental> _rentals = new List<Rental>();

        // Veri değiştiğinde UI'ı tetikleyen Olaylar (Events / Action delegates)
        public event Action? DataChanged;

        public RentalManager()
        {
            SeedSampleData();
        }

        #region Koleksiyon Erişimleri (Salt Okunur Kapsülleme)
        public IReadOnlyList<Vehicle> GetAllVehicles() => _vehicles.AsReadOnly();
        public IReadOnlyList<Customer> GetAllCustomers() => _customers.AsReadOnly();
        public IReadOnlyList<Rental> GetAllRentals() => _rentals.AsReadOnly();

        public List<Vehicle> GetAvailableVehicles()
        {
            return _vehicles.Where(v => v.Status == VehicleStatus.Available).ToList();
        }

        public List<Rental> GetActiveRentals()
        {
            return _rentals.Where(r => r.Status == RentalStatus.Active).ToList();
        }
        #endregion

        #region Araç Yönetim İşlemleri
        public void AddVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle), "Araç nesnesi boş olamaz!");

            // Plaka tekrar kontrolü
            if (_vehicles.Any(v => v.Plate.Equals(vehicle.Plate, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"'{vehicle.Plate}' plakalı araç sistemde zaten kayıtlıdır! Plaka tekrarı yapılamaz.");
            }

            _vehicles.Add(vehicle);
            NotifyDataChanged();
        }

        public void UpdateVehicle(string id, string plate, string brand, string model, int year, decimal dailyRate)
        {
            var vehicle = _vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle == null)
                throw new KeyNotFoundException("Güncellenecek araç bulunamadı!");

            // Plaka değiştiyse başka araçta var mı kontrolü
            if (!vehicle.Plate.Equals(plate, StringComparison.OrdinalIgnoreCase))
            {
                if (_vehicles.Any(v => v.Id != id && v.Plate.Equals(plate, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"'{plate}' plakası başka bir araca aittir!");
                }
            }

            vehicle.Plate = plate;
            vehicle.Brand = brand;
            vehicle.Model = model;
            vehicle.Year = year;
            vehicle.DailyRate = dailyRate;

            NotifyDataChanged();
        }

        public void DeleteVehicle(string id)
        {
            var vehicle = _vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle == null)
                throw new KeyNotFoundException("Silinecek araç bulunamadı!");

            // Kiralama kaydı bulunan araçların silinmesi engellenmelidir şartname kuralı
            bool hasRentals = _rentals.Any(r => r.Vehicle.Id == id);
            if (hasRentals)
            {
                throw new InvalidOperationException($"'{vehicle.Plate}' plakalı aracın geçmiş veya aktif kiralama kaydı bulunduğu için silinemez!");
            }

            _vehicles.Remove(vehicle);
            NotifyDataChanged();
        }
        #endregion

        #region Müşteri Yönetim İşlemleri
        public void AddCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer), "Müşteri nesnesi boş olamaz!");

            // 1. Ehliyet no tekrar kontrolü
            if (_customers.Any(c => c.DriverLicenseNumber.Equals(customer.DriverLicenseNumber, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"'{customer.DriverLicenseNumber}' ehliyet numarasına sahip müşteri sistemde zaten kayıtlı!");
            }

            // 2. Telefon numarası tekrar kontrolü (Boşluk/tire temizleyerek karşılaştır)
            string cleanNewPhone = System.Text.RegularExpressions.Regex.Replace(customer.PhoneNumber, @"[^0-9]", "");
            if (_customers.Any(c => System.Text.RegularExpressions.Regex.Replace(c.PhoneNumber, @"[^0-9]", "") == cleanNewPhone))
            {
                throw new InvalidOperationException($"'{customer.PhoneNumber}' telefon numarası sistemde başka bir müşteriye kayıtlı!");
            }

            // 3. İsim + Soyisim çakışma kontrolü
            if (_customers.Any(c => c.FullName.Equals(customer.FullName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"'{customer.FullName}' adında bir müşteri sistemde zaten kayıtlı! Mükerrer müşteri kaydı oluşturulamaz.");
            }

            _customers.Add(customer);
            NotifyDataChanged();
        }

        public void UpdateCustomer(string id, string firstName, string lastName, string phoneNumber, string driverLicense)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
                throw new KeyNotFoundException("Güncellenecek müşteri bulunamadı!");

            // Ehliyet no kontrolü
            if (!customer.DriverLicenseNumber.Equals(driverLicense, StringComparison.OrdinalIgnoreCase))
            {
                if (_customers.Any(c => c.Id != id && c.DriverLicenseNumber.Equals(driverLicense, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"'{driverLicense}' ehliyet numarası başka bir müşteriye ait!");
                }
            }

            // Telefon numarası kontrolü
            string cleanPhone = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^0-9]", "");
            if (_customers.Any(c => c.Id != id && System.Text.RegularExpressions.Regex.Replace(c.PhoneNumber, @"[^0-9]", "") == cleanPhone))
            {
                throw new InvalidOperationException($"'{phoneNumber}' telefon numarası başka bir müşteriye ait!");
            }

            // İsim + Soyisim kontrolü
            string newFullName = $"{firstName.Trim()} {lastName.Trim().ToUpper()}";
            if (_customers.Any(c => c.Id != id && c.FullName.Equals(newFullName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"'{newFullName}' adında başka bir müşteri zaten kayıtlı!");
            }

            customer.FirstName = firstName;
            customer.LastName = lastName;
            customer.PhoneNumber = phoneNumber;
            customer.DriverLicenseNumber = driverLicense;

            NotifyDataChanged();
        }

        public void DeleteCustomer(string id)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
                throw new KeyNotFoundException("Silinecek müşteri bulunamadı!");

            // Aktif kiralaması bulunan müşterinin silinmesini engelle
            bool hasActiveRental = _rentals.Any(r => r.Customer.Id == id && r.Status == RentalStatus.Active);
            if (hasActiveRental)
            {
                throw new InvalidOperationException($"'{customer.FullName}' isimli müşterinin şu anda aktif kiralaması devam ettiği için silinemez!");
            }

            _customers.Remove(customer);
            NotifyDataChanged();
        }
        #endregion

        #region Kiralama ve İade İşlemleri
        public Rental CreateRental(string customerId, string vehicleId, DateTime startDate, DateTime endDate)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null)
                throw new KeyNotFoundException("Kiralama yapılacak müşteri seçilmedi veya bulunamadı!");

            var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException("Kiralanacak araç bulunamadı!");

            if (vehicle.Status != VehicleStatus.Available)
                throw new InvalidOperationException($"'{vehicle.Plate}' plakalı araç şu anda müsait değildir ({vehicle.Status})!");

            if (startDate.Date < DateTime.Today)
                throw new ArgumentException("Kiralama başlangıç tarihi bugünden önceki bir tarih olamaz!");

            if (endDate.Date < startDate.Date)
                throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz!");

            // Yeni kiralama oluştur (Polymorphic fiyat hesabı içeride yapılır)
            var rental = new Rental(customer, vehicle, startDate, endDate);
            
            // Aracın durumunu kiralık yap
            vehicle.Rent();

            _rentals.Add(rental);
            NotifyDataChanged();
            return rental;
        }

        public void ReturnRental(string rentalId)
        {
            var rental = _rentals.FirstOrDefault(r => r.Id == rentalId);
            if (rental == null)
                throw new KeyNotFoundException("İade edilecek kiralama kaydı bulunamadı!");

            if (rental.Status != RentalStatus.Active)
                throw new InvalidOperationException("Bu kiralama kaydı zaten tamamlanmış veya aktif değil!");

            rental.CompleteReturn();
            NotifyDataChanged();
        }
        #endregion

        #region Dashboard / İstatistik Hesaplama
        public int GetTotalVehiclesCount() => _vehicles.Count;
        public int GetAvailableVehiclesCount() => _vehicles.Count(v => v.Status == VehicleStatus.Available);
        public int GetRentedVehiclesCount() => _vehicles.Count(v => v.Status == VehicleStatus.Rented);
        public int GetTotalCustomersCount() => _customers.Count;
        public int GetActiveRentalsCount() => _rentals.Count(r => r.Status == RentalStatus.Active);
        public int GetCompletedRentalsCount() => _rentals.Count(r => r.Status == RentalStatus.Completed);
        public decimal GetTotalRevenue() => _rentals.Sum(r => r.TotalPrice);
        #endregion

        private void NotifyDataChanged()
        {
            DataChanged?.Invoke();
        }

        /// <summary>
        /// Proje ilk açıldığında gösterilmek üzere örnek verileri yükler.
        /// </summary>
        private void SeedSampleData()
        {
            // Örnek Otomobiller (Car)
            var c1 = new Car("34ABC123", "Renault", "Megane", 2022, 1200m, 5, "Otomatik");
            var c2 = new Car("06ANK456", "Toyota", "Corolla", 2023, 1400m, 5, "Otomatik");
            var c3 = new Car("35IZM789", "Fiat", "Egea", 2021, 950m, 5, "Manuel");

            // Örnek SUV Araçlar (Suv)
            var s1 = new Suv("50NEV001", "Dacia", "Duster", 2023, 1600m, true);
            var s2 = new Suv("07ANT999", "Peugeot", "3008", 2024, 2200m, false);

            _vehicles.Add(c1);
            _vehicles.Add(c2);
            _vehicles.Add(c3);
            _vehicles.Add(s1);
            _vehicles.Add(s2);

            // Örnek Müşteriler (Customer)
            var cust1 = new Customer("Ahmet", "YILMAZ", "0532 111 2233", "EHL-99881");
            var cust2 = new Customer("Ayşe", "KAYA", "0544 222 3344", "EHL-77442");
            var cust3 = new Customer("Mehmet", "DEMİR", "0555 333 4455", "EHL-55223");

            _customers.Add(cust1);
            _customers.Add(cust2);
            _customers.Add(cust3);

            // Örnek Kiralama (1 adet aktif kiralama)
            var r1 = new Rental(cust1, c1, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(3));
            c1.Rent();
            _rentals.Add(r1);
        }
    }
}
