using System;
using System.Collections.Generic;
using System.Text;

namespace RfgTools.Formats.Meshes
{
    public enum PrimitiveType : byte
    {
        Linelist2D = 0,
        TexturedLinelist2D = 1,
        Linelist3D = 2,
        TexturedLinelist3D = 3,
        Tristrip2D = 4,
        TexturedTristrip2D = 5,
        Tristrip3D = 6,
        TexturedTristrip3D = 7,
        BinkedTristrip2D = 8,
        MaterialTristrip2D = 9,
        MaterialTristrip3D = 10,
        Invalid = 255
    }

    //public enum PrimitiveTopology : byte
    //{
    //    TriangleStrip = 0,
    //    LineList = 1,
    //    TriangleList = 2,
    //    PointList = 3,
    //}

    //Above is list pulled from game, this is what is believed to be more accurate based on decompilation
    public enum PrimitiveTopology : byte
    {
        TriangleStrip = 0, //internally D3D10_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP
        //LineList = 1,
        TriangleList = 1, //internally D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST
        //PointList = 3,
    }

    public enum VertexFormat : byte
    {
        Pixlit = 0,
        PixlitCa = 1,
        PixlitNmap = 2,
        PixlitNmapCa = 3,
        Unlit = 4,
        ParticlePointsprite = 5,
        ParticleBillboard = 6,
        ParticleRadial = 7,
        ParticleDrop = 8,
        ParticleRibbon = 9,
        ParticleOriented = 10,
        Primitive3D = 11,
        Primitive2D = 12,
        SgMesh = 13,
        HeightMesh = 14,
        HeightMeshLowLod = 15,
        ParticleParametric = 16,
        Compositor = 17,
        CloneUvs = 18,
        CloneNmap = 19,
        CloneClr = 20,
        Spline2D = 21,
        ParticleCorona = 22,
        ParticleRibbonParametric = 23,
        ConditionalBbox = 24,
        TerrainRoad = 25,
        HeightMeshLandmarkLod = 26,
        StarFieldPoint = 27,
        StarFieldBillboard = 28,
        MeteorShowerLine = 29,
        Pixlit0Uv = 30,
        Pixlit1Uv = 31,
        Pixlit1UvCa = 32,
        Pixlit1UvNmap = 33,
        Pixlit1UvNmapCa = 34,
        Pixlit2Uv = 35,
        Pixlit2UvCa = 36,
        Pixlit2UvNmap = 37,
        Pixlit2UvNmapCa = 38,
        Pixlit3Uv = 39,
        Pixlit3UvCa = 40,
        Pixlit3UvNmap = 41,
        Pixlit3UvNmapCa = 42,
        Pixlit4Uv = 43,
        Pixlit4UvCa = 44,
        Pixlit4UvNmap = 45,
        Pixlit4UvNmapCa = 46,
        Clone1UvUvs = 47,
        Clone2UvUvs = 48,
        UncompressedMorph = 49,
        Invalid = 255
    }
}


    /*
     * 
    enum rl_dev_primitive_topology
    {
        RL_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP = 0x0,
        RL_PRIMITIVE_TOPOLOGY_LINELIST = 0x1,
        RL_PRIMITIVE_TOPOLOGY_TRIANGLELIST = 0x2,
        RL_PRIMITIVE_TOPOLOGY_POINTLIST = 0x3,
    };

    
    enum rl_primitive_render_type
    {
        RLPRT_INVALID = 0xFFFFFFFF,
        RLPRT_LINELIST_2D = 0x0,
        RLPRT_TEXTURED_LINELIST_2D = 0x1,
        RLPRT_LINELIST_3D = 0x2,
        RLPRT_TEXTURED_LINELIST_3D = 0x3,
        RLPRT_TRISTRIP_2D = 0x4,
        RLPRT_TEXTURED_TRISTRIP_2D = 0x5,
        RLPRT_TRISTRIP_3D = 0x6,
        RLPRT_TEXTURED_TRISTRIP_3D = 0x7,
        RLPRT_BINKED_TRISTRIP_2D = 0x8,
        RLPRT_MATERIAL_TRISTRIP_2D = 0x9,
        RLPRT_MATERIAL_TRISTRIP_3D = 0xA,
        NUM_RL_PRIMITIVE_RENDER_TYPES = 0xB,
    };

    enum rl_dev_vertex_format
    {
        RLVF_INVALID = 0xFFFFFFFF,
        RLVF_PIXLIT = 0x0,
        RLVF_PIXLIT_CA = 0x1,
        RLVF_PIXLIT_NMAP = 0x2,
        RLVF_PIXLIT_NMAP_CA = 0x3,
        RLVF_UNLIT = 0x4,
        RLVF_PARTICLE_POINTSPRITE = 0x5,
        RLVF_PARTICLE_BILLBOARD = 0x6,
        RLVF_PARTICLE_RADIAL = 0x7,
        RLVF_PARTICLE_DROP = 0x8,
        RLVF_PARTICLE_RIBBON = 0x9,
        RLVF_PARTICLE_ORIENTED = 0xA,
        RLVF_PRIMITIVE_3D = 0xB,
        RLVF_PRIMITIVE_2D = 0xC,
        RLVF_SG_MESH = 0xD,
        RLVF_HEIGHT_MESH = 0xE,
        RLVF_HEIGHT_MESH_LOW_LOD = 0xF,
        RLVF_PARTICLE_PARAMETRIC = 0x10,
        RLVF_COMPOSITOR = 0x11,
        RLVF_CLONE_UVS = 0x12,
        RLVF_CLONE_NMAP = 0x13,
        RLVF_CLONE_CLR = 0x14,
        RLVF_2D_SPLINE = 0x15,
        RLVF_PARTICLE_CORONA = 0x16,
        RLVF_PARTICLE_RIBBON_PARAMETRIC = 0x17,
        RLVF_CONDITIONAL_BBOX = 0x18,
        RLVF_TERRAIN_ROAD = 0x19,
        RLVF_HEIGHT_MESH_LANDMARK_LOD = 0x1A,
        RLVF_STAR_FIELD_POINT = 0x1B,
        RLVF_STAR_FIELD_BILLBOARD = 0x1C,
        RLVF_METEOR_SHOWER_LINE = 0x1D,
        RLVF_PIXLIT_0UV = 0x1E,
        RLVF_PIXLIT_1UV = 0x1F,
        RLVF_PIXLIT_1UV_CA = 0x20,
        RLVF_PIXLIT_1UV_NMAP = 0x21,
        RLVF_PIXLIT_1UV_NMAP_CA = 0x22,
        RLVF_PIXLIT_2UV = 0x23,
        RLVF_PIXLIT_2UV_CA = 0x24,
        RLVF_PIXLIT_2UV_NMAP = 0x25,
        RLVF_PIXLIT_2UV_NMAP_CA = 0x26,
        RLVF_PIXLIT_3UV = 0x27,
        RLVF_PIXLIT_3UV_CA = 0x28,
        RLVF_PIXLIT_3UV_NMAP = 0x29,
        RLVF_PIXLIT_3UV_NMAP_CA = 0x2A,
        RLVF_PIXLIT_4UV = 0x2B,
        RLVF_PIXLIT_4UV_CA = 0x2C,
        RLVF_PIXLIT_4UV_NMAP = 0x2D,
        RLVF_PIXLIT_4UV_NMAP_CA = 0x2E,
        RLVF_CLONE_1UV_UVS = 0x2F,
        RLVF_CLONE_2UV_UVS = 0x30,
        RLVF_UNCOMPRESSED_MORPH = 0x31,
        RLVF_NUM_FORMATS = 0x32,
    };
     *
     *
     *
     *
     *
     *
     *
     */