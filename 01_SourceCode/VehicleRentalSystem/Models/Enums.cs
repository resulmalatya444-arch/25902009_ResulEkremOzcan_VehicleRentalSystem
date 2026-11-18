namespace VehicleRentalSystem.Models
{
    /// <summary>
    /// Aracın kiralama durumunu belirten enum.
    /// </summary>
    public enum VehicleStatus
    {
        Available,  // Müsait
        Rented,     // Kirada
        Maintenance // Bakımda (opsiyonel)
    }

    /// <summary>
    /// Kiralama işleminin durumunu belirten enum.
    /// </summary>
    public enum RentalStatus
    {
        Active,     // Aktif Kiralama
        Completed,  // İade Edildi / Tamamlandı
        Cancelled   // İptal Edildi
    }
}
