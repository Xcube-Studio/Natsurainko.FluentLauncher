using System;
using System.IO;

namespace Natsurainko.Toolkits.IO
{
    public static class StreamExtension
    {
        public static int ReadInt(this Stream stream) => stream.ReadByte();

        public static void WriteInt(this Stream stream, int value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public static long ReadLong(this Stream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToInt64(buffer, 0);
        }

        public static void WriteLong(this Stream stream, long value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public static long ReadShort(this Stream stream)
        {
            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToInt16(buffer, 0);
        }

        public static void WriteShort(this Stream stream, short value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
    }
}
