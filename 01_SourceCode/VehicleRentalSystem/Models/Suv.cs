using System;

namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// SUV ve Arazi araçlarını temsil eder.
    /// Vehicle temel sınıfından türetilmiştir (Kalıtım / Inheritance).
    /// </summary>
    public class Suv : Vehicle
    {
        private bool _fourWheelDrive; // 4x4 özelliği

        public bool FourWheelDrive
        {
            get => _fourWheelDrive;
            set => _fourWheelDrive = value;
        }

        public override string VehicleType => "SUV / Arazi";

        public Suv() : base()
        {
            _fourWheelDrive = true;
        }

        public Suv(string plate, string brand, string model, int year, decimal dailyRate, bool fourWheelDrive = true)
            : base(plate, brand, model, year, dailyRate)
        {
            this.FourWheelDrive = fourWheelDrive;
        }

        /// <summary>
        /// Çok Biçimlilik (Polymorphism):
        /// SUV araçlarda arazi kasko primi eklenir: 4x4 AWD araçlar için %25, 4x2 araçlar için %15.
        /// </summary>
        public override decimal CalculateRentalPrice(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Kiralama süresi en az 1 gün olmalıdır!");

            decimal basePrice = DailyRate * days;
            // 4x4 araçlarda zorunlu arazi kaskosu %25, 4x2 için şehir içi SUV kaskosu %15
            decimal surchargeRate = FourWheelDrive ? 1.25m : 1.15m;
            decimal suvPrice = basePrice * surchargeRate;
            return Math.Round(suvPrice, 2);
        }

        public override string GetDetails()
        {
            return $"{base.GetDetails()} | Çekiş: {(FourWheelDrive ? "4x4 AWD" : "4x2")}";
        }
    }
}
