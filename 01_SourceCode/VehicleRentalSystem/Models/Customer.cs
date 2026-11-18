using System;

namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// Müşteri bilgilerini ve temel müşteri kurallarını temsil eder.
    /// Kapsülleme (Encapsulation) kurallarına sıkı sıkıya bağlıdır.
    /// </summary>
    public class Customer
    {
        private string _id = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _driverLicenseNumber = string.Empty;

        public string Id
        {
            get => _id;
            private set => _id = value;
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Müşteri adı boş bırakılamaz!");
                _firstName = value.Trim();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Müşteri soyadı boş bırakılamaz!");
                _lastName = value.Trim().ToUpper();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Telefon numarası boş bırakılamaz!");
                _phoneNumber = value.Trim();
            }
        }

        public string DriverLicenseNumber
        {
            get => _driverLicenseNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ehliyet numarası boş bırakılamaz!");
                _driverLicenseNumber = value.Trim().ToUpper();
            }
        }

        /// <summary>
        /// Salt okunur özellik (Read-only property): Ad ve soyadı birleştirir.
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        // Parametresiz Yapıcı
        public Customer()
        {
            this._id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        // Parametreli Yapıcı
        public Customer(string firstName, string lastName, string phoneNumber, string driverLicenseNumber)
        {
            this._id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PhoneNumber = phoneNumber;
            this.DriverLicenseNumber = driverLicenseNumber;
        }

        public override string ToString()
        {
            return $"{FullName} (Ehliyet: {DriverLicenseNumber}) - Tel: {PhoneNumber}";
        }
    }
}
