# ImageDis

ImageDis is middleware that exposes an endpoint on your website which allows you to upload new images, and retrieve the original or resized variants of that image.


# Getting Started

First you'll need to grab the latest packages from NuGet.

* [ImageDis](https://www.nuget.org/packages/ImageDis/) - The ImageDis Core library
* [ImageDis.Owin](https://www.nuget.org/packages/ImageDis.Owin/) or [ImageDis.AspNet](https://www.nuget.org/packages/ImageDis.AspNet/) - Your choice of ImageDis middleware for Owin or the newer AspNet framework.
* [ImageDis.S3](https://www.nuget.org/packages/ImageDis.S3/) - A storage provider based on AWS S3.
* [ImageDis.ImageProcessor](https://www.nuget.org/packages/ImageDis.ImageProcessor/) - An image transform provider based on the ImageProcessor library.

Once the packages have been added to your project, attach ImageDis to your request pipeline like the below code snippets. In this example we're telling ImageDis what storage and image transform provider to use.

```
using ImageDis;
using ImageDis.AspNet;
using ImageDis.S3;
using ImageDis.ImageProcessor;
```

```
app.UseImageDis(new ImageDisOptions(
    new S3StorageProvider(
        awsClientId,
        awsClientSecret, 
        regionEndpoint, 
        bucketName, 
        "https://your-bucket.amazonaws.com/"
    ),
    new ImageProcessorImageTransformProvider()
));
```


# Uploading an image

The following is only an example snippet but demonstrates the POST required to upload an image. The image will be uploaded to the configured storage provider. The name of the file does not matter as ImageDis will grab the first image submitted.

```html
<form action="/imagedis" method="post" enctype="multipart/form-data">
    <input name="image" type="file" />
    <button type="submit">Submit!</button>
</form>
```

On a successful upload, the POST responds with a json object detailing the key that this image can be referenced with. You would typically store this key for later reference when wanting to retrieve the image.

```json
{
    "key": "iNaso4as9Dnro1InAs9041mwoapsdD2d",
    "url": "http://your-host.com/imagedis/iNaso4as9Dnro1InAs9041mwoapsdD2d"
}
```


# Retrieving an image

To retrieve an image, simply make  a GET request to the same /imagedis endpoint, including the key of the image, e.g. ```http://your-host.com/imagedis/iNaso4as9Dnro1InAs9041mwoapsdD2d```. This will redirect the client to the originally uploaded image.


# Retrieving a resized image

You can also retrieve resized variants of images by passing additional query string parameters. ImageDis will resize and save these variants to the storage provider, using the configured image transform provider. These parameters are:
* w (integer) - the new width of the image in pixels.
* h (integer) - the new height of the image in pixels.
* padded (boolean) - whether to pad the image instead of cropping when the dimensions don't match the original image ratio.

Example requests:
* ```http://your-host.com/imagedis/iNaso4as9Dnro1InAs9041mwoapsdD2d?w=300``` - will resize the image to a width of 300 and a height that maintains the original images aspect ratio.
* ```http://your-host.com/imagedis/iNaso4as9Dnro1InAs9041mwoapsdD2d?w=300&h=300``` - will resize the image to a width and height of 300px, center and then crop parts that won't fit within the new aspect ratio.
* ```http://your-host.com/imagedis/iNaso4as9Dnro1InAs9041mwoapsdD2d?w=300&h=300&padded=true``` - will resize the entire image to fit a width and height of 300px, and will fill the rest of the image with padding to ensure it is 300x300.


# Contribute
It's still early days for ImageDis and the library is extremely basic. If you have any requests, let me know or contribute by making a pull request.
