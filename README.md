# Rfg Tools
Code for interacting with Red Faction Guerrilla and it's file formats. Huge thanks to [Gibbed](https://github.com/gibbed/) who did the original reverse engineering work for several of the games formats, and who's tools were used as references when designing some of these. Notably the packfile, texture, and asm_pc code. See [Gibbed.Volition](https://github.com/gibbed/Gibbed.Volition) for his work on RFG and several other Volition games.

The goals of this repo are to rewrite any RFG file format code from his repo, fixing any bugs found, and to write new tools for some of the games other unique formats. Unlike Gibbed.Volition this repo will only contain code for RFG formats since it's only intended for use in RFG modding tools, keeping things more manageable.

Note: This repo does not contain any programs you can use directly and is intended to be used by other tools to interact with the games file formats. Any tools using this are found in other repos.

# Contents
- [x] Packfile v3 (vpp_pc & str2_pc) packer
- [ ] Packfile unpacker (Partially complete - WIP)
- [ ] Zone file (rfgzone_pc & layer_pc) reader and writer. Includes to/from xml and binary formats. Technically complete, but zone editing still has many bugs and is unreliable.
- [x] Asm_pc tools. Supports read/write with binary format. No xml support yet.
- [x] Texture files (cpeg/gpeg & cvbm/gvbm). Supports read/write.

# What uses this
- [OGE](https://github.com/Moneyl/OGE/) - A modding tool for RFG still very early in development. Currently has some useful file viewing features like being able to view the contents of a packfile without manually extracting it. Has no editing features yet.
- [Rfg Toolset](https://github.com/Moneyl/RFG-Toolset/) - A set of file format tools using this repo.
