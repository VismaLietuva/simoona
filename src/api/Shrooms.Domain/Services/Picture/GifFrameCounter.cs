using System.IO;

namespace Shrooms.Domain.Services.Picture
{
    /// <summary>
    /// Counts the image descriptors in a GIF by walking its block structure, without decoding any pixel
    /// data. A GIF with thousands of frames is tiny on disk yet decodes to frames × canvas in memory, which
    /// the dimension check (one frame) cannot see.
    /// </summary>
    public static class GifFrameCounter
    {
        /// <summary>Number of frames, stopping early once <paramref name="maxFrames"/> is exceeded. -1 when the data is not a GIF.</summary>
        public static int Count(Stream stream, int maxFrames)
        {
            var start = stream.Position;
            try
            {
                var header = new byte[13];
                if (stream.Read(header, 0, header.Length) != header.Length
                    || header[0] != (byte)'G' || header[1] != (byte)'I' || header[2] != (byte)'F')
                {
                    return -1;
                }

                // Logical screen descriptor: a global colour table follows when bit 7 of the packed byte is set.
                var packed = header[10];
                if ((packed & 0x80) != 0)
                {
                    Skip(stream, 3 * (1 << ((packed & 0x07) + 1)));
                }

                var frames = 0;
                while (true)
                {
                    var block = stream.ReadByte();
                    if (block < 0 || block == 0x3B)
                    {
                        break;
                    }

                    if (block == 0x21)
                    {
                        // Extension: label byte, then data sub-blocks.
                        stream.ReadByte();
                        SkipSubBlocks(stream);
                    }
                    else if (block == 0x2C)
                    {
                        frames++;
                        if (frames > maxFrames)
                        {
                            return frames;
                        }

                        // Image descriptor: left, top, width, height (8 bytes) then a packed byte.
                        Skip(stream, 8);
                        var imagePacked = stream.ReadByte();
                        if (imagePacked < 0)
                        {
                            break;
                        }

                        if ((imagePacked & 0x80) != 0)
                        {
                            Skip(stream, 3 * (1 << ((imagePacked & 0x07) + 1)));
                        }

                        stream.ReadByte(); // LZW minimum code size
                        SkipSubBlocks(stream);
                    }
                    else
                    {
                        break; // Unknown block: stop counting, the decoder will reject the file if it is broken.
                    }
                }

                return frames;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = start;
                }
            }
        }

        private static void SkipSubBlocks(Stream stream)
        {
            while (true)
            {
                var length = stream.ReadByte();
                if (length <= 0)
                {
                    return;
                }

                Skip(stream, length);
            }
        }

        private static void Skip(Stream stream, int count)
        {
            if (stream.CanSeek)
            {
                stream.Seek(count, SeekOrigin.Current);
                return;
            }

            var buffer = new byte[256];
            while (count > 0)
            {
                var read = stream.Read(buffer, 0, count < buffer.Length ? count : buffer.Length);
                if (read <= 0)
                {
                    return;
                }

                count -= read;
            }
        }
    }
}
