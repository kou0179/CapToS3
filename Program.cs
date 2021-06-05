using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapToS3
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var settings = AppSettings.ReadAppSettings();

            S3ImageRepository s3ImageRepository = new(settings.AwsS3Region, settings.AwsAccessKey, settings.AwsSecretKey, settings.AwsS3BucketName);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CaptureAreaForm(s3ImageRepository));
        }
    }
}
