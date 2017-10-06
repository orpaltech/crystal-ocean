using System;
using System.IO;
using System.Security.Cryptography;

namespace CrystalOcean.Utils
{
    public class Checksum
    {
        public static String GetChecksum(String fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public static String GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
