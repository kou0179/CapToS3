using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapToS3
{
    public interface IImageRepository
    {
        public Task<string> SaveAsync(Image image);
    }
}
