using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RfgTools.Dependencies
{
    class Squish
    {
        [DllImport("squish.dll", EntryPoint = "DecompressImage")]
        private static extern void DecompressRaw([MarshalAs(UnmanagedType.LPArray)] byte[] rgba, uint width, uint height, [MarshalAs(UnmanagedType.LPArray)] byte[] blocks, int flags);

        [DllImport("squish.dll", EntryPoint = "CompressImage")]
        private static extern void Compress_Raw([MarshalAs(UnmanagedType.LPArray)] byte[] rgba, uint width, uint height, [MarshalAs(UnmanagedType.LPArray)] byte[] blocks, int flags); //additional arg: float* metric = 0

        [DllImport("squish.dll", EntryPoint = "GetStorageRequirements")]
        private static extern int GetStorageRequirements(uint width, uint height, int flags);

        //public static byte[] Decompress(byte[] output, uint width, uint height, byte[] input, Flags flags)
        public static byte[] Decompress(byte[] input, uint width, uint height, Flags flags)
        {
            var decompressBuffer = new byte[width * height * 4];
            DecompressRaw(decompressBuffer, width, height, input, (int)flags);
            return decompressBuffer;
        }

        public static byte[] Compress(byte[] input, uint width, uint height, Flags flags)
        {
            var compressBuffer = new byte[GetStorageRequirements(width, height, (int)flags)];
            Compress_Raw(input, width, height, compressBuffer, (int)flags);
            return compressBuffer;
        }

        public enum Flags
        {
            DXT1 = 1,
            DXT3 = 2,
            DXT5 = 4,
            ColourClusterFit = 8,
            ColourRangeFit = 16,
            ColourMetricPerceptual = 32,
            ColourMetricUniform = 64,
            WeightColourByAlpha = 128,
            ColourIterativeClusterFit = 256
        }
    }
}
