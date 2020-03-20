using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;

namespace RfgTools.Formats.Textures
{
    public class PegEntry
    {
        public uint data; //Data offset of the entry in the gpeg file.
        public ushort width;
        public ushort height;
        public PegFormat bitmap_format; //Todo: Make an enum for this. //Todo: Figure out if rfg actually supports all values in the enum.
        public ushort source_width; //Todo: Figure out if really two byte values, actual var type is peg_entry::peg_flag_and_size_union 
        public ushort anim_tiles_width;
        public ushort anim_tiles_height;
        public ushort num_frames;
        public TextureFlags flags;
        public uint filename; //Filename offset for this entry in the header file?
        public ushort source_height; //Todo: Figure out if really two byte values, actual var type is peg_entry::peg_flag_and_size_union 
        public byte fps;
        public byte mip_levels;
        public uint frame_size;
        public uint next = 0; //This value and the following 3 are runtime only values AFAIK and are always zero. //Todo: Double check that's true.
        public uint previous = 0;
        public uint cache0 = 0;
        public uint cache1 = 0;

        //Note: This is stored separately from the other entry data. Placed in this class for coding convenience.
        public string Name;

        //Note: Only use these two if Edited == true //Todo: Maybe hide these behind a property that only lets you do that
        //This is the raw/unconverted data. Used an edited texture wasn't imported, to avoid two conversions
        public byte[] RawData;
        //This is stored in the gpu file (gpeg_pc or gvbm_pc) and converted to a bitmap for easy use with the editor.
        public Bitmap Bitmap; //Todo: Figure out if some other type like BitmapImage or ImageSource would work better here.
                              //True if an edited version was imported
        public bool Edited = false;
        //public int GpuFileDataOffset = 0;

        public void Read(BinaryReader header)
        {
            data = header.ReadUInt32();
            width = header.ReadUInt16();
            height = header.ReadUInt16();
            bitmap_format = (PegFormat)header.ReadUInt16();
            source_width = header.ReadUInt16();
            anim_tiles_width = header.ReadUInt16();
            anim_tiles_height = header.ReadUInt16();
            num_frames = header.ReadUInt16();
            flags = (TextureFlags)header.ReadUInt16();
            filename = header.ReadUInt32();
            source_height = header.ReadUInt16();
            fps = header.ReadByte();
            mip_levels = header.ReadByte();
            frame_size = header.ReadUInt32();
            next = header.ReadUInt32();
            previous = header.ReadUInt32();
            cache0 = header.ReadUInt32();
            cache1 = header.ReadUInt32();
        }

        public void Write(BinaryWriter header)
        {
            //Todo: Update these values before writing since some could've changed.
            header.Write(data);
            header.Write(width);
            header.Write(height);
            header.Write((ushort)bitmap_format);
            header.Write(source_width);
            header.Write(anim_tiles_width);
            header.Write(anim_tiles_height);
            header.Write(num_frames);
            header.Write((ushort)flags);
            header.Write(filename);
            header.Write(source_height);
            header.Write(fps);
            header.Write(mip_levels);
            header.Write(frame_size);
            header.Write(next);
            header.Write(previous);
            header.Write(cache0);
            header.Write(cache1);
        }
    }
}
