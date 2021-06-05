using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CapToS3
{
    public class AppSettings
    {
        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public string AwsS3Region { get; set; }
        public string AwsS3BucketName { get; set; }

        public static AppSettings ReadAppSettings()
        {
            try
            {
                var json = File.ReadAllText("settings.json");
                return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions() { 
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (FileNotFoundException)
            {
                File.Create("settings.json");
                File.WriteAllText("settings.json", @"{
  ""awsAccessKey"": """",
  ""awsSecretKey"": """",
  ""awsS3Region"": """",
  ""awsS3BucketName"": """"
}
");
                throw;
            }
        }
    }
}
