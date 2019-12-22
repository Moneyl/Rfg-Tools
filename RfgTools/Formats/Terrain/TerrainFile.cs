using System.Collections.Generic;
using System.IO;
using RfgTools.Helpers;

namespace RfgTools.Formats.Terrain
{
    public class TerrainFile
    {
        public int Signature; //Should be 1381123412 or TERR
        public int Version; //Should be 31
        public int NumTextureNameStrings;
        public int TextureNamesSize;
        public int NumFmeshNames;
        public int FmeshNamesSize;
        public int StitchPieceNamesSize;
        public int NumStitchPieceNames;
        public int NumStitchPieces;

        public List<string> TextureNames = new List<string>();
        public List<string> FmeshNames = new List<string>();

        public TerrainFile()
        {

        }

        public void ReadFromBinary(string inputPath)
        {
            using var stream = new FileStream(inputPath, FileMode.Open);
            using var reader = new BinaryReader(stream);

            Signature = reader.ReadInt32();
            Version = reader.ReadInt32();

            NumTextureNameStrings = reader.ReadInt32();
            TextureNamesSize = reader.ReadInt32();

            NumFmeshNames = reader.ReadInt32();
            FmeshNamesSize = reader.ReadInt32();

            StitchPieceNamesSize = reader.ReadInt32();
            NumStitchPieceNames = reader.ReadInt32();
            NumStitchPieces = reader.ReadInt32();

            //Read texture names
            for (int i = 0; i < NumTextureNameStrings; i++)
            {
                TextureNames.Add(reader.ReadNullTerminatedString());
                //Num of null bytes following string varies. Either 2 or 3
                while (reader.PeekUshort() == 0)
                {
                    reader.Skip(1);
                }
                reader.Skip(1);
            }

            var a = 2;

            ushort val = (ushort) '\0';

            //Read fmesh names
            for (int i = 0; i < NumFmeshNames; i++)
            {
                FmeshNames.Add(reader.ReadNullTerminatedString());

            }
        }
    }
}


//struct terrain_header
//{
//    unsigned int signature;
//    unsigned int version;

//    unsigned int num_texture_name_strings;
//    unsigned int texture_names_size;

//    unsigned int num_fmesh_names;
//    unsigned int fmesh_names_size;

//    unsigned int stitch_piece_names_size;
//    unsigned int num_stitch_piece_names;
//    unsigned int num_stitch_pieces;
//};

//struct terrain_stitch_info
//{
//    vector2 m_bmin;
//    vector2 m_bmax;
//    et_ptr_offset<char,0> m_filename;
//};