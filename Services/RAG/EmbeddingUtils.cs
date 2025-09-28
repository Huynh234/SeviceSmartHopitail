using System;

namespace SeviceSmartHopitail.Services.RAG
{
    public static class EmbeddingUtils
    {
        public static byte[] FloatArrayToBytes(float[] floats)
        {
            var bytes = new byte[floats.Length * 4];
            Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static float[] BytesToFloatArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return Array.Empty<float>();
            var floats = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            return floats;
        }
    }
}
