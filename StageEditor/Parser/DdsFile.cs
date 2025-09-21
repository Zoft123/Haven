using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;

namespace Haven.Parser
{
    public static class DdsFile
    {
        public static void Create(string path, uint height, uint width, string fourCC, int mipMapCount, byte[] data)
        {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new BinaryWriterEx(stream, false))
                {
                    stream.SetLength(0);

                    writer.Write(0x20534444);
                    writer.Write(0x7C);
                    writer.Write(0x00021007);
                    writer.Write(height);
                    writer.Write(width);
                    writer.Write(0); // pitch
                    writer.Write(0); // depth
                    writer.Write(mipMapCount);
                    for (int i = 0; i < 11; i++)
                    {
                        writer.Write(0);
                    }
                    writer.Write(0x20); // pixel format size
                    writer.Write(0x05); // flags
                    writer.Write(Encoding.UTF8.GetBytes(fourCC));
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0x00401008);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(data);
                }
            }
        }

        public static byte[] BuildHeader(int width, int height, string fourcc, int linearSize)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(Encoding.ASCII.GetBytes("DDS "));

            bw.Write(124);
            const int DDSD_CAPS = 0x1, DDSD_HEIGHT = 0x2, DDSD_WIDTH = 0x4, DDSD_PIXELFORMAT = 0x1000, DDSD_LINEARSIZE = 0x80000;
            bw.Write(DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT | DDSD_LINEARSIZE);
            bw.Write(height);
            bw.Write(width);
            bw.Write(linearSize);
            bw.Write(0);
            bw.Write(1);

            for (int i = 0; i < 11; i++) bw.Write(0);

            bw.Write(32);
            bw.Write(0x4);
            bw.Write(Encoding.ASCII.GetBytes(fourcc.PadRight(4, '\0')));
            bw.Write(0);
            bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);

            bw.Write(0x1000);
            bw.Write(0); bw.Write(0); bw.Write(0);

            bw.Write(0);

            return ms.ToArray();
        }

    }
}
