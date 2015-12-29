using System;
using System.Linq;

namespace ImageDis
{
    public class ImageDisOptions
    {
        public ImageDisOptions(
            IStorageProvider storageProvider,
            IImageTransformProvider imageTransformProvider, 
            string path = null,
            ImageType[] allowedImageTypes = null,
            IKeyGenerator keyGenerator = null)
        {
            if (storageProvider == null)
                throw new ArgumentNullException("storageProvider");

            StorageProvider = storageProvider;

            if (imageTransformProvider == null)
                throw new ArgumentNullException("imageTransformProvider");

            ImageTransformProvider = imageTransformProvider;

            Path = string.IsNullOrWhiteSpace(path) ? "/imagedis" : path;

            AllowedImageTypes = allowedImageTypes == null || !allowedImageTypes.Any() 
                ? new[] { ImageTypes.Jpeg, ImageTypes.Png } 
                : allowedImageTypes;

            KeyGenerator = keyGenerator ?? new RandomKeyGenerator();
        }

        public string Path { get; set; }

        public ImageType[] AllowedImageTypes { get; set; }

        public IKeyGenerator KeyGenerator { get; set; }

        public IImageTransformProvider ImageTransformProvider { get; set; }

        public IStorageProvider StorageProvider { get; set; }
    }
}
