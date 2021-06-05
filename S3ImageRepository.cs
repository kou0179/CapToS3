using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CapToS3
{
    public class S3ImageRepository : IImageRepository
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        private readonly string _regionName;

        public S3ImageRepository(string regionName, string accessKey, string secretKey, string bucketName)
        {
            _regionName = regionName;
            _bucketName = bucketName;

            var region = RegionEndpoint.GetBySystemName(regionName);
            _s3Client = new(accessKey, secretKey, region);
        }

        public async Task<string> SaveAsync(Image image)
        {
            using (MemoryStream stream = new())
            {
                image.Save(stream, ImageFormat.Png);
                await stream.FlushAsync();

                var key = DateTime.Now.ToString("yyyyMMddhhmmss") + Guid.NewGuid() + ".png";
                await _s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest()
                {
                    BucketName = _bucketName,
                    ContentType = "image/png",
                    Key = key,
                    InputStream = stream,
                    
                }).ConfigureAwait(false);

                return $"https://{_bucketName}.s3-{_regionName}.amazonaws.com/{key}";
            }
        }
    }
}
