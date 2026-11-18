namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// Kiralanabilir nesneler için sözleşme (Interface).
    /// Soyutlama (Abstraction) ilkesini sağlar.
    /// </summary>
    public interface IRentable
    {
        string Id { get; }
        string Plate { get; }
        decimal DailyRate { get; }
        VehicleStatus Status { get; }

        /// <summary>
        /// Gün sayısına göre kira tutarını hesaplar.
        /// </summary>
        decimal CalculateRentalPrice(int days);

        /// <summary>
        /// Aracı kiralık duruma geçirir.
        /// </summary>
        void Rent();

        /// <summary>
        /// Aracı iade alıp müsait duruma getirir.
        /// </summary>
        void ReturnVehicle();
    }
}
