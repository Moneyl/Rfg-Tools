using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace OGE.Helpers
{
    public static class CompressionHelpers
    {
        public static bool TryZlibInflate(byte[] compressedData, uint decompressedDataSize, out byte[] decompressedData, out int decompressedSizeResult)
        {
            decompressedData = new byte[decompressedDataSize];
            decompressedSizeResult = 0;
            
            using (MemoryStream memory = new MemoryStream(compressedData))
            {
                using (InflaterInputStream inflater = new InflaterInputStream(memory))
                {
                    decompressedSizeResult = inflater.Read(decompressedData, 0, (int)decompressedDataSize);
                }
            }

            return decompressedSizeResult == decompressedDataSize;
        }

        public static long GetCompressionSizeResult(byte[] uncompressedData)
        {
            var compressedData = new byte[int.MaxValue / 4]; //Todo: Determine if this needs to be bigger
            using var memory = new MemoryStream(uncompressedData);
            using var deflater = new DeflaterOutputStream(memory);
            deflater.Write(compressedData, 0, uncompressedData.Length);
            return deflater.Length;
        }
    }
}