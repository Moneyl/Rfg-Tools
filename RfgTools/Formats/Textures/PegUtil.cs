using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using RfgTools.Dependencies;

namespace RfgTools.Formats.Textures
{
    public static class PegUtil
    {
        public static string ReadNullTerminatedString(BinaryReader stream)
        {
            var String = new StringBuilder();
            do
            {
                String.Append(stream.ReadChar()); //Since the character isn't a null byte, add it to the string
            }
            while (stream.PeekChar() != 0); //Read bytes until a null byte (string terminator) is reached

            stream.ReadByte(); //Read past the null terminator
            return String.ToString();
        }

        public static Bitmap RawDataToBitmap(byte[] rawData, PegFormat format, ushort width, ushort height)
        {
            if (format == PegFormat.PC_DXT1)
            {
                var decompressBuffer = Squish.Decompress(rawData, width, height, Squish.Flags.DXT1);
                return MakeBitmapFromDXT(width, height, decompressBuffer, true);
            }
            else if (format == PegFormat.PC_DXT3)
            {
                var decompressBuffer = Squish.Decompress(rawData, width, height, Squish.Flags.DXT3);
                return MakeBitmapFromDXT(width, height, decompressBuffer, true);
            }
            else if (format == PegFormat.PC_DXT5)
            {
                var decompressBuffer = Squish.Decompress(rawData, width, height, Squish.Flags.DXT5);
                return MakeBitmapFromDXT(width, height, decompressBuffer, true);
            }
            else if (format == PegFormat.PC_8888)
            {
                return MakeBitmapFromPc8888(width, height, rawData);
            }
            else
            {
                throw new Exception($"Unsupported PEG data format detected! {format.ToString()} is not yet supported.");
            }
        }

        public static Bitmap MakeBitmapFromPc8888(uint width, uint height, byte[] buffer)
        {
            Bitmap bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
            int pos = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    //Assuming each pixel is a bgra color with one byte per component
                    var color = new Color();
                    int b = buffer[pos];
                    int g = buffer[pos + 1];
                    int r = buffer[pos + 2];
                    int a = buffer[pos + 3];
                    bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                    pos += 4;
                }
            }
            return bitmap;
        }

        public static Bitmap MakeBitmapFromDXT(uint width, uint height, byte[] buffer, bool keepAlpha)
        {
            Bitmap bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
            for (uint num = 0u; num < width * height * 4u; num += 4u)
            {
                byte b = buffer[(int)((UIntPtr)num)];
                buffer[(int)((UIntPtr)num)] = buffer[(int)((UIntPtr)(num + 2u))];
                buffer[(int)((UIntPtr)(num + 2u))] = b;
            }
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(buffer, 0, bitmapData.Scan0, (int)(width * height * 4u));
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        //public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png); //Use here to keep transparency
        //    BitmapImage image = new BitmapImage();
        //    image.BeginInit();
        //    ms.Seek(0, SeekOrigin.Begin);
        //    image.StreamSource = ms;
        //    image.EndInit();
        //    return image;
        //}

        public static byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                byte[] data = new byte[bitmap.Width * bitmap.Height * 4]; //* 3 for rgb

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Marshal.ReadByte(bitmapData.Scan0, i);
                }
                bitmap.UnlockBits(bitmapData);

                //At this point the data is in BGRA arrangement, need to make it RGBA for DXT compression purposes.
                var redChannel = new byte[data.Length / 4];
                var greenChannel = new byte[data.Length / 4];
                var blueChannel = new byte[data.Length / 4];
                var alphaChannel = new byte[data.Length / 4];

                int pixelIndex = 0;
                for (int i = 0; i < data.Length - 3; i += 4)
                {
                    blueChannel[pixelIndex] = data[i];
                    greenChannel[pixelIndex] = data[i + 1];
                    redChannel[pixelIndex] = data[i + 2];
                    alphaChannel[pixelIndex] = data[i + 3];
                    pixelIndex++;
                }

                pixelIndex = 0;
                for (int i = 0; i < data.Length - 3; i += 4)
                {
                    data[i] = redChannel[pixelIndex];
                    data[i + 1] = greenChannel[pixelIndex];
                    data[i + 2] = blueChannel[pixelIndex];
                    data[i + 3] = alphaChannel[pixelIndex];
                    pixelIndex++;
                }

                return data;
            }
            //else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            //{

            //}
            else
            {
                throw new Exception($"Texture import failed! {bitmap.PixelFormat.ToString()} is currently an unsupported import pixel format.");
            }
        }
    }
}
