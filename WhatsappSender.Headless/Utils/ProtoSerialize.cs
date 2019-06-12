using System.IO;
using System.IO.Compression;

namespace WhatsappSender.Headless.Utils
{
    public static class ProtoSerialize
    {
        public static void Serialize<TObject>(TObject objext, Stream destination)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, objext);
                ms.Position = 0;
                using (var gZipStream = new GZipStream(destination, CompressionMode.Compress, true))
                {
                    ms.CopyTo(gZipStream);
                    gZipStream.Close();
                }
            }
        }

        public static byte[] Serialize<TObject>(TObject obj)
        {
            byte[] bytes, compressedBytes;
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, obj);
                bytes = ms.ToArray();
                ms.Close();
                using (var compressedMs = new MemoryStream())
                using (var gZipStream = new GZipStream(compressedMs, CompressionMode.Compress))
                {
                    gZipStream.Write(bytes, 0, bytes.Length);
                    gZipStream.Close();
                    compressedBytes = compressedMs.ToArray();
                    compressedMs.Close();
                }
            }
            return compressedBytes;
        }

        public static TObject Deserialize<TObject>(Stream source)
        {
            using (var gZipStream = new GZipStream(source, CompressionMode.Decompress))
            using (var ms = new MemoryStream())
            {
                gZipStream.CopyTo(ms);
                var bytes_arr = ms.ToArray();
                ms.Position = 0;
                ms.SetLength(bytes_arr.LongLength);
                return ProtoBuf.Serializer.Deserialize<TObject>(ms);
            }
        }

        public static TObject Deserialize<TObject>(byte[] source)
        {
            using (var stream = new MemoryStream(source))
            {
                stream.Position = 0;
                return Deserialize<TObject>(stream);
            }
        }
    }
}
