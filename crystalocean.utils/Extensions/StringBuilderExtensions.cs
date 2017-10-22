using System.Text;

namespace CrystalOcean.Utils.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendHex(this StringBuilder sb, byte[] bytes) 
        {
            foreach (byte b in bytes)
                sb.AppendFormat("{0:x2}", b);
            return sb;
        }
    }
}