using System;
using System.IO;
using RePKG.Application.Exceptions;
using RePKG.Application.Texture.Helpers;
using RePKG.Core.Texture;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RePKG.Application.Texture
{
    public class ImageToTexConverter
    {
        public ITex LoadFileToTex(FileInfo filePath)
        {

            switch (filePath.Extension)
            {
                case ".tex-full-json":
                    {
                        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                        ITex tex = JsonConvert.DeserializeObject<Tex>(File.ReadAllText(filePath.FullName), settings);
                        return tex;
                    }

                default:
                    {
                        Image image = Image.FromFile(filePath.FullName);
                        byte[] imageBytes;

                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, image.RawFormat);
                            imageBytes = ms.ToArray();
                        }

                        // var fullJsonInfo = Newtonsoft.Json.JsonConvert.SerializeObject(image);
                        // File.WriteAllText($"{filePath}.test-json", fullJsonInfo);

                        // var bytes = image.SavePixelData();

                        var tex = new Tex { };

                        tex.Magic1 = "TEXV0005";
                        tex.Magic2 = "TEXI0001";

                        tex.Header = new TexHeader
                        {
                            Format = (TexFormat)0,
                            Flags = (TexFlags)2,
                            TextureWidth = image.Width,
                            TextureHeight = image.Height,
                            ImageWidth = image.Width,
                            ImageHeight = image.Height,
                            UnkInt0 = 4278190080
                        };
                        tex.ImagesContainer = new TexImageContainer { };
                        tex.ImagesContainer.Magic = "TEXB0003";

                        switch (tex.ImagesContainer.Magic)
                        {
                            case "TEXB0001":
                            case "TEXB0002":
                                break;

                            case "TEXB0003":
                                tex.ImagesContainer.ImageFormat = FreeImageFormat.FIF_PNG;
                                break;

                            default:
                                throw new UnknownMagicException(nameof(TexImageContainerReader), tex.ImagesContainer.Magic);
                        }

                        tex.ImagesContainer.ImageContainerVersion = (TexImageContainerVersion)Convert.ToInt32(tex.ImagesContainer.Magic.Substring(4));

                        // var mipmapCount = 1;

                        // if (mipmapCount > Constants.MaximumMipmapCount)
                        //     throw new UnsafeTexException(
                        //         $"Mipmap count exceeds limit: {mipmapCount}/{Constants.MaximumMipmapCount}");

                        // var readFunction = PickMipmapReader(container.ImageContainerVersion);
                        var format = TexMipmapFormatGetter.GetFormatForTex(tex.ImagesContainer.ImageFormat, tex.Header.Format);
                        var teximage = new TexImage();

                        // var mipmap = readFunction(reader);
                        // mipmap.Format = format;

                        // if (DecompressMipmapBytes)
                        //     _texMipmapDecompressor.DecompressMipmap(mipmap);

                        var mipmap = new TexMipmap { };

                        mipmap.Format = MipmapFormat.ImagePNG;

                        mipmap.Width = image.Width;
                        mipmap.Height = image.Height;

                        mipmap.Bytes = imageBytes;

                        teximage.Mipmaps.Add(mipmap);

                        tex.ImagesContainer.Images.Add(teximage);

                        return tex;
                    }
            }
        }
    }
}