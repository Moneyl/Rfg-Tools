using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace OGE.Helpers
{
    public static class CompressionHelpers
    {
        public static bool TryZlibInflate(byte[] compressedData, uint decompressedDataSize, out byte[] decompressedData)
        {
            decompressedData = new byte[decompressedDataSize];
            int decompressedSizeResult = 0;
            
            using (MemoryStream memory = new MemoryStream(compressedData))
            {
                using (InflaterInputStream inflater = new InflaterInputStream(memory))
                {
                    decompressedSizeResult = inflater.Read(decompressedData, 0, (int)decompressedDataSize);
                }
            }

            return decompressedSizeResult == decompressedDataSize;
        }
    }
}