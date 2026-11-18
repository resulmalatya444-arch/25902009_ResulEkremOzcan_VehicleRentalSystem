using System;

namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// Tüm araç türleri için temel soyut sınıf (Base Abstract Class).
    /// Soyutlama (Abstraction), Kapsülleme (Encapsulation) ve Kalıtım (Inheritance) ilkelerini barındırır.
    /// IRentable arayüzünü uygular.
    /// </summary>
    public abstract class Vehicle : IRentable
    {
        // 1. Kapsülleme (Encapsulation): Private alanlar (field)
        private string _id = string.Empty;
        private string _plate = string.Empty;
        private string _brand = string.Empty;
        private string _model = string.Empty;
        private int _year;
        private decimal _dailyRate;
        private VehicleStatus _status;

        // 2. Özellikler (Properties) ve Doğrulama (Validation) Kuralları
        public string Id
        {
            get => _id;
            private set => _id = value;
        }

        public string Plate
        {
            get => _plate;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Araç plakası boş bırakılamaz!");
                _plate = value.Trim().ToUpper();
            }
        }

        public string Brand
        {
            get => _brand;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Araç markası boş bırakılamaz!");
                _brand = value.Trim();
            }
        }

        public string Model
        {
            get => _model;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Araç modeli boş bırakılamaz!");
                _model = value.Trim();
            }
        }

        public int Year
        {
            get => _year;
            set
            {
                if (value < 1990 || value > DateTime.Now.Year + 1)
                    throw new ArgumentException($"Üretim yılı 1990 ile {DateTime.Now.Year + 1} arasında olmalıdır!");
                _year = value;
            }
        }

        public decimal DailyRate
        {
            get => _dailyRate;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Günlük kiralama ücreti 0'dan büyük olmalıdır!");
                _dailyRate = value;
            }
        }

        public VehicleStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public abstract string VehicleType { get; }

        // 3. Constructor Overloading (Yapıcı Metot Aşırı Yükleme)
        /// <summary>
        /// Parametresiz varsayılan yapıcı metot (Default Constructor).
        /// </summary>
        public Vehicle()
        {
            this._id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            this._status = VehicleStatus.Available;
            this._year = DateTime.Now.Year;
        }

        /// <summary>
        /// Parametreli yapıcı metot (Parameterized Constructor).
        /// Alt sınıflar base(...) ile bu yapıcıyı çağırır.
        /// </summary>
        public Vehicle(string plate, string brand, string model, int year, decimal dailyRate)
        {
            this._id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            this.Plate = plate;
            this.Brand = brand;
            this.Model = model;
            this.Year = year;
            this.DailyRate = dailyRate;
            this.Status = VehicleStatus.Available;
        }

        // 4. Çok Biçimlilik (Polymorphism): Soyut ve Sanal Metotlar
        /// <summary>
        /// Çok Biçimlilik (Polymorphism): Araç tipine göre kiralama ücreti hesaplar.
        /// Alt sınıflar bu metodu override ederek türe özgü hesaplama yapar.
        /// </summary>
        public abstract decimal CalculateRentalPrice(int days);

        /// <summary>
        /// Araç detay özetini döner. Alt sınıflar override edebilir.
        /// </summary>
        public virtual string GetDetails()
        {
            return $"[{VehicleType}] {Brand} {Model} ({Year}) - Plaka: {Plate} - Günlük: {DailyRate:C2} - Durum: {Status}";
        }

        public void Rent()
        {
            if (Status == VehicleStatus.Rented)
                throw new InvalidOperationException($"'{Plate}' plakalı araç zaten kiradadır!");
            Status = VehicleStatus.Rented;
        }

        public void ReturnVehicle()
        {
            Status = VehicleStatus.Available;
        }

        public override string ToString()
        {
            return $"{Plate} - {Brand} {Model} ({VehicleType})";
        }
    }
}
