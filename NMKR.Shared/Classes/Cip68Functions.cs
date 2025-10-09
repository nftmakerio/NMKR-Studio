using System.Text;
using System;

public static class Cip68Functions
{
    /// <summary>
    /// Assumes this string being in hex notation and converts it to a normal string
    /// See: https://stackoverflow.com/questions/724862/converting-from-hex-to-string
    /// it also checks for cip67 and removes any prefixes like that
    /// </summary>
    /// <param name="s">String</param>
    /// <returns>This (hex string) as normal string, or null if this string is null</returns>
    public static string FromHexToNormal(this string s)
    {
        if (s == null)
        {
            return null;
        }

        s = s.Replace("-", "");

        byte[] raw = new byte[s.Length / 2];
        if (raw.Length >= 4)
        {
            //[ 0000 | 16 bits label_num | 8 bits checksum | 0000 ]
            //0x000de140 example label prefix
            if (s.Substring(0, 1) == "0" && s.Substring(7, 1) == "0")
            {
                byte[] result = new byte[raw.Length - 4];
                var b1 = Convert.ToByte(s.Substring(1, 2), 16);
                var b2 = Convert.ToByte(s.Substring(3, 2), 16);
                var checksum = Convert.ToByte(s.Substring(5, 2), 16);
                if (Crc8.ComputeChecksum(new byte[] {b1, b2}) == checksum)
                {
                    for (int i = 4; i < raw.Length; i++)
                    {
                        result[i - 4] = Convert.ToByte(s.Substring(i * 2, 2), 16);
                    }

                    var v = Encoding.UTF8.GetString(result);
                    return v;
                }
            }
        }


        for (int i = 0; i < raw.Length; i++)
        {
            raw[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
        }

        return Encoding.UTF8.GetString(raw);
    }
}