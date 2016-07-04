using System;
using System.Linq;

namespace ImageDis
{
    public class ImageDisOptions
    {
        public ImageDisOptions(
            IStorageProvider storageProvider,
            string path = null,
            ImageType[] allowedImageTypes = null,
            IKeyGenerator keyGenerator = null)
        {
            if (storageProvider == null)
                throw new ArgumentNullException("storageProvider");

            StorageProvider = storageProvider;

            Path = string.IsNullOrWhiteSpace(path) ? "/imagedis" : path;

            AllowedImageTypes = allowedImageTypes == null || !allowedImageTypes.Any() 
                ? new[] { ImageTypes.Jpeg, ImageTypes.Png } 
                : allowedImageTypes;

            KeyGenerator = keyGenerator ?? new RandomKeyGenerator();
        }

        public string Path { get; set; }

        public ImageType[] AllowedImageTypes { get; set; }

        public IKeyGenerator KeyGenerator { get; set; }

        public IStorageProvider StorageProvider { get; set; }
    }
}
