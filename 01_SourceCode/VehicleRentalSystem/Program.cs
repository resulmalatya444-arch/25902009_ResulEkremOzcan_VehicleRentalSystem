using System;
using System.IO;
using System.Windows.Forms;
using VehicleRentalSystem.Forms;

namespace VehicleRentalSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Tüm beklenmeyen özel durumları yakalayıp kullanıcıya net hata detayı gösteren global hata yöneticisi
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => HandleException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    HandleException(ex);
            };

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void HandleException(Exception ex)
        {
            string err = $"Hata: {ex.Message}\n\nKonum:\n{ex.StackTrace}";
            try
            {
                File.AppendAllText("error_log.txt", $"[{DateTime.Now}] {err}\n\n");
            }
            catch { }
            MessageBox.Show(err, "Uygulama Hatası Detayı", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
