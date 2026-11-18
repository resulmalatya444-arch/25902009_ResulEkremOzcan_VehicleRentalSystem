using System;

namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// Otomobil türündeki araçları temsil eder.
    /// Vehicle temel sınıfından türetilmiştir (Inheritance / Kalıtım).
    /// </summary>
    public class Car : Vehicle
    {
        private int _seatingCapacity;
        private string _transmissionType = "Otomatik"; // Düz / Otomatik

        public int SeatingCapacity
        {
            get => _seatingCapacity;
            set
            {
                if (value <= 0 || value > 9)
                    throw new ArgumentException("Koltuk kapasitesi 1 ile 9 arasında olmalıdır!");
                _seatingCapacity = value;
            }
        }

        public string TransmissionType
        {
            get => _transmissionType;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Vites türü boş olamaz!");
                _transmissionType = value.Trim();
            }
        }

        public override string VehicleType => "Otomobil";

        // Parametresiz Constructor
        public Car() : base()
        {
            this._seatingCapacity = 5;
            this._transmissionType = "Otomatik";
        }

        // Parametreli Constructor - base(...) kullanımı ile temel sınıf başlatılır
        public Car(string plate, string brand, string model, int year, decimal dailyRate, int seatingCapacity = 5, string transmission = "Otomatik")
            : base(plate, brand, model, year, dailyRate)
        {
            this.SeatingCapacity = seatingCapacity;
            this.TransmissionType = transmission;
        }

        /// <summary>
        /// Çok Biçimlilik (Polymorphism):
        /// Otomobil kiralama ücreti hesaplama (Haftalık kiralamada %10 indirim uygulanır).
        /// </summary>
        public override decimal CalculateRentalPrice(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Kiralama süresi en az 1 gün olmalıdır!");

            decimal basePrice = DailyRate * days;

            // 7 gün ve üzeri otomobil kiralamalarında %10 kampanya indirimi
            if (days >= 7)
            {
                return Math.Round(basePrice * 0.90m, 2);
            }

            return Math.Round(basePrice, 2);
        }

        public override string GetDetails()
        {
            return $"{base.GetDetails()} | Koltuk: {SeatingCapacity} | Vites: {TransmissionType}";
        }
    }
}
