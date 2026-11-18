using System;

namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// Kiralama işlem kaydını temsil eden model sınıfı.
    /// Bir müşterinin belirli bir aracı hangi tarihler arasında kiraladığını ve ücret detaylarını tutar.
    /// </summary>
    public class Rental
    {
        private string _id = string.Empty;
        private Customer _customer = null!;
        private Vehicle _vehicle = null!;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _totalDays;
        private decimal _totalPrice;
        private RentalStatus _status;

        public string Id
        {
            get => _id;
            private set => _id = value;
        }

        public Customer Customer
        {
            get => _customer;
            set => _customer = value ?? throw new ArgumentNullException(nameof(value), "Müşteri bilgisi boş olamaz!");
        }

        public Vehicle Vehicle
        {
            get => _vehicle;
            set => _vehicle = value ?? throw new ArgumentNullException(nameof(value), "Araç bilgisi boş olamaz!");
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = value;
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (value.Date < _startDate.Date)
                    throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz!");
                _endDate = value;
            }
        }

        public int TotalDays
        {
            get => _totalDays;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("Toplam gün sayısı en az 1 olmalıdır!");
                _totalDays = value;
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            private set => _totalPrice = value;
        }

        public RentalStatus Status
        {
            get => _status;
            set => _status = value;
        }

        // Parametresiz Yapıcı
        public Rental()
        {
            this._id = "RNT-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            this._startDate = DateTime.Today;
            this._endDate = DateTime.Today.AddDays(1);
            this._status = RentalStatus.Active;
        }

        // Parametreli Yapıcı
        public Rental(Customer customer, Vehicle vehicle, DateTime startDate, DateTime endDate)
        {
            this._id = "RNT-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            this._customer = customer ?? throw new ArgumentNullException(nameof(customer));
            this._vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            this.StartDate = startDate;
            this.EndDate = endDate;

            // Gün sayısını hesapla (Aynı gün bile olsa en az 1 gün sayılır)
            int days = (endDate.Date - startDate.Date).Days;
            if (days <= 0) days = 1;
            this.TotalDays = days;

            // Çok biçimli fiyat hesaplama (Polymorphism kullanımı)
            this.TotalPrice = vehicle.CalculateRentalPrice(this.TotalDays);
            this.Status = RentalStatus.Active;
        }

        /// <summary>
        /// İade işlemini tamamlar.
        /// </summary>
        public void CompleteReturn()
        {
            if (Status != RentalStatus.Active)
                throw new InvalidOperationException("Yalnızca aktif kiralamalar iade edilebilir!");

            this.Status = RentalStatus.Completed;
            this.Vehicle.ReturnVehicle();
        }

        public override string ToString()
        {
            return $"[{Id}] {Customer?.FullName} -> {Vehicle?.Plate} ({StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}) - {TotalPrice:C2} [{Status}]";
        }
    }
}
