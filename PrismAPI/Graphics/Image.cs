using PrismAPI.Tools.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace PrismAPI.Graphics;

public unsafe static class Image
{
    #region Structure

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TGAHeader
    {
        public char Magic1;                         // must be zero
        public char ColorMap;                       // must be zero
        public char Encoding;                       // must be 2
        public short CMaporig, CMaplen, CMapent;    // must be zero
        public short X;                             // must be zero
        public short Y;                             // image's height
        public short Height;                        // image's height
        public short Width;                         // image's width
        public char ColorDepth;                     // must be 32
        public char PixelType;                      // must be 40
    }

    #endregion

    #region Methods

    /// <summary>
    /// Loads a bitmap file.
    /// Based on: https://github.com/CosmosOS/Cosmos/blob/master/source/Cosmos.System2/Graphics/Bitmap.cs
    /// </summary>
    /// <param name="Binary">Raw file data.</param>
    /// <returns>BMP file as a <see cref="Canvas"/> instance.</returns>
    public static Canvas FromBitmap(byte[] Binary, bool UseBGR = true)
    {
        // Create reader instance.
        BinaryReader Reader = new(new MemoryStream(Binary));

        // Create temporary buffer.
        Canvas Temp = new(0, 0);

        // Reading magic number to identify if BMP file. (BM as string - 42 4D as Hex)
        if (BitConverter.ToString(Reader.ReadBytes(2)) != "42-4D") // do uint if not work?
        {
            throw new FormatException("The input file is not a bitmap file!");
        }

        // Read the size of BMP file. - byte 2 -> 6
        uint FileSize = Reader.ReadUInt32();

        // Read the header - bytes 10 -> 14 is the offset of the bitmap image data.
        Reader.BaseStream.Position = 10;
        uint PixelTableOffset = Reader.ReadUInt32();

        // Now reading size of BITMAPINFOHEADER should be 40. - bytes 14 -> 18
        uint InfoHeaderSize = Reader.ReadUInt32();

        // 124 - is BITMAPV5INFOHEADER, 56 - is BITMAPV3INFOHEADER, where we ignore the additional values see https://web.archive.org/web/20150127132443/https://forums.adobe.com/message/3272950
        if (InfoHeaderSize != 40 && InfoHeaderSize != 56 && InfoHeaderSize != 124)
        {
            throw new FormatException("Info header size has the wrong value!");
        }

        // Now reading width of image in pixels. - bytes 18 -> 22
        Temp.Width = (ushort)Reader.ReadUInt32();

        // Now reading height of image in pixels. - byte 22 -> 26
        Temp.Height = (ushort)Reader.ReadUInt32();

        // Now reading number of planes - should be 1. - byte 26 -> 28
        ushort Planes = Reader.ReadUInt16();

        // Check that the image only has one plane.
        if (Planes != 1)
        {
            throw new FormatException("Number of planes is not 1! Can not read file!");
        }

        // Now reading size of bits per pixel (1, 4, 8, 24, 32). - bytes 28 - 30
        ushort PixelSize = Reader.ReadUInt16();

        // TODO: Be able to handle other pixel sizes.
        if (!(PixelSize == 32 || PixelSize == 24))
        {
            throw new NotImplementedException("Can only handle 32-bit or 24-bit bitmap files!");
        }

        // Now reading compression type. - bytes 30 -> 34
        uint Compression = Reader.ReadUInt32();

        // TODO: Be able to handle compressed files.
        // 3 is BI_BITFIELDS again ignore for now is for Adobe Images.
        if (Compression != 0 && Compression != 3)
        {
            throw new NotImplementedException("Can only handle uncompressed bitmap files!");
        }

        // Now reading total image data size(including padding). - bytes 34 -> 38\
        uint TotalImageSize = Reader.ReadUInt32();

        // Adjust to corrected image size.
        // Look at the link above for the explanation.
        if (TotalImageSize == 0)
        {
            TotalImageSize = (uint)(((Temp.Width * PixelSize) + 31 & ~31) >> 3) * Temp.Height;
        }

        // Calculate the padding.
        int PureImageSize = Temp.Width * Temp.Height * PixelSize / 8;
        int PaddingPerRow = 0;

        if (TotalImageSize != 0)
        {
            int Remainder = (int)TotalImageSize - PureImageSize;

            if (Remainder < 0)
            {
                throw new Exception("Total Image Size is smaller than pure image size!");
            }

            PaddingPerRow = Remainder / Temp.Height;
            PureImageSize = (int)TotalImageSize;
        }

        // Read the pixel data.
        Reader.BaseStream.Position = PixelTableOffset;
        byte[] Pixel = new byte[4]; //All must have the same size

        // Loop over each pixel.
        for (int Y = 0; Y < Temp.Height; Y++)
        {
            for (int X = 0; X < Temp.Width; X++)
            {
                switch (PixelSize)
                {
                    case 32:
                        Pixel[0] = Reader.ReadByte();
                        Pixel[1] = Reader.ReadByte();
                        Pixel[2] = Reader.ReadByte();
                        Pixel[3] = Reader.ReadByte();
                        break;
                    case 24:
                        if (UseBGR)
                        {
                            Pixel[3] = Reader.ReadByte();
                            Pixel[2] = Reader.ReadByte();
                            Pixel[1] = Reader.ReadByte();
                            Pixel[0] = 255;
                        }
                        else
                        {
                            Pixel[0] = Reader.ReadByte();
                            Pixel[1] = Reader.ReadByte();
                            Pixel[2] = Reader.ReadByte();
                            Pixel[3] = 255;
                        }
                        break;
                }

                // Set the pixel value. The bits should be A, R, G, B but the order is switched.
                Temp.Internal[X + ((Temp.Height - (Y + 1)) * Temp.Width)] = BitConverter.ToUInt32(Pixel, 0);
            }

            // Increment the padding.
            Reader.BaseStream.Position += PaddingPerRow;
        }

        // Return final image result.
        return Temp;
    }

    /// <summary>
    /// Loads a PNG file.
    /// </summary>
    /// <param name="Binary">Raw file data.</param>
    /// <returns>PNG file as a <see cref="Canvas"/> instance.</returns>
    public static Canvas FromPNG(byte[] Binary)
    {
        // Check header for invalid magic data.
        if (Binary[0] != 137 ||
            Binary[1] != 80 ||
            Binary[2] != 78 ||
            Binary[3] != 71 ||
            Binary[4] != 13 ||
            Binary[5] != 10 ||
            Binary[6] != 26 ||
            Binary[7] != 10)
        {
            throw new("Invalid header magic!");
        }

        BinaryReader Reader = new(new MemoryStream(Binary));
        Reader.BaseStream.Position = 8;
        Canvas Result = new(0, 0);

        while (Reader.BaseStream.Position < Reader.BaseStream.Length)
        {
            uint Length = Reader.ReadUInt32();
            long Position = Reader.BaseStream.Position;
            bool IsDone = false;

            switch (Encoding.ASCII.GetString(Reader.ReadBytes(4)))
            {
                case "IHDR":
                    Result.Width = (ushort)Reader.ReadUInt32();
                    Result.Height = (ushort)Reader.ReadUInt32();
                    Reader.BaseStream.Position += 5;
                    break;
                case "PLTE":
                    break;
                case "IDAT":
                    List<byte> Buffer = new();
                    Reader.BaseStream.Position += 2;
                    for (int i = 2; i < Length; i++)
                    {
                        Buffer.Add(Reader.ReadByte());
                    }

                    List<byte> Data = DeflateStream.Inflate(Buffer);

                    BinaryReader D = new(new MemoryStream(Data.ToArray()));

                    var totalScanlines = Data.Count / (Result.Width + 1) / 4;

                    var prevScanline = new List<byte>();

                    for (int y = 0; y < totalScanlines; y++)
                    {
                        var filter = D.ReadByte();

                        var dat = new List<byte>();

                        for (int x = 0; x < Result.Width * 4; x++)
                        {
                            dat.Add(D.ReadByte());
                        }
                        var scanline = new List<byte>();

                        if (filter == 1)
                        {
                            scanline.Add(dat[0]);
                            for (var index = 1; index < dat.Count; index++)
                            {
                                scanline.Add((byte)((scanline[index - 4 > 0 ? index - 4 : 0] + dat[index - 1]) % 256));
                                //scanline.Add((byte) (255));
                            }
                        }
                        else if (filter == 2)
                        {
                            for (var index = 0; index < dat.Count; index++)
                            {
                                scanline.Add((byte)((prevScanline[index] + dat[index]) % 256));
                                //scanline.Add((byte) (255));
                            }
                        }
                        else
                        {
                        }

                        var line = new BinaryReader(new MemoryStream(scanline.ToArray()));
                        prevScanline.Clear();
                        prevScanline.AddRange(scanline);

                        for (int x = 0; x < Result.Width; x++)
                        {
                            // Read ARGB color
                            Result[x, y] = line.ReadUInt32();
                        }
                    }

                    break;
                case "IEND":
                    IsDone = true;
                    break;
            }

            if (IsDone)
            {
                break;
            }

            Reader.BaseStream.Position = (int)(Position + Length) + 4;
        }

        return Result;
    }

    /// <summary>
    /// Loads a TGA file.
    /// </summary>
    /// <param name="Binary">Raw file data.</param>
    /// <returns>TGA file as a <see cref="Canvas"/> instance.</returns>
    public static Canvas FromTGA(byte[] Binary)
    {
        Canvas Result = new(0, 0);
        TGAHeader* Header;

        fixed (byte* P = Binary)
        {
            Header = (TGAHeader*)P;
        }

        Result.Height = (ushort)Header->Height;
        Result.Width = (ushort)Header->Width;

        switch (Header->ColorDepth)
        {
            case (char)32:
                for (uint I = 0; I < Result.Width * Result.Height * 4; I++)
                {
                    Result[I] = Color32.FromARGB(Binary[I + 22], Binary[I + 21], Binary[I + 20], Binary[I + 19]);
                }
                break;
            case (char)24:
                for (uint I = 0; I < Result.Width * Result.Height * 3; I++)
                {
                    Result[I] = Color32.FromARGB(255, Binary[I + 21], Binary[I + 20], Binary[I + 19]);
                }
                break;
        }

        return Result;
    }

    /// <summary>
    /// Loads a PPM file.
    /// </summary>
    /// <param name="Binary">Raw file data.</param>
    /// <returns>PPM file as a <see cref="Canvas"/> instance.</returns>
    public static Canvas FromPPM(byte[] Binary)
    {
        BinaryReader Reader = new(new MemoryStream(Binary));

        if (Reader.ReadChar() != 'P' || Reader.ReadChar() != '6')
        {
            throw new("Not a PPM image!");
        }

        Reader.ReadChar(); // Skip Newline
        string widths = "", heights = "";

        for (char TMP = '\0'; TMP != ' '; TMP = Reader.ReadChar())
        {
            if (TMP == '#')
            {
                while (Reader.ReadChar() != '\n') ;
            }
            else
            {
                widths += TMP;
            }
        }
        for (char TMP = '\0'; TMP != '0' && TMP != '9'; TMP = Reader.ReadChar())
        {
            heights += TMP;
        }

        if (Reader.ReadChar() != '2' || Reader.ReadChar() != '5' || Reader.ReadChar() != '5')
        {
            throw new("Improper file data!");
        }

        Reader.ReadChar(); // Skip Newline

        Canvas Result = new((ushort)uint.Parse(widths), (ushort)uint.Parse(heights));

        for (int Y = 0; Y < Result.Height; Y++)
        {
            for (int X = 0; X < Result.Width; X++)
            {
                Result[X, Y] = Color32.FromARGB(255, Reader.ReadByte(), Reader.ReadByte(), Reader.ReadByte());
            }
        }

        return Result;
    }

    #endregion
}