//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.IO;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// This class contains encoders and decoders for the Westwood XOR Delta and LCW compression schemes.
    /// </summary>
    public static class WWCompression
    {
        ////////////////////////////////////////////////////////////////////////////////
        //  Copyright Notice
        ////////////////////////////////////////////////////////////////////////////////
        // This code is free software: you can redistribute it and/or modify
        // it under the terms of the GNU General Public License as published by
        // the Free Software Foundation, either version 2 of the License, or
        // (at your option) any later version.
        //
        // This code is distributed in the hope that it will be useful,
        // but WITHOUT ANY WARRANTY; without even the implied warranty of
        // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        // GNU General Public License for more details.
        //
        // You should have received a copy of the GNU General Public License
        // along with this code.  If not, see <http://www.gnu.org/licenses/>.
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////
        //  Notes
        ////////////////////////////////////////////////////////////////////////////////
        //
        // LCW streams should always start and end with the fill command (& 0x80) though
        // the decompressor doesn't strictly require that it start with one the ability
        // to use the offset commands in place of the RLE command early in the stream
        // relies on it. Streams larger than 64k that need the relative versions of the
        // 3 and 5 byte commands should start with a null byte before the first 0x80
        // command to flag that they are relative compressed.
        //
        // LCW uses the following rules to decide which command to use:
        // 1. Runs of the same colour should only use 4 byte RLE command if longer than
        //    64 bytes. 2 and 3 byte offset commands are more efficient otherwise.
        // 2. Runs of less than 3 should just be stored as is with the one byte fill
        //    command.
        // 3. Runs greater than 10 or if the relative offset is greater than
        //    4095 use an absolute copy. Less than 64 bytes uses 3 byte command, else it
        //    uses the 5 byte command.
        // 4. If Absolute rule isn't met then copy from a relative offset with 2 byte
        //    command.
        //
        // Absolute LCW can efficiently compress data that is 64k in size, much greater
        // and relative offsets for the 3 and 5 byte commands are needed.
        //
        // The XOR delta generator code works to the following assumptions
        //
        // 1. Any skip command is preferable if source and base are same
        // 2. Fill is preferable to XOR if 4 or larger, XOR takes same data plus at
        //    least 1 byte
        //
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////
        //  Some defines used by the encoders
        ////////////////////////////////////////////////////////////////////////////////
        public const byte XOR_SMALL = 0x7F;
        public const byte XOR_MED = 0xFF;
        public const int XOR_LARGE = 0x3FFF;
        public const int XOR_MAX = 0x7FFF;

        ////////////////////////////////////////////////////////////////////////////////
        //  Some utility functions to get worst case sizes for buffer allocation
        ////////////////////////////////////////////////////////////////////////////////

        public static int LCWWorstCase(int datasize)
        {
            return datasize + (datasize / 63) + 1;
        }

        public static int XORWorstCase(int datasize)
        {
            return datasize + ((datasize / 63) * 3) + 4;
        }

        /// <summary>
        /// Compresses data to the proprietary LCW format used in many games developed by Westwood Studios.
        /// Compression is better than that achieved by popular community tools. This is a new implementation
        /// based on understanding of the compression gained from the reference code.
        /// Written by Omniblade.
        /// </summary>
        /// <param name="input">Array of the data to compress.</param>
        /// <returns>The compressed data.</returns>
        /// <remarks>Commonly known in the community as "format80".</remarks>
        public static byte[] LcwCompress(byte[] input)
        {
            if (input == null || input.Length == 0)
                return new byte[0];

            //Decide if we are going to do relative offsets for 3 and 5 byte commands
            bool relative = input.Length > ushort.MaxValue;

            // Nyer's C# conversion: replacements for write and read for pointers.
            int getp = 0;
            int putp = 0;
            // Input length. Used commonly enough to warrant getting it out in advance I guess.
            int getend = input.Length;
            // "Worst case length" code by OmniBlade. We'll just use a buffer of
            // that max length and cut it down to the actual used size at the end.
            // Not using it- it's not big enough in case of some small images.
            //LCWWorstCase(getend)
            int worstcase = Math.Max(10000, getend * 2);
            byte[] output = new byte[worstcase];
            // relative LCW starts with 0 as flag to decoder.
            // this is only used by later games for decoding hi-color vqa files.
            if (relative)
                output[putp++] = 0;

            //Implementations that properly conform to the WestWood encoder should
            //write a starting cmd1. It's important for using the offset copy commands
            //to do more efficient RLE in some cases than the cmd4.

            //we also set bool to flag that we have an on going cmd1.
            int cmd_onep = putp;
            output[putp++] = 0x81;
            output[putp++] = input[getp++];
            bool cmd_one = true;

            //Compress data until we reach end of input buffer.
            while (getp < getend)
            {
                //Is RLE encode (4bytes) worth evaluating?
                if (getend - getp > 64 && input[getp] == input[getp + 64])
                {
                    //RLE run length is encoded as a short so max is UINT16_MAX
                    int rlemax = (getend - getp) < ushort.MaxValue ? getend : getp + ushort.MaxValue;
                    int rlep = getp + 1;
                    while (rlep < rlemax && input[rlep] == input[getp])
                        rlep++;

                    ushort run_length = (ushort)(rlep - getp);

                    //If run length is long enough, write the command and start loop again
                    if (run_length >= 0x41)
                    {
                        //write 4byte command 0b11111110
                        cmd_one = false;
                        output[putp++] = 0xFE;
                        output[putp++] = (byte)(run_length & 0xFF);
                        output[putp++] = (byte)((run_length >> 8) & 0xFF);
                        output[putp++] = input[getp];
                        getp = rlep;
                        continue;
                    }
                }

                //current block size for an offset copy
                ushort block_size = 0;
                //Set where we start looking for matching runs.
                int offstart = relative ? getp < ushort.MaxValue ? 0 : getp - ushort.MaxValue : 0;

                //Look for matching runs
                int offchk = offstart;
                int offsetp = getp;
                while (offchk < getp)
                {
                    //Move offchk to next matching position
                    while (offchk < getp && input[offchk] != input[getp])
                        offchk++;

                    //If the checking pointer has reached current pos, break
                    if (offchk >= getp)
                        break;

                    //find out how long the run of matches goes for
                    int i;
                    for (i = 1; getp + i < getend; ++i)
                        if (input[offchk + i] != input[getp + i])
                            break;
                    if (i >= block_size)
                    {
                        block_size = (ushort)i;
                        offsetp = offchk;
                    }
                    offchk++;
                }

                //decide what encoding to use for current run
                //If it's less than 2 bytes long, we store as is with cmd1
                if (block_size <= 2)
                {
                    //short copy 0b10??????
                    //check we have an existing 1 byte command and if its value is still
                    //small enough to handle additional bytes
                    //start a new command if current one doesn't have space or we don't
                    //have one to continue
                    if (cmd_one && output[cmd_onep] < 0xBF)
                    {
                        //increment command value
                        output[cmd_onep]++;
                        output[putp++] = input[getp++];
                    }
                    else
                    {
                        cmd_onep = putp;
                        output[putp++] = 0x81;
                        output[putp++] = input[getp++];
                        cmd_one = true;
                    }
                    //Otherwise we need to decide what relative copy command is most efficient
                }
                else
                {
                    int offset;
                    int rel_offset = getp - offsetp;
                    if (block_size > 0xA || ((rel_offset) > 0xFFF))
                    {
                        //write 5 byte command 0b11111111
                        if (block_size > 0x40)
                        {
                            output[putp++] = 0xFF;
                            output[putp++] = (byte)(block_size & 0xFF);
                            output[putp++] = (byte)((block_size >> 8) & 0xFF);
                            //write 3 byte command 0b11??????
                        }
                        else
                        {
                            output[putp++] = (byte)((block_size - 3) | 0xC0);
                        }

                        offset = relative ? rel_offset : offsetp;
                        //write 2 byte command? 0b0???????
                    }
                    else
                    {
                        offset = rel_offset << 8 | (16 * (block_size - 3) + (rel_offset >> 8));
                    }
                    output[putp++] = (byte)(offset & 0xFF);
                    output[putp++] = (byte)((offset >> 8) & 0xFF);
                    getp += block_size;
                    cmd_one = false;
                }
            }

            //write final 0x80, basically an empty cmd1 to signal the end of the stream.
            output[putp++] = 0x80;

            byte[] finalOutput = new byte[putp];
            Array.Copy(output, 0, finalOutput, 0, putp);
            // Return the final compressed data.
            return finalOutput;
        }

        /// <summary>
        /// Decompresses data in the proprietary LCW format used in many games developed by Westwood Studios.
        /// Written by Omniblade.
        /// </summary>
        /// <param name="input">The data to decompress.</param>
        /// <param name="readOffset">Location to start at in the input array.</param>
        /// <param name="output">The buffer to store the decompressed data. This is assumed to be initialized to the correct size.</param>
        /// <param name="readEnd">End offset for reading. Use 0 to take the end of the given data array.</param>
        /// <returns>Length of the decompressed data in bytes.</returns>
        public static int LcwDecompress(byte[] input, ref int readOffset, byte[] output, int readEnd)
        {
            if (input == null || input.Length == 0 || output == null || output.Length == 0)
                return 0;
            bool relative = false;
            // Nyer's C# conversion: replacements for write and read for pointers.
            int writeOffset = 0;
            // Output length should be part of the information given in the file format using LCW.
            // Techncically it can just be cropped at the end, though this value is used to
            // automatically cut off repeat-commands that go too far.
            int writeEnd = output.Length;
            if (readEnd <= 0)
                readEnd = input.Length;

            //Decide if the stream uses relative 3 and 5 byte commands
            //Extension allows effective compression of data > 64k
            //https://github.com/madmoose/scummvm/blob/bladerunner/engines/bladerunner/decompress_lcw.cpp
            // this is only used by later games for decoding hi-color vqa files.
            // For other stuff (like shp), just check in advance to decide if the data is too big.
            if (readOffset >= readEnd)
                return writeOffset;
            if (input[readOffset] == 0)
            {
                relative = true;
                readOffset++;
            }
            //DEBUG_SAY("LCW Decompression... \n");
            while (writeOffset < writeEnd)
            {
                if (readOffset >= readEnd)
                    return writeOffset;
                byte flag = input[readOffset++];
                ushort cpysize;
                ushort offset;

                if ((flag & 0x80) != 0)
                {
                    if ((flag & 0x40) != 0)
                    {
                        cpysize = (ushort)((flag & 0x3F) + 3);
                        if (flag == 0xFE)
                        {
                            //long set 0b11111110
                            if (readOffset >= readEnd)
                                return writeOffset;
                            cpysize = input[readOffset++];
                            if (readOffset >= readEnd)
                                return writeOffset;
                            cpysize += (ushort)((input[readOffset++]) << 8);
                            if (cpysize > writeEnd - writeOffset)
                                cpysize = (ushort)(writeEnd - writeOffset);
                            if (readOffset >= readEnd)
                                return writeOffset;
                            //DEBUG_SAY("0b11111110 Source Pos %ld, Dest Pos %ld, Count %d\n", source - sstart - 3, dest - start, cpysize);
                            for (; cpysize > 0; --cpysize)
                            {
                                if (writeOffset >= writeEnd)
                                    return writeOffset;
                                output[writeOffset++] = input[readOffset];
                            }
                            readOffset++;
                        }
                        else
                        {
                            int s;
                            if (flag == 0xFF)
                            {
                                //long move, abs 0b11111111
                                if (readOffset >= readEnd)
                                    return writeOffset;
                                cpysize = input[readOffset++];
                                if (readOffset >= readEnd)
                                    return writeOffset;
                                cpysize += (ushort)((input[readOffset++]) << 8);
                                if (cpysize > writeEnd - writeOffset)
                                    cpysize = (ushort)(writeEnd - writeOffset);
                                if (readOffset >= readEnd)
                                    return writeOffset;
                                offset = input[readOffset++];
                                if (readOffset >= readEnd)
                                    return writeOffset;
                                offset += (ushort)((input[readOffset++]) << 8);
                                //extended format for VQA32
                                if (relative)
                                    s = writeOffset - offset;
                                else
                                    s = offset;
                                //DEBUG_SAY("0b11111111 Source Pos %ld, Dest Pos %ld, Count %d, Offset %d\n", source - sstart - 5, dest - start, cpysize, offset);
                                for (; cpysize > 0; --cpysize)
                                {
                                    if (writeOffset >= writeEnd)
                                        return writeOffset;
                                    output[writeOffset++] = output[s++];
                                }
                            }
                            else
                            {
                                //short move abs 0b11??????
                                if (cpysize > writeEnd - writeOffset)
                                    cpysize = (ushort)(writeEnd - writeOffset);
                                if (readOffset >= readEnd)
                                    return writeOffset;
                                offset = input[readOffset++];
                                if (readOffset >= readEnd)
                                    return writeOffset;
                                offset += (ushort)((input[readOffset++]) << 8);
                                //extended format for VQA32
                                if (relative)
                                    s = writeOffset - offset;
                                else
                                    s = offset;
                                //DEBUG_SAY("0b11?????? Source Pos %ld, Dest Pos %ld, Count %d, Offset %d\n", source - sstart - 3, dest - start, cpysize, offset);
                                for (; cpysize > 0; --cpysize)
                                {
                                    if (writeOffset >= writeEnd)
                                        return writeOffset;
                                    output[writeOffset++] = output[s++];
                                }
                            }
                        }
                    }
                    else
                    {
                        //short copy 0b10??????
                        if (flag == 0x80)
                        {
                            //DEBUG_SAY("0b10?????? Source Pos %ld, Dest Pos %ld, Count %d\n", source - sstart - 1, dest - start, 0);
                            return writeOffset;
                        }
                        cpysize = (ushort)(flag & 0x3F);
                        if (cpysize > writeEnd - writeOffset)
                            cpysize = (ushort)(writeEnd - writeOffset);
                        //DEBUG_SAY("0b10?????? Source Pos %ld, Dest Pos %ld, Count %d\n", source - sstart - 1, dest - start, cpysize);
                        for (; cpysize > 0; --cpysize)
                        {
                            if (readOffset >= readEnd || writeOffset >= writeEnd)
                                return writeOffset;
                            output[writeOffset++] = input[readOffset++];
                        }
                    }
                }
                else
                {
                    //short move rel 0b0???????
                    cpysize = (ushort)((flag >> 4) + 3);
                    if (cpysize > writeEnd - writeOffset)
                        cpysize = (ushort)(writeEnd - writeOffset);
                    if (readOffset >= readEnd)
                        return writeOffset;
                    offset = (ushort)(((flag & 0xF) << 8) + input[readOffset++]);
                    //DEBUG_SAY("0b0??????? Source Pos %ld, Dest Pos %ld, Count %d, Offset %d\n", source - sstart - 2, dest - start, cpysize, offset);
                    for (; cpysize > 0; --cpysize)
                    {
                        if (writeOffset >= writeEnd || writeOffset < offset)
                            return writeOffset;
                        output[writeOffset] = output[writeOffset - offset];
                        writeOffset++;
                    }
                }
            }
            // If buffer is full, make sure to skip end command!
            if (writeOffset == writeEnd && readOffset < input.Length && input[readOffset] == 0x80)
                readOffset++;
            return writeOffset;
        }

        /// <summary>
        /// Generates a binary delta between two buffers. Mainly used for image data.
        /// Written by Omniblade.
        /// </summary>
        /// <param name="source">Buffer containing data to generate the delta for.</param>
        /// <param name="base">Buffer containing data that is the base for the delta.</param>
        /// <returns>The generated delta as bytes array.</returns>
        /// <remarks>Commonly known in the community as "format40".</remarks>
        public static byte[] GenerateXorDelta(byte[] source, byte[] @base)
        {
            // Nyer's C# conversion: replacements for write and read for pointers.
            // -for our delta (output)
            int putp = 0;
            // -for the image we go to
            int getsp = 0;
            // -for the image we come from
            int getbp = 0;
            //Length to process
            int getsendp = Math.Min(source.Length, @base.Length);
            byte[] dest = new byte[XORWorstCase(getsendp)];

            //Only check getsp to save a redundant check.
            //Both source and base should be same size and both pointers should be
            //incremented at the same time.
            while (getsp < getsendp)
            {
                uint fillcount = 0;
                uint xorcount = 0;
                uint skipcount = 0;
                byte lastxor = (byte)(source[getsp] ^ @base[getbp]);
                int testsp = getsp;
                int testbp = getbp;

                //Only evaluate other options if we don't have a matched pair
                while (testsp < getsendp && source[testsp] != @base[testbp])
                {
                    if ((source[testsp] ^ @base[testbp]) == lastxor)
                    {
                        ++fillcount;
                        ++xorcount;
                    }
                    else
                    {
                        if (fillcount > 3)
                            break;
                        lastxor = (byte)(source[testsp] ^ @base[testbp]);
                        fillcount = 1;
                        ++xorcount;
                    }
                    testsp++;
                    testbp++;
                }

                //fillcount should always be lower than xorcount and should be greater
                //than 3 to warrant using the fill commands.
                fillcount = fillcount > 3 ? fillcount : 0;

                //Okay, lets see if we have any xor bytes we need to handle
                xorcount -= fillcount;
                while (xorcount != 0)
                {
                    ushort count;
                    //It's cheaper to do the small cmd twice than do the large cmd once
                    //for data that can be handled by two small cmds.
                    //cmd 0???????
                    if (xorcount < XOR_MED)
                    {
                        count = (ushort)(xorcount <= XOR_SMALL ? xorcount : XOR_SMALL);
                        dest[putp++] = (byte)count;
                        //cmd 10000000 10?????? ??????
                    }
                    else
                    {
                        count = (ushort)(xorcount <= XOR_LARGE ? xorcount : XOR_LARGE);
                        dest[putp++] = 0x80;
                        dest[putp++] = (byte)(count & 0xFF);
                        dest[putp++] = (byte)(((count >> 8) & 0xFF) | 0x80);
                    }

                    while (count != 0)
                    {
                        dest[putp++] = (byte)(source[getsp++] ^ @base[getbp++]);
                        count--;
                        xorcount--;
                    }
                }

                //lets handle the bytes that are best done as xorfill
                while (fillcount != 0)
                {
                    ushort count;
                    //cmd 00000000 ????????
                    if (fillcount <= XOR_MED)
                    {
                        count = (ushort)fillcount;
                        dest[putp++] = 0;
                        dest[putp++] = (byte)(count & 0xFF);
                        //cmd 10000000 11?????? ??????
                    }
                    else
                    {
                        count = (ushort)(fillcount <= XOR_LARGE ? fillcount : XOR_LARGE);
                        dest[putp++] = 0x80;
                        dest[putp++] = (byte)(count & 0xFF);
                        dest[putp++] = (byte)(((count >> 8) & 0xFF) | 0xC0);
                    }
                    dest[putp++] = (byte)(source[getsp] ^ @base[getbp]);
                    fillcount -= count;
                    getsp += count;
                    getbp += count;
                }

                //Handle regions that match exactly
                while (testsp < getsendp && source[testsp] == @base[testbp])
                {
                    skipcount++;
                    testsp++;
                    testbp++;
                }

                while (skipcount != 0)
                {
                    ushort count;
                    //Again it's cheaper to do the small cmd twice than do the large cmd
                    //once for data that can be handled by two small cmds.
                    //cmd 1???????
                    if (skipcount < XOR_MED)
                    {
                        count = (byte)(skipcount <= XOR_SMALL ? skipcount : XOR_SMALL);
                        dest[putp++] = (byte)(count | 0x80);
                        //cmd 10000000 0??????? ????????
                    }
                    else
                    {
                        count = (ushort)(skipcount <= XOR_MAX ? skipcount : XOR_MAX);
                        dest[putp++] = 0x80;
                        dest[putp++] = (byte)(count & 0xFF);
                        dest[putp++] = (byte)((count >> 8) & 0xFF);
                    }
                    skipcount -= count;
                    getsp += count;
                    getbp += count;
                }
            }

            //final skip command of 0 to signal end of stream.
            dest[putp++] = 0x80;
            dest[putp++] = 0;
            dest[putp++] = 0;

            byte[] finalOutput = new byte[putp];
            Array.Copy(dest, 0, finalOutput, 0, putp);
            // Return the final data
            return finalOutput;
        }

        /// <summary>
        /// Applies a binary delta to a buffer.
        /// Written by Omniblade.
        /// </summary>
        /// <param name="data">The data to apply the xor to.</param>
        /// <param name="xorSource">The the delta data to apply.</param>
        /// <param name="xorStart">Start offset in the data.</param>
        /// <param name="xorEnd">End offset in the data. Use 0 to take the end of the whole array.</param>
        public static void ApplyXorDelta(byte[] data, byte[] xorSource, ref int xorStart, int xorEnd)
        {
            // Nyer's C# conversion: replacements for write and read for pointers.
            int putp = 0;
            byte value = 0;
            int dataEnd = data.Length;
            if (xorEnd <= 0)
                xorEnd = xorSource.Length;
            while (putp < dataEnd && xorStart < xorEnd)
            {
                //DEBUG_SAY("XOR_Delta Put pos: %u, Get pos: %u.... ", putp - scast<sint8*>(dest), getp - scast<sint8*>(source));
                byte cmd = xorSource[xorStart++];
                ushort count = cmd;
                bool xorval = false;

                if ((cmd & 0x80) == 0)
                {
                    //0b00000000
                    if (cmd == 0)
                    {
                        if (xorStart >= xorEnd)
                            return;
                        count = (ushort)(xorSource[xorStart++] & 0xFF);
                        if (xorStart >= xorEnd)
                            return;
                        value = xorSource[xorStart++];
                        xorval = true;
                        //DEBUG_SAY("0b00000000 Val Count %d ", count);
                        //0b0???????
                    }
                }
                else
                {
                    //0b1??????? remove most significant bit
                    count &= 0x7F;
                    if (count != 0)
                    {
                        putp += count;
                        //DEBUG_SAY("0b1??????? Skip Count %d\n", count);
                        continue;
                    }
                    if (xorStart >= xorEnd)
                        return;
                    count = (ushort)(xorSource[xorStart++] & 0xFF);
                    if (xorStart >= xorEnd)
                        return;
                    count += (ushort)(xorSource[xorStart++] << 8);

                    //0b10000000 0 0
                    if (count == 0)
                    {
                        //DEBUG_SAY("0b10000000 Count %d to end delta\n", count);
                        return;
                    }

                    //0b100000000 0?
                    if ((count & 0x8000) == 0)
                    {
                        putp += count;
                        //DEBUG_SAY("0b100000000 0? Skip Count %d\n", count);
                        continue;
                    }
                    //0b10000000 11
                    if ((count & 0x4000) != 0)
                    {
                        count &= 0x3FFF;
                        if (xorStart >= xorEnd)
                            return;
                        value = xorSource[xorStart++];
                        //DEBUG_SAY("0b10000000 11 Val Count %d ", count);
                        xorval = true;
                        //0b10000000 10
                    }
                    else
                    {
                        count &= 0x3FFF;
                        //DEBUG_SAY("0b10000000 10 XOR Count %d ", count);
                    }
                }

                if (xorval)
                {
                    //DEBUG_SAY("XOR Val %d\n", value);
                    for (; count > 0; --count)
                    {
                        if (putp >= dataEnd)
                            return;
                        data[putp++] ^= value;
                    }
                }
                else
                {
                    //DEBUG_SAY("XOR Source to Dest\n");
                    for (; count > 0; --count)
                    {
                        if (putp >= dataEnd || xorStart >= xorEnd)
                            return;
                        data[putp++] ^= xorSource[xorStart++];
                    }
                }
            }
        }

        /// <summary>
        /// Decompresses the zero-collapsing RLE used in Tiberian Sun.
        /// Written by Maarten 'Nyerguds' Meuris
        /// </summary>
        /// <param name="fileData">File data</param>
        /// <param name="offset">Offset in the file data to start reading.</param>
        /// <param name="frameWidth">Image height</param>
        /// <param name="frameHeight">Image width</param>
        /// <returns>The decompressed data.</returns>
        /// <exception cref="ArgumentException">The decompression failed because the data did not match the expected format.</exception>
        public static byte[] RleZeroTsDecompress(byte[] fileData, ref int offset, int frameWidth, int frameHeight)
        {
            byte[] finalImage = new byte[frameWidth * frameHeight];
            int datalen = fileData.Length;
            int outLineOffset = 0;
            for (int y = 0; y < frameHeight; ++y)
            {
                int outOffset = outLineOffset;
                int nextLineOffset = outLineOffset + frameWidth;
                if (offset + 2 >= datalen)
                    throw new ArgumentException("Not enough lines in RLE-Zero data!", "fileData");
                // Compose little-endian UInt16 from 2 bytes
                int lineLen = fileData[offset] | (fileData[offset + 1] << 8);
                int end = offset + lineLen;
                if (lineLen < 2 || end > datalen)
                    throw new ArgumentException("Bad value in RLE-Zero line header!", "fileData");
                // Skip header
                offset += 2;
                bool readZero = false;
                for (; offset < end; ++offset)
                {
                    if (outOffset >= nextLineOffset)
                        throw new ArgumentException("Bad line alignment in RLE-Zero data!", "fileData");
                    if (readZero)
                    {
                        // Zero has been read. Process 0-repeat.
                        readZero = false;
                        int zeroes = fileData[offset];
                        for (; zeroes > 0 && outOffset < nextLineOffset; zeroes--)
                            finalImage[outOffset++] = 0;
                    }
                    else if (fileData[offset] == 0)
                    {
                        // Rather than manually increasing the offset, just flag that
                        // "a 0 value has been read" so the next loop can read the repeat value.
                        readZero = true;
                    }
                    else
                    {
                        // Simply copy a value.
                        finalImage[outOffset++] = fileData[offset];
                    }
                }
                // If a data line ended on a 0, there's something wrong.
                if (readZero)
                    throw new ArgumentException("Incomplete zero-repeat command!", "fileData");
                outLineOffset = nextLineOffset;
            }
            return finalImage;
        }

        /// <summary>
        /// Compresses data with the zero-collapsing RLE used in Tiberian Sun.
        /// Written by Maarten 'Nyerguds' Meuris
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="frameWidth">Image width</param>
        /// <param name="frameHeight">Image height</param>
        /// <returns>The compressed data.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] RleZeroTsCompress(byte[] imageData, int frameWidth, int frameHeight)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int inputLineOffset = 0;
                for (int y = 0; y < frameHeight; ++y)
                {
                    long lineStartOffs = ms.Position;
                    ms.Position = lineStartOffs + 2;
                    int inputOffset = inputLineOffset;
                    int nextLineOffset = inputOffset + frameWidth;
                    while (inputOffset < nextLineOffset)
                    {
                        byte b = imageData[inputOffset];
                        if (b == 0)
                        {
                            int startOffs = inputOffset;
                            int max = Math.Min(startOffs + 256, nextLineOffset);
                            for (; inputOffset < max && imageData[inputOffset] == 0; ++inputOffset) { }
                            ms.WriteByte(0);
                            int skip = inputOffset - startOffs;
                            ms.WriteByte((byte)(skip));
                        }
                        else
                        {
                            ms.WriteByte(b);
                            inputOffset++;
                        }
                    }
                    // Go back to start of the line data and fill in the length.
                    long lineEndOffs = ms.Position;
                    long len = lineEndOffs - lineStartOffs;
                    if (len > ushort.MaxValue)
                        throw new ArgumentException("Compressed line width is too large to store!", "imageData");
                    ms.Position = lineStartOffs;
                    ms.WriteByte((byte)(len & 0xFF));
                    ms.WriteByte((byte)((len >> 8) & 0xFF));
                    ms.Position = lineEndOffs;
                    inputLineOffset = nextLineOffset;
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the zero-collapsing RLE used in Dune II.
        /// Written by Maarten 'Nyerguds' Meuris
        /// </summary>
        /// <param name="fileData">File data</param>
        /// <param name="offset">Offset in the file data to start reading.</param>
        /// <param name="frameWidth">Image height</param>
        /// <param name="frameHeight">Image width</param>
        /// <returns>The decompressed data.</returns>
        public static byte[] RleZeroD2Decompress(byte[] fileData, ref int offset, int frameWidth, int frameHeight)
        {
            int fullLength = frameWidth * frameHeight;
            byte[] finalImage = new byte[fullLength];
            int datalen = fileData.Length;
            int outLineOffset = 0;
            for (int y = 0; y < frameHeight; ++y)
            {
                int outOffset = outLineOffset;
                int nextLineOffset = outLineOffset + frameWidth;
                bool readZero = false;
                for (; offset < datalen; ++offset)
                {
                    if (outOffset >= nextLineOffset)
                        break;
                    if (readZero)
                    {
                        readZero = false;
                        int zeroes = fileData[offset];
                        for (; zeroes > 0 && outOffset < nextLineOffset; zeroes--)
                            finalImage[outOffset++] = 0;
                    }
                    else if (fileData[offset] == 0)
                    {
                        readZero = true;
                    }
                    else
                    {
                        finalImage[outOffset++] = fileData[offset];
                    }
                }
                outLineOffset = nextLineOffset;
            }
            return finalImage;
        }


        /// <summary>
        /// Compresses data with the zero-collapsing RLE used in Tiberian Sun.
        /// Written by Maarten 'Nyerguds' Meuris.
        /// </summary>
        /// <param name="imageData">Image data</param>
        /// <param name="frameWidth">Image width</param>
        /// <param name="frameHeight">Image height</param>
        /// <returns>The compressed data.</returns>
        public static byte[] RleZeroD2Compress(byte[] imageData, int frameWidth, int frameHeight)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int inputLineOffset = 0;
                for (int y = 0; y < frameHeight; ++y)
                {
                    int inputOffset = inputLineOffset;
                    int nextLineOffset = inputOffset + frameWidth;
                    while (inputOffset < nextLineOffset)
                    {
                        byte b = imageData[inputOffset];
                        if (b == 0)
                        {
                            int startOffs = inputOffset;
                            int max = Math.Min(startOffs + 256, nextLineOffset);
                            for (; inputOffset < max && imageData[inputOffset] == 0; ++inputOffset) { }
                            ms.WriteByte(0);
                            int skip = inputOffset - startOffs;
                            ms.WriteByte((byte)(skip));
                        }
                        else
                        {
                            ms.WriteByte(b);
                            inputOffset++;
                        }
                    }
                    inputLineOffset = nextLineOffset;
                }
                return ms.ToArray();
            }
        }
    }
}