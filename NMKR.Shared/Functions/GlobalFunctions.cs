using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NMKR.Shared.Classes;
using NMKR.Shared.DbSyncModel;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using NMKR.SimpleExec;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Transaction = NMKR.Shared.Model.Transaction;

namespace NMKR.Shared.Functions
{
    public static class GlobalFunctions
    {

        public static DbContextOptionsBuilder<EasynftprojectsContext> optionsBuilder = new();
        public static DbContextOptionsBuilder<DbSyncContext> optionsBuilderDbSync = new();

      public static int ServerId=0;
    
        public static string GetEnumDescription(System.Enum input)
        {
            Type type = input.GetType();
            MemberInfo[] memInfo = type.GetMember(input.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = (object[])memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return input.ToString();
        }
        public static string? NmkrToIsoDateString(this DateTime? self)
        {
            if (self is null)
            {
                return null;
            }

            return $"{self.Value.Year:D4}-{self.Value.Month:D2}-{self.Value.Day:D2}";
        }
        public static string FirstCharToCharArray(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var stringArray = input.ToCharArray();

            if (char.IsLower(stringArray[0]))
            {
                stringArray[0] = char.ToUpper(stringArray[0]);
            }

            return new string(stringArray);
        }
        public static string CharHelper(string minCharactersMaxCharacters, string  projectnameLength)
        {
            projectnameLength ??= "";
            if (minCharactersMaxCharacters == null)
                return "";
            return minCharactersMaxCharacters + " (" + projectnameLength.Length + ")";
        }
        public static Mimetype GetMimeType(EasynftprojectsContext db, string extension, bool mainfile)
        {
            var mt = (from a in db.Mimetypes
                where a.Fileextensions.Contains(extension) && (!mainfile || a.Allowedasmain)
                select a).AsNoTracking().FirstOrDefault();

            if (mt != null)
                return mt;

            return null;
        }

        public static string GetExtension(EasynftprojectsContext db,string aMimetype)
        {
             var _mimetypes = (from a in db.Mimetypes
                 select a).AsNoTracking().ToArray();

            var m = _mimetypes.FirstOrDefault(x => x.Mimetype1 == aMimetype);
            if (m == null)
                return "";

            return m.Fileextensions.Split(',').First();
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }


        public static string[] SplitStringIntoChunks(string str)
        {
            var words = str.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();
            var sb = new StringBuilder();

            foreach (var word in words)
            {
                if ((sb.Length + word.Length) > 63)
                {
                    result.Add(sb.ToString().TrimEnd());
                    sb.Clear();
                }

                sb.Append(word);
                sb.Append(' ');
            }

            if (sb.Length > 0)
            {
                result.Add(sb.ToString().TrimEnd());
            }

            return result.ToArray();
        }

        /*     public static string[] SplitStringIntoChunks(string str)
             {
                 var lines = str.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                 var result = new List<string>();

                 foreach (var line in lines)
                 {
                     int start = 0;
                     while (start < line.Length)
                     {
                         int length = Math.Min(63, line.Length - start);
                         result.Add(line.Substring(start, length));
                         start += length;
                     }
                 }

                 return result.ToArray();
             }*/
     
    
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static bool IsUrlValid(string url)
        {
            string pattern =
                @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }

        public static T[] RemoveFromArray<T>(this T[] original, T itemToRemove)
        {
            int numIdx = Array.IndexOf(original, itemToRemove);
            if (numIdx == -1) return original;
            List<T> tmp = new(original);
            tmp.RemoveAt(numIdx);
            return tmp.ToArray();
        }

        public static double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString())
                            .Replace(",", systemSeparator.ToString()));
            }
            catch
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        throw new("Wrong string-to-double format");
                    }
                }
            }

            return result;
        }

        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
                list.Swap(0, rnd.Next(0, i));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

     
        public static string PadInt(int N, int P)
        {
            // string used in Format() method
            string s = "{0:";
            for (int i = 0; i < P; i++)
            {
                s += "0";
            }

            s += "}";

            // use of string.Format() method
            string value = string.Format(s, N);

            // return output
            return value;
        }

        public static bool IsValidUrl(this string URL)
        {
            string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            Regex Rgx = new(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(URL);
        }

        public static bool IsValidEmail(this string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }

        public static string GetHMAC(string text, string key)
        {
            key = key ?? "";

            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToBase64String(hash);
        }

        public static async Task<int> GetWebsiteSettingsIntAsync(EasynftprojectsContext db, string key, int defaultvalue)
        {
            var websitesettings = await (from a in db.Websitesettings
                where a.Key == key
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (websitesettings == null) return defaultvalue;
            try
            {
                return Convert.ToInt32(websitesettings.Stringvalue);
            }
            catch
            {
                return defaultvalue;
            }
        }
        public static async Task<string> GetWebsiteSettingsStringAsync(EasynftprojectsContext db, string key)
        {
            var websitesettings = await (from a in db.Websitesettings
                where a.Key == key
                select a).AsNoTracking().FirstOrDefaultAsync();
            return websitesettings != null ? websitesettings.Stringvalue : null;
        }
        public static string GetWebsiteSettingsString(EasynftprojectsContext db, string key)
        {
            var res = Task.Run(async () => await GetWebsiteSettingsStringAsync(db, key));
            return res.Result;
        }
        public static bool GetWebsiteSettingsBool(EasynftprojectsContext db, string key)
        {
            var res = Task.Run(async () => await GetWebsiteSettingsBoolAsync(db,key));
            return res.Result;
        }


        public static async Task<bool> GetWebsiteSettingsBoolAsync(EasynftprojectsContext db, string key)
        {
            var websitesettings = await (from a in db.Websitesettings
                where a.Key == key
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (websitesettings != null)
            {
                return websitesettings.Boolvalue ?? false;
            }

            return false;
        }

        public static int CountLines(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            int counter = 1;
            str = str.Replace("\r", "");
            string[] strTemp = str.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (strTemp.Length > 0)
            {
                counter = strTemp.Length;
            }

            return counter;
        }

        public static int CountAddresses(EasynftprojectsContext db, string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            int counter = 0;
            str = str.Replace("\r", "");
            string[] strTemp = str.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in strTemp)
            {
                if (ConsoleCommand.CheckIfAddressIsValid(db, s, IsMainnet(), out string outaddress, out Blockchain blockchain))
                    counter++;
            }

            return counter;
        }

        public static long GetFileSize(string filename)
        {
            if (!File.Exists(filename)) return 0;
            FileInfo fi = new(filename);
            return fi.Length;

        }

        public static bool IsMainnet()
        {
            return !GeneralConfigurationClass.UseTestnet;
        }
        public static string GetNetworkName()
        {
            return IsMainnet() ? "Mainnet" : "Preprod";
        }
        private static string GetRand(char x)
        {
            int asc = Convert.ToInt16(x);
            if (asc >= 48 && asc <= 57)
            {
                //get a digit
                return (Convert.ToInt16(Path.GetRandomFileName()[0]) % 10).ToString();
            }

            if ((asc >= 65 && asc <= 90)
                || (asc >= 97 && asc <= 122))
            {
                //get a char
                return Path.GetRandomFileName().FirstOrDefault(n => Convert.ToInt16(n) >= 65).ToString();
            }

            return x.ToString();
        }

        public static string RadomizeMetadata(string metadata)
        {
            var res = metadata;
            string findstr = "\": \"";
            string replstr = "%%%";
            int i = 0;
            do
            {
                i = res.IndexOf(findstr, StringComparison.Ordinal);

                if (i != -1)
                {
                    var first = res.Substring(0, i);
                    var last = res.Substring(i + findstr.Length);
                    var end = last.IndexOf("\"", StringComparison.Ordinal);
                    string content = last.Substring(0, end);
                    string newcontent = string.Join("", content
                        .ToList()
                        .Select(x => GetRand(x))
                    );
                    
                    var rest = last.Substring(end);
                    res = first + replstr + newcontent + rest;

                }

            } while (i != -1);

            res = res.Replace(replstr, findstr, StringComparison.Ordinal);

            return res;
        }

        public static string ToPrettyFormat(this TimeSpan span)
        {

            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days > 0)
                sb.AppendFormat("{0} day{1} ", span.Days, span.Days > 1 ? "s" : string.Empty);
            if (span.Hours > 0)
                sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : string.Empty);


            if (span.Minutes > 0 && span.TotalHours == 0)
                sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : string.Empty);
            return sb.ToString();

        }

        public static string RemoveStaticPlaceholder(string s)
        {
            s = s.Replace("<nft_name>", "");
            s = s.Replace("<ipfs_link>", "");
            s = s.Replace("<iagon_link>", "");
            s = s.Replace("<gateway_link>", "");
            s = s.Replace("<policy_name>", "");
            s = s.Replace("<tokenname_prefix>", "");
            s = s.Replace("<policy_id>", "");
            s = s.Replace("<asset_name>", "");
            s = s.Replace("<mime_type>", "");
            s = s.Replace("<description>", "");
            s = s.Replace("<project_description>", "");
            s = s.Replace("<series_description>", "");
            s = s.Replace("<project_name>", "");
            s = s.Replace("<series_name>", "");
            s = s.Replace("<detail_data>", "");
            s = s.Replace("<display_name>", "");
            s = s.Replace("<version>", "");
            return s;
        }

        public static async Task<Burnigendpoint> CreateBurningAddressAsync(EasynftprojectsContext db, int nftprojectid,
            DateTime validuntil,Blockchain blockchain, bool fixnfts = false, bool shownotification = true)
        {

            var adr = await (from a in db.Burnigendpoints
                where a.NftprojectId == nftprojectid && a.Fixnfts == fixnfts && a.Blockchain==blockchain.ToString().ToLower()
                select a).FirstOrDefaultAsync();

            if (adr != null)
            {
                adr.State = "active";
                if (validuntil > adr.Validuntil)
                    adr.Validuntil = validuntil;
                await db.SaveChangesAsync();
                return adr;
            }

            CreateNewPaymentAddressClass cn = null;
            if (blockchain == Blockchain.Cardano)
            {
                cn = ConsoleCommand.CreateNewPaymentAddress(IsMainnet());
                if (cn.ErrorCode != 0)
                {
                    return null;
                }
            }

            if (blockchain == Blockchain.Solana)
            {
                cn = SolanaFunctions.CreateNewWallet();
                if (cn.ErrorCode != 0)
                {
                    return null;
                }
            }

            if (cn == null)
                return null;

            CryptographyProcessor cp = new();
            string salt = cp.CreateSalt(30);
            string password = salt + GeneralConfigurationClass.Masterpassword;

            Burnigendpoint newaddress = new()
            {
                Lovelace = 0,
                Salt = salt,
                Privatevkey = Encryption.EncryptString(cn.privatevkey, password),
                Privateskey = Encryption.EncryptString(cn.privateskey, password),
                NftprojectId = nftprojectid,
                Address = cn.Address,
                State = "active",
                Validuntil = validuntil,
                Fixnfts = fixnfts,
                Shownotification = shownotification,
                Blockchain = blockchain.ToString(),
            };

            await db.Burnigendpoints.AddAsync(newaddress);
            await db.SaveChangesAsync();
            await db.Database.CloseConnectionAsync();

            return newaddress;
        }


        public static GetPaymentAddressResultClass GetPaymentAddressResult(EasynftprojectsContext db,IConnectionMultiplexer redis,
            Nftaddress address, Nftproject project, int pricelistcount = 0)
        {
            switch (address.Coin.ToEnum<Coin>())
            {
                case Coin.ADA:
                {
                    GetPaymentAddressResultClass pnrc = new()
                    {
                        Expires = address.Expires,
                        PaymentAddress = address.Address,
                        PaymentAddressId = address.Id,
                        Debug = "",

                        PriceInEur = address.Price == -1 ? 0 : GetFiatPriceFromCoin(redis, address.Price ?? 0, "EUR", Coin.ADA),
                        PriceInUsd = address.Price == -1 ? 0 : GetFiatPriceFromCoin(redis, address.Price ?? 0, "USD", Coin.ADA),
                        PriceInJpy = address.Price == -1 ? 0 : GetFiatPriceFromCoin(redis, address.Price ?? 0, "JPY", Coin.ADA),

                        PriceInLovelace = address.Price ?? 0,
                        AdditionalPriceInTokens = GetAdditionalTokens(address),
                        Effectivedate = DateTime.Now,
                        SendbackToUser = address.Sendbacktouser,
                        Revervationtype = address.Reservationtype,
                        Currency=address.Coin,
                        PriceInLamport = 0,
                        PriceInOcta = 0,
                    };
                    return pnrc;
                }
                case Coin.SOL:
                {
                    GetPaymentAddressResultClass pnrc = new()
                    {
                        Expires = address.Expires,
                        PaymentAddress = address.Address,
                        PaymentAddressId = address.Id,
                        Debug = "",

                        PriceInEur = GetFiatPriceFromCoin(redis, address.Price ?? 0, "EUR",Coin.SOL),
                        PriceInUsd = GetFiatPriceFromCoin(redis, address.Price ?? 0, "USD",Coin.SOL),
                        PriceInJpy = GetFiatPriceFromCoin(redis, address.Price ?? 0, "JPY", Coin.SOL),
                        PriceInLovelace = 0,
                        PriceInLamport = address.Price ?? 0,
                        Effectivedate = DateTime.Now,
                        SendbackToUser = 0,
                        Revervationtype = address.Reservationtype,
                        Currency = address.Coin,
                    };
                    return pnrc;
                }
                case Coin.APT:
                {
                    GetPaymentAddressResultClass pnrc = new()
                    {
                        Expires = address.Expires,
                        PaymentAddress = address.Address,
                        PaymentAddressId = address.Id,
                        Debug = "",
                        PriceInEur = GetFiatPriceFromCoin(redis,address.Price??0,"EUR", Coin.APT),
                        PriceInUsd = GetFiatPriceFromCoin(redis, address.Price??0, "USD", Coin.APT),
                        PriceInJpy = GetFiatPriceFromCoin(redis, address.Price??0, "JPY", Coin.APT),
                        PriceInLovelace = 0,
                        PriceInOcta=address.Price??0,
                        PriceInLamport = 0,
                        Effectivedate = DateTime.Now,
                        SendbackToUser = 0,
                        Revervationtype = address.Reservationtype,
                        Currency = address.Coin,
                    };
                    return pnrc;
                }
                case Coin.BTC:
                {
                    GetPaymentAddressResultClass pnrc = new()
                    {
                        Expires = address.Expires,
                        PaymentAddress = address.Address,
                        PaymentAddressId = address.Id,
                        Debug = "",
                        PriceInEur = GetFiatPriceFromCoin(redis, address.Price ?? 0, "EUR", Coin.BTC),
                        PriceInUsd = GetFiatPriceFromCoin(redis, address.Price ?? 0, "USD", Coin.BTC),
                        PriceInJpy = GetFiatPriceFromCoin(redis, address.Price ?? 0, "JPY", Coin.BTC),
                        PriceInLovelace = 0,
                        PriceInOcta = 0,
                        PriceInLamport = 0,
                        PriceInSatoshi=address.Price ?? 0,
                        Effectivedate = DateTime.Now,
                        SendbackToUser = 0,
                        Revervationtype = address.Reservationtype,
                        Currency = address.Coin,
                    };
                    return pnrc;
                }
                default:
                    return null;
            }
        }

        public static Tokens[] GetAdditionalTokens(Pricelist pricelist)
        {
            if (pricelist.Priceintoken != null && pricelist.Priceintoken != 0)
            {
                var multiplier = GetFtTokensMultiplier(pricelist.Tokenpolicyid, pricelist.Assetnamehex?? pricelist.Tokenassetid.ToHex());
                return new Tokens[1]
                {
                    new()
                    {
                        AssetNameInHex = pricelist.Assetnamehex?? pricelist.Tokenassetid.ToHex(),
                        AssetName = pricelist.Tokenassetid,
                        CountToken = (long) pricelist.Priceintoken / (pricelist.Tokenmultiplier ?? 1),
                        PolicyId = pricelist.Tokenpolicyid,
                        TotalCount = (long) pricelist.Priceintoken,
                        Multiplier = pricelist.Tokenmultiplier ?? 1,
                        Decimals = GetDecimalsFromMultiplier(pricelist.Tokenmultiplier),
                    }
                };
            }

            return new Tokens[] { };
        }

        public static Tokens[] GetAdditionalTokens(Nftaddress address)
        {
            if (address.Priceintoken != null && address.Priceintoken != 0)
            {
              //  var multiplier = GetFtTokensMultiplier(address.Tokenpolicyid, address.Tokenassetid.ToHex());

                return new Tokens[1]
                {
                    new()
                    {
                        AssetNameInHex = address.Tokenassetid.ToHex(),
                        AssetName = address.Tokenassetid,
                        CountToken = (long) address.Priceintoken,
                        PolicyId = address.Tokenpolicyid,
                        Multiplier = address.Tokenmultiplier,
                        TotalCount = (long) address.Priceintoken*address.Tokenmultiplier,
                        Decimals = GetDecimalsFromMultiplier(address.Tokenmultiplier)
                    }
                };
            }

            return new Tokens[] { };
        }


        public static string HasSpecialChars(string yourString)
        {
            char[] ch = yourString.ToCharArray();
            char[] two = new char[ch.Length];
            int c = 0;
            for (int i = 0; i < ch.Length; i++)
            {
                if (!Char.IsLetterOrDigit(ch[i]) &&
                    ch[i] != '"' && ch[i] != '{' && ch[i] != '}' &&
                    ch[i] != ':' && ch[i] != '[' && ch[i] != ']' &&
                    ch[i] != ',' && ch[i] != '/' && ch[i] != ' ' &&
                    ch[i] != '.' && ch[i] != '@')
                {
                    two[c] = ch[i];
                    c++;
                }
            }

            Array.Resize(ref two, c);
            string s = "";
            foreach (var items in two)
            {
                if (!s.Contains(items))
                    s += items;
            }

            return s;
        }

        public static string ReplaceExtension(this string file, string extension)
        {
            var split = file.Split('.');

            if (string.IsNullOrEmpty(extension))
                return string.Join(".", split[..^1]);

            split[^1] = extension;

            return string.Join(".", split);
        }

        public static string GetStartAndEnd(string st, int showchars = 3)
        {
            if (st == null)
                return "";
            if (st.Length <= showchars * 3)
                return "";

            return st.Substring(0, showchars) + "******" + st.Substring(st.Length - showchars);
        }

        /// <summary>
        /// Attempt to empty the folder. Return false if it fails (locked files...).
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns>true on success</returns>
        public static bool EmptyFolder(string pathName)
        {
            bool errors = false;
            try
            {
                DirectoryInfo dir = new(pathName);

                foreach (FileInfo fi in dir.EnumerateFiles())
                {
                    try
                    {
                        fi.IsReadOnly = false;
                        fi.Delete();

                        //Wait for the item to disapear (avoid 'dir not empty' error).
                        while (fi.Exists)
                        {
                            Thread.Sleep(10);
                            fi.Refresh();
                        }
                    }
                    catch (IOException e)
                    {
                        Debug.WriteLine(e.Message);
                        errors = true;
                    }
                }

                foreach (DirectoryInfo di in dir.EnumerateDirectories())
                {
                    try
                    {
                        EmptyFolder(di.FullName);
                        di.Delete();

                        //Wait for the item to disapear (avoid 'dir not empty' error).
                        while (di.Exists)
                        {
                            Thread.Sleep(10);
                            di.Refresh();
                        }
                    }
                    catch (IOException e)
                    {
                        Debug.WriteLine(e.Message);
                        errors = true;
                    }
                }
            }
            catch
            {

            }

            return !errors;
        }

        public static string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        public static string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }
      

        public static string CheckMobilenumber(string number)
        {
            if (number.StartsWith("+"))
            {
                number = number.Remove(0, 1);
            }

            if (number.StartsWith("0"))
            {
                number = number.Remove(0, 1);
                number = "49" + number;
            }

            return number;
        }
        public static string? FindKeyInJson(string jsonString, string targetValue)
        {
            JObject jsonObj = JObject.Parse(jsonString);
            return FindKeyInJson(jsonObj, targetValue);
        }

        public static string? FindKeyInJson(JObject jsonObj, string targetValue)
        {
            foreach (var property in jsonObj.Properties())
            {
                if (property.Value.ToString() == targetValue)
                {
                    return property.Name;
                }

                if (property.Value.Type == JTokenType.Object)
                {
                    string nestedKey = FindKeyInJson((JObject)property.Value, targetValue);
                    if (nestedKey != null)
                    {
                        return nestedKey;
                    }
                }
            }

            return null;
        }

        public static bool IsValidJson(string strInput, out string formatedmetadata)
        {
            formatedmetadata = "";
            try
            {
                var metadata1 = JsonFormatter.IndentJSON(strInput);
                var metadata = JToken.Parse(metadata1).ToString(Formatting.Indented);
                formatedmetadata = metadata;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static async Task LogMessageAsync(EasynftprojectsContext db, string message, string data = "",
            int serverid = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "LOGMESSAGE WAS NULL";
                }

                message = serverid != 0 ? "SrvId:" + serverid + ": " + message : message;
                if (message.Length > 255)
                    message = message.Substring(0, 255);

                if (!string.IsNullOrEmpty(data))
                {
                    await db.Backgroundtaskslogs.AddAsync(new()
                        {Created = DateTime.Now, Logmessage = message, Additionaldata = data});
                    await db.SaveChangesAsync();
                }


            }
            catch
            {
                ResetContextState(db);
            }
        }


        public static async Task LogExceptionAsync(EasynftprojectsContext db, string message, string data = "",
            int serverid = 0)
        {
            try
            {
                db ??= new EasynftprojectsContext(optionsBuilder.Options);

                message = serverid != 0 ? "SrvId:" + serverid + ": " + message : message;
                if (message.Length > 255)
                    message = message.Substring(0, 255);


                string sql =
                    "INSERT INTO `serverexceptions` (`logmessage`, `created`, `data`) VALUES(@logmessage,NOW(),@additionaldata)";
                await db.Database.ExecuteSqlRawAsync(sql,
                    new MySqlParameter("logmessage", message),
                    new MySqlParameter("additionaldata", data));
            }
            catch
            {
                ResetContextState(db);
            }
        }

        public static void LogException(EasynftprojectsContext db, string message, string data = "", int serverid = 0)
        {
            var res = Task.Run(async () => await LogExceptionAsync(db,message,data,serverid));
        }

        public static void LogMessage(EasynftprojectsContext db, string message, string data = "", int serverid = 0)
        {
            var res = Task.Run(async () => await LogMessageAsync(db, message, data, serverid));
        }

      

        public static void ResetContextState(EasynftprojectsContext db)
        {
            var entries = db.ChangeTracker
                .Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToArray();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }

        public static async Task ClearInstockPremintedAddressAsync(EasynftprojectsContext db, int? id)
        {
            if (id == null)
                return;

            var ispm = await (from a in db.Premintednftsaddresses
                where a.Id == id
                select a).FirstOrDefaultAsync();

            if (ispm != null)
            {
                await LogMessageAsync(db, "Release Preminted Address to free: " + ispm.Address);
                ispm.State = "free";
                ispm.Expires = null;
                ispm.Lovelace = 0;
                ispm.NftprojectId = null;
                await db.SaveChangesAsync();
            }
        }

        public static string FormatCurrency(long? lovelace, int dec, Coin coin = Coin.ADA)
        {
            var provider = CultureInfo.GetCultureInfo("en-US");
            switch (lovelace)
            {
                case null:
                    return "0";
                default:
                    switch (coin)
                    {
                        case Coin.ADA:
                            return ((double) lovelace / 1000000f).ToString("N" + dec,provider);
                        case Coin.SOL:
                            return ((double)lovelace / 1000000000f).ToString("N" + dec, provider);
                        case Coin.APT:
                            return ((double)lovelace / 100000000f).ToString("N" + dec, provider);
                        case Coin.BTC:
                            return ((double)lovelace / 100000000f).ToString("N" + dec, provider);
                        case Coin.USD:
                        case Coin.EUR:
                        case Coin.JPY:
                            return ((double) lovelace / 100f).ToString("N" + dec, provider);
                        default:
                            return "0";
                    }
            }
        }




        public static string ToHexString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var sb = new StringBuilder();

            var bytes = Encoding.ASCII.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString().ToLower(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        public static string FromHexString(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return "";
            hexString = hexString.Replace(" ", "");

            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }


        public static bool CheckCustomerWallet(Customerwallet projectCustomerwallet)
        {
            return true;

            // TODO: Check it again
            if (projectCustomerwallet == null)
                return true;

            if (projectCustomerwallet.Confirmationdate == null)
                return false;

            CryptographyProcessor cp = new();
            return cp.AreEqual(projectCustomerwallet.Walletaddress, projectCustomerwallet.Hash,
                projectCustomerwallet.Confirmationdate.Value.ToLongTimeString());
        }


        public static MintingCostsClass GetMintingcosts2(int projectid, long countnft, long totalamount)
        {
            try
            {
                using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
                var p = (from a in db.Nftprojects
                        .Include(a => a.Settings).AsSplitQuery()
                    where a.Id == projectid
                    select a).AsNoTracking().FirstOrDefault();

                if (p != null)
                {
                    if (p.Settings.Feespercent == 0)
                    {
                        return new()
                        {
                            Costs = p.Settings.Mintingcosts * countnft,
                            Mintingcostsreceiver = p.Settings.Mintingaddress,
                            MinUtxo = p.Settings.Minutxo,
                            CostsSolana = p.Settings.Mintingcostssolana * countnft,
                            MintingcostsreceiverSolana = p.Settings.Mintingaddresssolana,
                            MintingcostsreceiverAptos=p.Settings.Mintingaddresssaptos,
                            CostsAptos = p.Settings.Mintingcostsaptos * countnft,
                            CostsBitcoin=p.Settings.Mintingcostsbitcoin * countnft,
                            MintingcostsreceiverBitcoin = p.Settings.Mintingaddressbitcoin,
                        };
                    }

                    var costs1 = p.Settings.Mintingcosts * countnft;
                    var costs2 = Convert.ToInt64(totalamount * p.Settings.Feespercent / 100);
                    var costs = Math.Max(costs1, costs2);

                    var costssolana1 = p.Settings.Mintingcostssolana * countnft;
                    var costssolana2 = Convert.ToInt64(totalamount * p.Settings.Feespercent / 100);
                    var costssolana = Math.Max(costssolana1, costssolana2);

                    var costsaptos1 = p.Settings.Mintingcostsaptos * countnft;
                    var costsaptos2 = Convert.ToInt64(totalamount * p.Settings.Feespercent / 100);
                    var costsaptos = Math.Max(costsaptos1, costsaptos2);

                    var costsbitcoin1 = p.Settings.Mintingcostsbitcoin * countnft;
                    var costsbitcoin2 = Convert.ToInt64(totalamount * p.Settings.Feespercent / 100);
                    var costsbitcoin = Math.Max(costsbitcoin1, costsbitcoin2);
                    return new()
                    {
                        Costs = costs,
                        Mintingcostsreceiver = p.Settings.Mintingaddress,
                        MinUtxo = p.Settings.Minutxo,
                        CostsSolana = costssolana,
                        MintingcostsreceiverSolana = p.Settings.Mintingaddresssolana,
                        MintingcostsreceiverAptos = p.Settings.Mintingaddresssaptos,
                        CostsAptos = costsaptos,
                        CostsBitcoin = costsbitcoin,
                        MintingcostsreceiverBitcoin = p.Settings.Mintingaddressbitcoin
                    };
                }
            }
            catch
            {
            }
            

            // Failover
            return new()
            {
                Costs = 2000000 * countnft,
                Mintingcostsreceiver = IsMainnet()
                    ? "addr1vxrmu3m2cc5k6xltupj86a2uzcuq8r4nhznrhfq0pkwl4hgqj2v8w"
                    : "addr_test1vqnku6rsllyln4fa5s4tlv5ujx0y6kvu4mzzfh5jaht8nfq8584jf",
                MinUtxo = 2000000,
                CostsSolana = 11000000,
                MintingcostsreceiverSolana = "8RkGUdhfxiWrNda1cWaTgwtCEdTReqDv7TsEynTSFnNe"
            };
        }

        public static bool IsAllLettersOrDigitsOrUnderscores(string s)
        {
            foreach (char c in s.OrEmptyIfNull())
            {
                if (!Char.IsLetterOrDigit(c) && c != '_' && c != '.')
                    return false;
            }

            return true;
        }

        public static bool IsAllLettersOrDigits(string s)
        {
            foreach (char c in s.OrEmptyIfNull())
            {
                if (!Char.IsLetterOrDigit(c))
                    return false;
            }

            return true;
        }

        public static bool IsAllDigits(string s)
        {
            foreach (char c in s.OrEmptyIfNull())
            {
                if (!Char.IsDigit(c))
                    return false;
            }

            return true;
        }

        public static string FilterToLetterOrDigit(this string str)
        {
            string res = "";
            foreach (char c in str.OrEmptyIfNull())
            {
                if (Char.IsLetterOrDigit(c))
                    res += c;
                // We need this, because of testnet/preprod addresses
                if (c == '_')
                    res += c;
            }

            return res;
        }
        public static Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck);
        }

        public static async Task ReleaseNftAsync(EasynftprojectsContext db, IConnectionMultiplexer redis,
            int nftaddresses_id)
        {
            var na = await (from a in db.Nftaddresses
                where a.Id == nftaddresses_id
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (na == null)
                return;

            try
            {
                string sql = $"delete from nfttonftaddresses where nftaddresses_id={nftaddresses_id}";
                await ExecuteSqlWithFallbackAsync(db, sql);
                await LogMessageAsync(db, sql);
            }
            catch (Exception e)
            {
                await LogExceptionAsync(db, "Release NFT " + e.Message, e.InnerException?.Message);
            }

            try
            {
                await NftReservationClass.ReleaseAllNftsAsync(db, redis, na.Reservationtoken);
            }
            catch (Exception e)
            {
                await LogExceptionAsync(db, "Release NFT 2 " + e.Message, e.InnerException?.Message);
            }
        }


        private static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }

        public static long CalculateMinutxoNew(Nftproject project, int countnft, long lovelacetopay, long sendback = 0)
        {
            if (countnft == 0)
                return 0;

            long minutxo = project.Settings.Minutxo;
            long minutxofinal = 0;
            long mintingcosts = project.Settings.Mintingcosts * countnft;
            long minfees = project.Settings.Minfees;
            long rest = 1000000; // Rest is for the seller and other add. payouts. Min 1 ada


            minutxofinal = sendback;

            if (sendback == 0)
            {
                int ux = 0;
                if (project.Minutxo == nameof(MinUtxoTypes.twoadaeverynft))
                {
                    minutxofinal = (minutxo * countnft);
                }
                else
                {
                    for (int i = 0; i < countnft; i++)
                    {
                        ux++;
                        if (ux >= 5)
                        {
                            ux = 0;
                            minutxofinal += minutxo;
                        }
                    }
                }
            }

            foreach (var projectNftprojectsadditionalpayout in project.Nftprojectsadditionalpayouts)
            {
                if (projectNftprojectsadditionalpayout.Valuepercent != null)
                {
                    rest += 1000000;
                }

                if (projectNftprojectsadditionalpayout.Valuetotal != null)
                {
                    rest += (long) projectNftprojectsadditionalpayout.Valuetotal;
                }
            }


            return minutxofinal + mintingcosts + minfees + rest;
        }


        public static long CalculateMinutxoForBuyer(Nftproject project, int countnft, long lovelacetopay)
        {
            if (countnft == 0)
                return 0;

            long minutxo = project.Settings.Minutxo;
            long minutxofinal = 0;

            int ux = 0;
            if (project.Minutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                minutxofinal = (minutxo * countnft);
            }
            else
            {
                for (int i = 0; i < countnft; i++)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        minutxofinal += minutxo;
                    }
                }
            }


            return minutxofinal;
        }

        public static long CalculateMinutxoNew(Customer customer, long countnft)
        {
            if (countnft == 0)
                return 0;

            long minutxo = customer.Defaultsettings.Minutxo;
            long mintingcosts = customer.Defaultsettings.Mintingcosts * countnft;
            long minfees = customer.Defaultsettings.Minfees;

            long minutxofinal = minutxo;

            return minutxofinal + mintingcosts + minfees;
        }

        public static long CalculateMinutxoNew(Nftproject project, long countnft)
        {
            if (countnft == 0)
                return 0;

            long minutxo = project.Settings.Minutxo;
            long mintingcosts = project.Settings.Mintingcosts * countnft;
            long minfees = project.Settings.Minfees;

            long minutxofinal = minutxo;

            return minutxofinal + mintingcosts + minfees;
        }

        public static long CalculateMinutxo(Nft[] nft)
        {
            if (!nft.Any())
                return 0;

            long minutxo = nft.First().Nftproject.Settings.Minutxo;
            long minutxofinal = minutxo * 2;
            long mintingcosts = nft.First().Nftproject.Settings.Mintingcosts * nft.Length;
            long minfees = nft.First().Nftproject.Settings.Minfees;

            int ux = 0;
            if (nft.First().Nftproject.Minutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                minutxofinal = (minutxo * nft.Length) + minutxo;
            }
            else
            {
                foreach (var tok in nft)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        minutxofinal += minutxo;
                    }
                }
            }

            return minutxofinal + mintingcosts + minfees;
        }

        public static long CalculateSendbackToUser(EasynftprojectsContext db, IConnectionMultiplexer redis,
            long countnft, int projectId, bool allowminutxo = true)
        {
            if (countnft == 0)
                return 0;

            var project = (from a in db.Nftprojects
                    .Include(a=>a.Settings)
                where a.Id == projectId
                select a).AsNoTracking().FirstOrDefault();


            long minutxo = project.Settings.Minutxo;
            long minutxofinal = minutxo;

            if (project.Minutxo == nameof(MinUtxoTypes.twoadaeverynft) || allowminutxo == false)
            {
                if (project.Maxsupply == 1)
                {
                    minutxofinal = (minutxo * countnft); // + minutxo;
                }
                else
                {
                    minutxofinal = minutxo;
                }
            }

            if (project.Minutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                if (project.Maxsupply == 1)
                {
                    int ux = 0;
                    for (int ux1 = 1; ux1 <= countnft; ux1++)
                    {
                        ux++;
                        if (ux >= 6)
                        {
                            ux = 0;
                            minutxofinal += minutxo;
                        }
                    }
                }
                else
                {
                    minutxofinal = minutxo;
                }
            }

            if (project.Minutxo == nameof(MinUtxoTypes.minutxo) && allowminutxo)
            {
                string guid = GetGuid();
                BuildTransactionClass buildtransaction = new();
                string sendtoken = "";
                if (project.Maxsupply == 1)
                {
                    for (int i = 0; i < countnft; i++)
                    {
                        if (!string.IsNullOrEmpty(sendtoken))
                            sendtoken += " + ";
                        sendtoken += "1 " + project.Policyid + "." +
                                     ConsoleCommand.CreateMintTokenname("",
                                         "testtesttesttesttesttesttesttest");
                    }
                }
                else
                {
                    sendtoken += $"{countnft} " + project.Policyid + "." +
                                 ConsoleCommand.CreateMintTokenname("",
                                     "testtesttesttesttesttesttesttest");
                }

                minutxofinal = ConsoleCommand.CalculateRequiredMinUtxo(redis, project.Settings.Mintingaddress,
                    sendtoken, "",guid, IsMainnet(), ref buildtransaction);
            }

            return minutxofinal;
        }

        // 1234567 = 1235000
        public static long RoundUpToThousand(long zahl)
        {
            long rest = zahl % 10000;
            return zahl + (10000 - rest);
        }

       

        public static uint GetIpAsUInt32(string ipString)
        {
            IPAddress address = IPAddress.Parse(ipString);

            byte[] ipBytes = address.GetAddressBytes();

            Array.Reverse(ipBytes);

            return BitConverter.ToUInt32(ipBytes, 0);
        }

        public static string GetIpAsString(uint ipVal)
        {
            byte[] ipBytes = BitConverter.GetBytes(ipVal);

            Array.Reverse(ipBytes);

            return new IPAddress(ipBytes).ToString();
        }

        private static int GetRandonNUmber(int i)
        {
            Random rnd = new();
            return rnd.Next(i);
        }

        public static int ExecuteSqlWithFallback(EasynftprojectsContext db, string sql,
            int serverid = 0)
        {
            var res = Task.Run(async () => await ExecuteSqlWithFallbackAsync(db, sql, serverid));
            return res.Result;
        }

        public static async Task<int> ExecuteSqlWithFallbackAsync(EasynftprojectsContext db, string sql,
             int serverid = 0)
        {
            int i = 0;
            int c = 0;

            do
            {
                c++;
                try
                {
                    i = await db.Database.ExecuteSqlRawAsync(sql);
                    return i;
                }
                catch (Exception e)
                {
                    await Task.Delay(500);
                    if (c >= 10)
                    {
                        await LogExceptionAsync(db, $"Exception: Server: {serverid} - Try: {c} - {e.Message}", sql);
                        break;
                    }
                }
            } while (true);

            await LogExceptionAsync(db, $"{serverid} - {c}: Fallback Sql failed", sql);
            return i;
        }


        public static async Task UpdateLastActionProjectAsync(EasynftprojectsContext db, int nftprojectid,
            IConnectionMultiplexer? redis)
        {
            if (redis != null)
            {
                string key = $"UpdateLastActionProjectAsync_{nftprojectid}_{GeneralConfigurationClass.EnvironmentName}";
                var check = RedisFunctions.GetStringData(redis, key, false);
                if (!string.IsNullOrEmpty(check))
                    return;
                RedisFunctions.SetStringData(redis, key, nftprojectid.ToString(), 3);
            }

            string sql = $"update nftprojects set lastupdate=NOW() where id ={nftprojectid}";
            await ExecuteSqlWithFallbackAsync(db, sql);
        }


        public static async Task UpdateLifesignAsync(EasynftprojectsContext db, int serverid)
        {
            string sql = $"update backgroundserver set lastlifesign=NOW() where id={serverid}";
            await ExecuteSqlWithFallbackAsync(db, sql);
        }

        public static bool CheckMetaDataLength(string metaData)
        {
            List<string> contents = new();
            do
            {
                string st = metaData.Between("\"", "\"");
                if (!string.IsNullOrEmpty(st))
                    contents.Add(st);

                metaData = metaData.Replace("\"" + st + "\"", "");

            } while (metaData.IndexOf('"') >= 0);

            foreach (var c in contents)
            {
                if (c.Length > 64)
                    return false;
            }

            return true;
        }

        public static string GetAssetId(string projectPolicyid, string projectTokennameprefix, string modelNftName,
            bool removespaces = false)
        {
            string s = (projectTokennameprefix ?? "") + (removespaces ? modelNftName.Replace(" ", "") : modelNftName);
            byte[] as1 = Encoding.UTF8.GetBytes(s);
            var as2 = BitConverter.ToString(as1);
            as2 = as2.Replace("-", "");
            as2 = as2.ToLower();
            return projectPolicyid + as2;
        }

        public static long? GetPrice(EasynftprojectsContext db,IConnectionMultiplexer redis, Nft? nftx, long tokenornftcount, Coin coin=Coin.ADA )
        {
            if (nftx == null)
                return null;

            if (nftx.Nftproject.Enabledcoins.Contains(Coin.ADA.ToString()) == false && coin == Coin.ADA)
                return null;
            if (nftx.Nftproject.Enabledcoins.Contains(Coin.SOL.ToString()) == false && coin == Coin.SOL)
                return null;
            if (nftx.Nftproject.Enabledcoins.Contains(Coin.APT.ToString()) == false && coin == Coin.APT)
                return null;
            if (nftx.Nftproject.Enabledcoins.Contains(Coin.BTC.ToString()) == false && coin == Coin.BTC)
                return null;

            if (nftx.Price != null && coin==Coin.ADA)
                return ((long) nftx.Price * tokenornftcount);

            if (nftx.Pricesolana != null && coin == Coin.SOL)
                return ((long)nftx.Pricesolana * tokenornftcount);
            if (nftx.Priceaptos != null && coin == Coin.APT)
                return ((long)nftx.Priceaptos * tokenornftcount);
        /*    if (nftx.Pricebitcoin != null && coin == Coin.BTC)
                return ((long)nftx.Pricebitcoin * tokenornftcount);*/
            return GetPriceFromProjectId(db,redis, nftx.NftprojectId, tokenornftcount, coin);
        }

        public static long? GetPriceFromProjectId(EasynftprojectsContext db,IConnectionMultiplexer redis, int projectid, long tokenornftcount, Coin coin = Coin.ADA)
        {
            var pl = (from a in db.Pricelists
                where a.NftprojectId == projectid && a.Countnftortoken == tokenornftcount &&
                      (a.Validfrom == null || a.Validfrom <= DateTime.Now) &&
                      (a.Validto == null || a.Validto >= DateTime.Now) && (a.Currency =="USD" || a.Currency=="EUR" || a.Currency=="JPY" || a.Currency=="BTC" || a.Currency==coin.ToString())
                select a).FirstOrDefault();
            if (pl != null && coin==Coin.ADA)
                return GetPriceInEntities(redis, pl);
            if (pl != null && coin == Coin.SOL)
                return GetPriceinLamport(redis, pl, coin);
            if (pl != null && coin == Coin.APT)
                return GetPriceinOcta(redis, pl, coin);
            if (pl != null && coin == Coin.BTC)
                return GetPriceinSatoshi(redis, pl, coin);
            return null;
        }


        public static async Task UpdateLastInputOnProjectAsync(EasynftprojectsContext db, int nftprojectid)
        {
            string sql = $"update nftprojects set lastinputonaddress=NOW() where id ={nftprojectid}";
            await ExecuteSqlWithFallbackAsync(db, sql);
        }
        

        public static string FilterTokenname(string tokenname)
        {
            return tokenname.Replace("\"", "").Replace("/", "").Replace("\\", "").Replace("<", "").Replace(">", "")
                .Replace("^", "").Replace("+", "").Replace(":",""); 
        }

        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }

        public static void DeleteOldFiles(string pathname, int ageInDays)
        {
            Directory.GetFiles(pathname)
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTime < DateTime.Now.AddDays(-ageInDays))
                .ToList()
                .ForEach(f => f.Delete());
        }

        public static string ReplaceBetween(this string str, string startsearch, string endsearch, string replacement)
        {

            int start = str.IndexOf(startsearch);
            int end = str.IndexOf(endsearch, start);
            string result = str.Substring(start + 1, end - start - 1);
            str = str.Replace(result, replacement);
            return str;
        }

        public static async Task SaveRefundLogAsync(EasynftprojectsContext db, string senderaddress, string receiveraddress,string txhash,
            bool successful, string buildtransactionTxId, string sendbackmessage, int pvId,
            string buildtransactionLogFile, long lovelace, long fee, long nmkrcosts, Coin coin)
        {
            await db.Refundlogs.AddAsync(new()
            {
                Created = DateTime.Now, Log = buildtransactionLogFile, NftprojectId = pvId,
                Senderaddress = senderaddress, Receiveraddress = receiveraddress, Refundreason = sendbackmessage ?? "",
                Lovelace = lovelace, Fee = fee, Nmkrcosts = nmkrcosts,
                Txhash = txhash, Outgoingtxhash = buildtransactionTxId, State = successful ? "successful" : "failed",Coin = coin.ToString()
            });
            await db.SaveChangesAsync();
        }
        public static Coin ToCoin(this Blockchain blockchain)
        {
            return ConvertToCoin(blockchain);
        }
        public static Coin ToCoin(this string blockchain)
        {
            return ConvertToCoin(blockchain.ToEnum<Blockchain>());
        }

        public static Blockchain ToBlockchain(this Coin coin)
        {
            return ConvertToBlockchain(coin);
        }

       
        public static Blockchain ConvertToBlockchain(Coin coin)
        {
            return coin switch
            {
                Coin.ADA=> Blockchain.Cardano,
                Coin.SOL => Blockchain.Solana,
                Coin.APT=>Blockchain.Aptos,
                Coin.HBAR=>Blockchain.Hedara,
                Coin.ETH=>Blockchain.Ethereum,
                Coin.MATIC=>Blockchain.Polygon,
                Coin.BTC => Blockchain.Bitcoin,
                _ => Blockchain.Cardano
            };
        }
        public static Coin ConvertToCoin(Blockchain blockchain)
        {
            return blockchain switch
            {
                Blockchain.Cardano => Coin.ADA,
                Blockchain.Solana => Coin.SOL,
                Blockchain.Ethereum=>Coin.ETH,
                Blockchain.Aptos=>Coin.APT,
                Blockchain.Polygon=>Coin.MATIC,
                Blockchain.Hedara=>Coin.HBAR,
                Blockchain.Bitcoin=> Coin.BTC,
                _ => Coin.ADA
            };
        }

        public static string GetCoinUnit(Blockchain blockchain)
        {
            return blockchain switch
            {
                Blockchain.Cardano => "Lovelace",
                Blockchain.Solana => "Lamport",
                Blockchain.Hedara=>"Wei",
                Blockchain.Ethereum=>"Wei",
                Blockchain.Polygon=>"Wei",
                Blockchain.Aptos=>"Octa",
                Blockchain.Bitcoin => "Satoshi",
                _ => "Lovelace"
            };
        }
        public static string GetCoinUnit(Coin coin)
        {
            return GetCoinUnit(ConvertToBlockchain(coin));
        }
        public static long GetPriceInEntities(IConnectionMultiplexer redis, Pricelist pricelist)
        {
            if (pricelist.Currency == Coin.ADA.ToString())
                return pricelist.Priceinlovelace;

            var rates = GlobalFunctions.GetNewRates(redis, Coin.ADA);

            return pricelist.Currency switch
            {
                // Prices are in cent
                "EUR" => ((long) ((pricelist.Priceinlovelace / rates.EurRate) * 10000f)).RoundOff(),
                "USD" => ((long) ((pricelist.Priceinlovelace / rates.UsdRate ) * 10000f)).RoundOff(),
                "JPY" => ((long) ((pricelist.Priceinlovelace / rates.JpyRate ) * 10000f)).RoundOff(),
                _ => 0
            };
        }
        public static long GetPriceinSatoshi(IConnectionMultiplexer redis, Pricelist pricelist, Coin coin)
        {
            if (pricelist.Currency == Coin.BTC.ToString())
                return pricelist.Priceinlovelace;
            return GetPrice2(redis, pricelist.Priceinlovelace, pricelist.Currency, coin);
        }
        public static long GetPriceinLamport(IConnectionMultiplexer redis, Pricelist pricelist, Coin coin)
        {
            if (pricelist.Currency == Coin.SOL.ToString())
                return pricelist.Priceinlovelace;
            return GetPrice2(redis, pricelist.Priceinlovelace, pricelist.Currency, coin);
        }
        public static long GetPriceinOcta(IConnectionMultiplexer redis, Pricelist pricelist, Coin coin)
        {
            if (pricelist.Currency == Coin.APT.ToString())
                return pricelist.Priceinlovelace;

            return GetPriceinOcta(redis, pricelist.Priceinlovelace, pricelist.Currency, coin);
        }

        public static long GetPriceinOcta(IConnectionMultiplexer redis, long? amount, string currency, Coin coin)
        {
            if (amount == null)
                return 0;
            if (currency == Coin.SOL.ToString())
                return (long)amount;
            if (currency == Coin.APT.ToString())
                return (long)amount;
            if (currency == Coin.BTC.ToString())
                return (long)amount;
            if (amount == -1)
                return 0;

            var rates = GetNewRates(redis, coin);

            return currency switch
            {
                // Prices are in cent
                "EUR" => ((long)((amount / rates.EurRate ?? 1) * 1000000f)).RoundOff(),
                "USD" => ((long)((amount / rates.UsdRate ?? 1) * 1000000f)).RoundOff(),
                "JPY" => ((long)((amount / rates.JpyRate ?? 1) * 1000000f)).RoundOff(),
                "BTC" => ((long)((amount / rates.BtcRate ?? 1))).RoundOff(),
                _ => 0
            };
        }

        public static long GetPrice2(IConnectionMultiplexer redis, long? amount, string currency, Coin coin)
        {
            if (amount == null)
                return 0;
            if (currency == Coin.SOL.ToString())
                return (long)amount;
            if (currency == Coin.APT.ToString())
                return (long)amount;
            if (currency == Coin.BTC.ToString())
                return (long)amount;

            if (amount == -1)
                return 0;

            var rates = GetNewRates(redis, coin);

            return currency switch
            {
                // Prices are in cent
                "EUR" => ((long)((amount / rates.EurRate ?? 1) * 10000000f)).RoundOff(),
                "USD" => ((long)((amount / rates.UsdRate ?? 1) * 10000000f)).RoundOff(),
                "JPY" => ((long)((amount / rates.JpyRate ?? 1) * 10000000f)).RoundOff(),
                _ => 0
            };
        }
     
        public static double GetFiatPriceFromCoin(IConnectionMultiplexer redis, long price, string currency, Coin coin)
        {
            if (currency != Coin.USD.ToString() && currency != Coin.EUR.ToString() && currency!=Coin.JPY.ToString() && currency!=Coin.BTC.ToString())
                return 0;
            if (price == -1)
                return 0;

            var rates = GetNewRates(redis, coin);

            double divider = 1000000000d;
            switch (coin)
            {
                case Coin.APT:
                    divider = 100000000d;
                    break;
                case Coin.SOL:
                    divider = 1000000000d;
                    break;
                case Coin.ADA:
                    divider = 1000000d;
                    break;
                case Coin.BTC:
                    divider = 1d;
                    break;

            }


            return currency switch
            {
                // Prices are in cent
                "EUR" => Math.Round((double)price / divider * (rates.EurRate), 2, MidpointRounding.AwayFromZero),
                "USD" => Math.Round((double)price / divider * (rates.UsdRate), 2, MidpointRounding.AwayFromZero),
                "JPY" => Math.Round((double)price / divider * (rates.JpyRate), 2, MidpointRounding.AwayFromZero),
                "BTC" => Math.Round((double)price / divider * (rates.BtcRate), 2, MidpointRounding.AwayFromZero),
                _ => 0
            };
        }
        public static long GetPriceInEntities(IConnectionMultiplexer redis, AddPriceClass pricelist, string requestedcurrency)
        {

            if (pricelist.Currency == Coin.ADA.ToString())
                return (long) Math.Round(pricelist.Price * 1000000f);
            if (pricelist.Currency == Coin.SOL.ToString())
                return (long)Math.Round(pricelist.Price * 1000000000f);
            if (pricelist.Currency == Coin.APT.ToString())
                return (long)Math.Round(pricelist.Price * 100000000f);
            if (pricelist.Currency == Coin.BTC.ToString())
                return (long)Math.Round(pricelist.Price); // We fill out the price in BTC on Satoshis

            Pricelist p = new()
            {
                Countnftortoken = pricelist.CountNft, 
                Currency = pricelist.Currency,
                Priceinlovelace = (long) Math.Round(pricelist.Price * 100f),
            };
            switch (requestedcurrency.ToEnum<Coin>())
                {
                case Coin.ADA:
                    return GetPriceInEntities(redis, p);
                case Coin.SOL:
                    return GetPriceinLamport(redis, p, Coin.SOL);
                case Coin.APT:
                    return GetPriceinOcta(redis, p, Coin.APT);
                case Coin.BTC:
                    return GetPriceinSatoshi(redis, p, Coin.BTC);
            }

            return GetPriceInEntities(redis, p);
        }

      
        public static double GetActualNewRates(IConnectionMultiplexer redis, string currency, Coin coin)
        {
            if (currency == coin.ToString())
                return 1;
            try
            {
                var rates = GetNewRates(redis,coin);

                if (rates == null)
                    return 0;
                switch (currency)
                {
                    case "EUR":
                        return rates.EurRate;
                    case "USD":
                        return rates.UsdRate;
                    case "JPY":
                        return rates.JpyRate;
                }
            }
            catch
            {
                return 0;
            }

            return 0;
        }

        public static long GetMultiplier(EasynftprojectsContext db, int? aNftId)
        {
            if (aNftId == null)
                return 1;

            var nft = (from a in db.Nfts
                where a.Id == aNftId
                select a).FirstOrDefault();
            if (nft == null)
                return 1;

            return nft.Multiplier;
        }

        public static string FormatMultiplier(long? tokencount, long? multiplier)
        {
            if (tokencount == null)
                return "0";

            if (multiplier == null || multiplier == 0 || multiplier == 1)
                return ((double)tokencount).ToString("N0", CultureInfo.GetCultureInfo("en-US"));

            var l = multiplier.ToString().Length - 1;

            return ((double) tokencount / (double) multiplier).ToString($"F{l}", CultureInfo.GetCultureInfo("en-US"));
        }

        public static async Task<long> GetNftMultiplierAsync(EasynftprojectsContext db, int snNftId)
        {
            try
            {
                var multiplier = await (from a in db.Nfts
                    where a.Id == snNftId
                    select a.Multiplier).FirstOrDefaultAsync();

                return multiplier;
            }
            catch
            {
                return 1;
            }
        }



        public static string GetFtPayTokenname(string policyid, string tokenName)
        {
            using var db = new EasynftprojectsContext(optionsBuilder.Options);

            var fttoken = (from a in db.Registeredtokens
                where a.Subject == policyid || a.Policyid == policyid
                select a).FirstOrDefault();

            if (fttoken == null || !string.IsNullOrEmpty(tokenName))
                return GetStartAndEnd(policyid) + (string.IsNullOrEmpty(tokenName) ? "" : "." + tokenName);

            return fttoken.Name + (string.IsNullOrEmpty(fttoken.Ticker) ? "" : " (" + fttoken.Ticker + ")");
        }

        public static MultiplierClass GetFtTokensMultiplier(string policyid, string assetnameinhex)
        {
            var multiplier = KoiosFunctions.GetFtTokensMultiplier(policyid, assetnameinhex);
            return multiplier;
        }

        public static async Task<MultiplierClass> GetFtTokensMultiplierAsync(string policyid, string assetnameinhex)
        {
            var multiplier = await KoiosFunctions.GetFtTokensMultiplierAsync(policyid, assetnameinhex);
            return multiplier;
        }

        public static bool CheckIfTxHashStillExists(string txHashAndId)
        {
            var bl = BlockfrostFunctions.GetTransactionUtxoFromBlockfrost(txHashAndId);
            return bl != null && bl.Outputs.Any();
        }

        public static async Task<PromotionClass> GetPromotionAsync(EasynftprojectsContext db,IConnectionMultiplexer redis, int promotionid,
            int multiplier)
        {
            var promotion = await (from a in db.Promotions
                    .Include(a => a.Nftproject)
                    .AsSplitQuery()
                    .Include(a => a.Nft)
                    .ThenInclude(a => a.Nftproject)
                    .AsSplitQuery()
                where a.Id == promotionid && a.State == "active"
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (promotion == null)
                return null;

            if (promotion.Startdate != null && promotion.Startdate > DateTime.Now)
                return null;

            if (promotion.Enddate != null && promotion.Enddate < DateTime.Now)
                return null;

            if (promotion.Maxcount != null && promotion.Soldcount >= promotion.Maxcount)
                return null;

            if (multiplier == 0)
                return null;


            // TODO: Support also single nfts
            if (promotion.Nftproject.Maxsupply == 1)
                return null;


            string skey = Encryption.DecryptString(promotion.Nftproject.Policyskey, promotion.Nftproject.Password);
            string vkey = Encryption.DecryptString(promotion.Nftproject.Policyvkey, promotion.Nftproject.Password);
            GetMetadataClass gmc = new(promotion.Nft.Id,"", true);

            var nftmultiplier = promotion.Nft.Multiplier;
            if (nftmultiplier == 0)
                nftmultiplier = 1;

            PromotionClass pc = new()
            {
                PolicyId = promotion.Nftproject.Policyid,
                Tokencount = promotion.Count * multiplier * nftmultiplier,
                SKey = skey,
                VKey = vkey,
                TokennameHex = ToHexString(promotion.Nftproject.Tokennameprefix + promotion.Nft.Name),
                Metadata = (await gmc.MetadataResultAsync()).Metadata,
                PromotionNft = promotion.Nft,
                PolicyScriptfile = promotion.Nftproject.Policyscript,
            };

            return pc;

        }

        public static async Task SetPromotionSoldcountAsync(EasynftprojectsContext db, int promotionid, long count)
        {
            var promotion = await (from a in db.Promotions
                    .Include(a => a.Nftproject)
                    .AsSplitQuery()
                    .Include(a => a.Nft)
                    .ThenInclude(a => a.Nftproject)
                    .AsSplitQuery()
                where a.Id == promotionid && a.State == "active"
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (promotion == null)
                return;


            var nftcount = count / promotion.Nft.Multiplier;

            string sql = $"update promotions set soldcount=soldcount + {nftcount} where id={promotionid}";
            await ExecuteSqlWithFallbackAsync(db, sql);
        }


        public static void DeleteProjectsRedisKeys(IConnectionMultiplexer redis, string adminOrPro, int nftNftprojectId)
        {
            string redisKey =
                $"{(IsMainnet() ? "Mainnet" : "Testnet")}_{adminOrPro}_LoadProject_{nftNftprojectId}";
            string redisKey1 =
                $"{(IsMainnet() ? "Mainnet" : "Testnet")}_{adminOrPro}_LoadTodos_Nfts_{nftNftprojectId}*";
            string redisKey2 =
                $"{(IsMainnet() ? "Mainnet" : "Testnet")}_{adminOrPro}_LoadTodos_Counts_{nftNftprojectId}*";

            RedisFunctions.DeleteKey(redis, redisKey);
            RedisFunctions.DeleteKey(redis, redisKey1);
            RedisFunctions.DeleteKey(redis, redisKey2);
        }

        public static void SaveStringToRedis(IConnectionMultiplexer redis, string keyname, string data,
            int expireInSeconds)
        {
            string redisKey = $"{(IsMainnet() ? "mainnet" : $"testnet_{GeneralConfigurationClass.TestnetMagicId}")}_{keyname}";

            RedisFunctions.SetStringData(redis, redisKey, data, expireInSeconds);
        }

        public static string GetStringFromRedis(IConnectionMultiplexer redis, string keyname)
        {
            string redisKey =
                $"{(IsMainnet() ? "mainnet" : $"testnet_{GeneralConfigurationClass.TestnetMagicId}")}_{keyname}";

            return RedisFunctions.GetStringData(redis, redisKey, false);
        }

        public static void DeleteStringFromRedis(IConnectionMultiplexer redis, string keyname)
        {
            string redisKey =
                $"{(IsMainnet() ? "mainnet" : $"testnet_{GeneralConfigurationClass.TestnetMagicId}")}_{keyname}";
            RedisFunctions.DeleteKey(redis, redisKey);
        }

        public static async Task SaveTransactionAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, BuildTransactionClass buildtransaction,
            int? customerId, int? nftprojectid, string transactiontype, int? walletId, int nftid, long nftcount, Coin coin)
        {
            if (buildtransaction.BuyerTxOut == null)
                return;
            try
            {

                var rates = await GlobalFunctions.GetNewRatesAsync(redis, coin);

                Transaction t = new()
                {
                    Senderaddress = buildtransaction.SenderAddress,
                    Receiveraddress = buildtransaction.BuyerTxOut.ReceiverAddress,
                    Originatoraddress = buildtransaction.BuyerTxOut.ReceiverAddress,
                    Stakeaddress = Bech32Engine.GetStakeFromAddress(buildtransaction.BuyerTxOut.ReceiverAddress),
                    Ada = buildtransaction.BuyerTxOut.Amount,
                    Stakereward = buildtransaction.StakeRewards ?? 0,
                    Tokenreward = buildtransaction.TokenRewards ?? 0,
                    Discount = buildtransaction.Discount ?? 0,
                    Created = DateTime.Now,
                    CustomerId = customerId,
                    NftaddressId = null,
                    NftprojectId = nftprojectid,
                    Transactiontype = transactiontype,
                    Transactionid = buildtransaction.TxHash,
                    Fee = buildtransaction.Fees,
                    Projectaddress = buildtransaction.ProjectTxOut?.ReceiverAddress,
                    Projectada = transactiontype == nameof(TransactionTypes.mintfromcustomeraddress)
                        ? 0
                        : buildtransaction.ProjectTxOut?.Amount,
                    Mintingcostsaddress = buildtransaction.MintingcostsTxOut?.ReceiverAddress,
                    Mintingcostsada = buildtransaction.MintingcostsTxOut?.Amount ?? 0,
                    WalletId = walletId,
                    Nmkrcosts = transactiontype is nameof(TransactionTypes.mintfromcustomeraddress) or nameof(TransactionTypes.mintfromnftmakeraddress) ? (buildtransaction.BuyerTxOut.Amount + buildtransaction.Fees) : 0,
                    State = "submitted",
                    Ipaddress = "",
                    Eurorate =(float)rates.EurRate,
                    Serverid = null,
                    Coin= coin.ToString(),
                    Cbor = null,
                    Confirmed = false,
                    Nftcount = 1,
                };

                await db.AddAsync(t);
                await db.SaveChangesAsync();


                await db.TransactionNfts.AddAsync(new()
                {
                    NftId = nftid,
                    TransactionId = t.Id,
                    Mintedontransaction = true,
                    Tokencount = nftcount,
                    Multiplier = await GetNftMultiplierAsync(db, nftid),
                    Ispromotion = false
                });
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                ResetContextState(db);
            }
        }


        public static long GetFullNumber(long tokencount, long multiplier)
        {
            if (multiplier == 0 || multiplier == 1)
                return tokencount;

            return (long) (tokencount / (double) multiplier);
        }
        public static string GetPrettyFileSizeString(long length)
        {
            long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            double size = length;
            string suffix = nameof(B);

            if (length >= TB)
            {
                size = Math.Round((double)length / TB, 2);
                suffix = nameof(TB);
            }
            else if (length >= GB)
            {
                size = Math.Round((double)length / GB, 2);
                suffix = nameof(GB);
            }
            else if (length >= MB)
            {
                size = Math.Round((double)length / MB, 2);
                suffix = nameof(MB);
            }
            else if (length >= KB)
            {
                size = Math.Round((double)length / KB, 2);
                suffix = nameof(KB);
            }

            return $"{size} {suffix}";
        }
       

        /// <summary>
        /// Checks if a policy of a project is already locked
        /// </summary>
        /// <param name="project">The NftProject Table</param>
        /// <returns>true when the policy is still open, false is no further minting is possible</returns>
        public static bool CheckExpirationSlot(Nftproject project)
        {
            if (project.Policyexpire == null && project.Lockslot == null)
                return true;


            BuildTransactionClass bt = new();
            var tip = BlockfrostFunctions.GetQueryTip();
            if (tip != null && tip.Slot != null && tip.Slot != 0)
            {
                if (project.Lockslot != null && tip.Slot > project.Lockslot)
                    return false;
            }

            return !(DateTime.Now > project.Policyexpire);
        }
        public static string GetPkhFromAddress(string address)
        {
            Bech32Engine.Decode(address, out string hrp, out var data);
            if (data == null)
                return null;

            string pkh1 = Bech32Engine.ByteArrayToString(data);
            return pkh1.Substring(2, 56);
        }

        public static long GetDecimalsFromMultiplier(long? pricelistTokenmultiplier)
        {
            if (pricelistTokenmultiplier == null || pricelistTokenmultiplier == 1)
                return 0;

            return pricelistTokenmultiplier.ToString().Length - 1;
        }
        public static long GetMultiplierFromDecimals(long decimals)
        {
            if (decimals <= 0)
                return 1;

            return (long)Math.Pow(10, decimals);
        }
        public static bool CheckTwitterHandle(ref string modelTwitterhandle)
        {
            if (string.IsNullOrEmpty(modelTwitterhandle))
                return true;

            int min = 4;
            int max = 16;
            if (!modelTwitterhandle.StartsWith("@"))
                modelTwitterhandle = "@" + modelTwitterhandle;

            if (modelTwitterhandle.Length > max)
                return false;

            if (modelTwitterhandle.Length < min)
                return false;

            return true;
        }

        public static NewRatesClass GetNewRates(IConnectionMultiplexer redis, Coin coin)
        {
            var res = Task.Run(async () => await GetNewRatesAsync(redis,coin));
            return res.Result;
        }
     

        public static void GetDepositBackInAda(Nftproject project, long countnft, out double minvalue, out double maxvalue)
        {
            if (project.Enabledecentralpayments)
            {
                minvalue = 0;
                maxvalue = 0;
                return;
            }   

            minvalue = 2;
            maxvalue = 2;
            if (project.Minutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                minvalue = countnft * 2;
                maxvalue = countnft * 2;
            }
            if (project.Minutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                minvalue = (Convert.ToInt64(Math.Floor((double)countnft / 5)) + 1) * 2;
                maxvalue = (Convert.ToInt64(Math.Floor((double)countnft / 5)) + 1) * 2;
            }
            if (project.Minutxo == nameof(MinUtxoTypes.minutxo))
            {
                double fee = 1.4;
                fee += countnft * .11;
                minvalue = fee;
                maxvalue = fee+0.5f;
            }
        }

        public static double GetNmkrFeesInAda(Nftproject project, long countnft, long contextPriceinlovelace, out string s)
        {
            s = "";
            if (project.Settings.Feespercent != 0)
            {
                var value1 = (project.Settings.Mintingcosts) * countnft;
                var value2 = Convert.ToInt64(contextPriceinlovelace * project.Settings.Feespercent / 100);
                if (value2 > value1)
                {
                    s = (value2/1000000f).ToString("F2", CultureInfo.GetCultureInfo("en-US")) + " ADA (" + project.Settings.Feespercent.ToString("F2", CultureInfo.GetCultureInfo("en-US")) + " % minimum)";
                    return value2/1000000f;
                }
            }
            s= (project.Settings.Mintingcosts / 1000000 * countnft).ToString("F2", CultureInfo.GetCultureInfo("en-US")) + " ADA (" + (project.Settings.Mintingcosts / 1000000).ToString("F2", CultureInfo.GetCultureInfo("en-US")) + " ADA per NFT)";
            return project.Settings.Mintingcosts / 1000000 * countnft;
        }

        public static void GetNetworkFeesInAda(long countnft, out double minvalue, out double maxvalue)
        {
            double networkfees = 0.3f;
            networkfees += countnft * 0.05f;

            minvalue = networkfees;
            maxvalue=networkfees + 0.2f;
        }

        public static async Task<string> WhatIsMyIp()
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.whatismyip.com/ip.php?key=0a46fe2855427308d84fecef83a0ad88");
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responsestring = await response.Content.ReadAsStringAsync();
                return responsestring;
            }

            return null;
        }

        public static async Task<Adminmintandsendaddress> GetNmkrPaywalletAndBlockAsync(EasynftprojectsContext db, int serverid,string caller, string restoken, Coin coin=Coin.ADA)
        {
            string reservationtoken = serverid + "_" + caller + "_" +
                                      (string.IsNullOrEmpty(restoken) ? Guid.NewGuid().ToString() : restoken);

            long minamount = 50000;
            switch (coin)
            {
                case Coin.BTC:
                    minamount = 5000;
                    break;
                case Coin.ADA:
                    minamount = 8000000;
                    break;
            }


            string sql = $"update adminmintandsendaddresses set addressblocked=1, reservationtoken='{reservationtoken}', lasttxhash=NULL,lasttxdate=NOW(),blockcounter=0 where lovelace>{minamount} and addressblocked=0 and coin='{coin.ToString()}' ORDER BY RAND() limit 1";

            await ExecuteSqlWithFallbackAsync(db, sql);

            var paywallet = await(from a in db.Adminmintandsendaddresses
                where a.Reservationtoken==reservationtoken
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (paywallet == null)
                return null;
            return paywallet;
        }
        public static async Task<Premintedpromotokenaddress> GetPremintedTokenWalletAndBlockAsync(EasynftprojectsContext db, int serverid, Nftprojectsendpremintedtoken premintedToken,int nftcount, string caller, string restoken)
        {
            string reservationtoken = serverid + "_" + caller + "_" +
                                      (string.IsNullOrEmpty(restoken) ? Guid.NewGuid().ToString() : restoken);

            string sql = $"update premintedpromotokenaddresses set state='blocked', reservationtoken='{reservationtoken}', " +
                         $"lasttxhash=NULL,blockdate=NOW() where state='active' and blockchain_id={premintedToken.Blockchain.Id} " +
                         $"and policyid_or_collection='{premintedToken.PolicyidOrCollection}' and tokenname='{premintedToken.Tokenname}'" +
                         $"and totaltokens >='{premintedToken.Countokenstosend*nftcount}' ORDER BY RAND() limit 1";

            await ExecuteSqlWithFallbackAsync(db, sql);

            var wallet = await (from a in db.Premintedpromotokenaddresses
                                where a.Reservationtoken == reservationtoken
                select a).AsNoTracking().FirstOrDefaultAsync();
            return wallet;
        }
        public static async Task<Adminmintandsendaddress> GetFirstNmkrPaywalletAndBlockAsync(EasynftprojectsContext db, int serverid)
        {
            string reservationtoken = serverid + "_" + GetGuid();
            string sql = $"update adminmintandsendaddresses set addressblocked=1, reservationtoken='{reservationtoken}', lasttxhash=NULL,lasttxdate=NOW(),blockcounter=0 where lovelace>3500000 and addressblocked=0 and coin='ADA' ORDER BY lovelace desc limit 1";

            await ExecuteSqlWithFallbackAsync(db, sql);

            var paywallet = await (from a in db.Adminmintandsendaddresses
                where a.Reservationtoken == reservationtoken
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (paywallet == null)
                return null;
            return paywallet;
        }
        public static double GetPricePerMintCoupon(EasynftprojectsContext db, Blockchain blockchain, int customerid)
        {
            var customer = (from a in db.Customers
                    .Include(x => x.Defaultsettings)
                where a.Id == customerid
                select a).FirstOrDefault();

            if (customer == null)
                return 0;

            switch (blockchain)
            {
                case Blockchain.Cardano:
                    return customer.Defaultsettings.Pricemintcoupons;
                case Blockchain.Solana:
                    return customer.Defaultsettings.Pricemintcouponssolana;
                case Blockchain.Aptos:
                    return customer.Defaultsettings.Pricemintcouponsaptos;
            /*    case Blockchain.Bitcoin:
                    return customer.Defaultsettings.Pricemintcouponsbitcoin;*/
                default:
                    return 0;
            }
        }
        public static string GetTokenname(string nftprojectTokennameprefix, string nftName)
        {
            string tokenname = string.IsNullOrEmpty(nftprojectTokennameprefix)
                ? nftName
                : nftprojectTokennameprefix + nftName;
            return tokenname;
        }

        // Generates a numeric pin code with the amount of numbers
        public static string GeneratePinCode(int amountOfNumbers)
        {
            string pinCode = "";
            Random random = new Random();
            for (int i = 0; i < amountOfNumbers; i++)
            {
                pinCode += random.Next(0, 9).ToString();
            }
            return pinCode;
        }

        public static void GetCip68ReferenceCostsInAda(Nftproject project, long countnft, out double minvalue,
            out double maxvalue)
        {
            if (project.Cip68 == false)
            {
                minvalue = 0;
                maxvalue = 0;
                return;
            }
            double fee = 2.3;
            minvalue = fee * countnft;
            maxvalue = (fee + 0.5f)*countnft;
        }

        public static string GetGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

      
        public static string GetCip68ReferenceToken(string assetid)
        {
            assetid = assetid.Substring(0, 56) + "000643b0" + assetid.Substring(56);
            return assetid;
        }
        public static string CheckMetadataFile(EasynftprojectsContext db, int projectId, string metadatafile, ICheckMetadataFields metadatachecker)
        {
            if (File.Exists(metadatafile))
            {
                try
                {
                    var metadata = File.ReadAllText(metadatafile);
                    return CheckMetadata(db, projectId, metadata, metadatachecker);
                }
                catch
                {
                    return "Error while reading the metadata file";
                }
            }

            return "Metadatafile not found";

        }
        public static string CheckMetadata(EasynftprojectsContext db, int projectid, string metadata, ICheckMetadataFields metadatachecker)
        {
            var project = (from a in db.Nftprojects
                where a.Id == projectid && a.State == "active"
                select a).FirstOrDefault();

            if (project == null)
            {
                return "Project not found or not active. Please contact support.";
            }
            
            var checkmetadata = metadatachecker.CheckMetadata(metadata, project.Policyid, "", true, false);
            return checkmetadata.ErrorMessage;

        }

        public static async Task UnlockPaywalletAsync(EasynftprojectsContext db, Adminmintandsendaddress paywallet)
        {
            string sql = $"update adminmintandsendaddresses set addressblocked=0, reservationtoken='', " +
                         $"lasttxhash=NULL,lasttxdate=NULL,blockcounter=0 where id={paywallet.Id}";

            await ExecuteSqlWithFallbackAsync(db, sql);
            return;

            /*
            var pw = await(from a in db.Adminmintandsendaddresses
                where a.Id==paywallet.Id
                select a).FirstOrDefaultAsync();

            if (pw == null)
                return;
            pw.Addressblocked = false;
            pw.Reservationtoken = null;
            pw.Lasttxhash = null;
            pw.Lasttxdate = null;
            pw.Blockcounter = 0;
            await db.SaveChangesAsync();*/
        }

        public static async Task ReduceMintCouponsAsync(EasynftprojectsContext db, int customerId, float settingsMintandsendcoupons)
        {
            string sql = $"update customers set newpurchasedmints=newpurchasedmints-"+ settingsMintandsendcoupons.ToString("F2", CultureInfo.CreateSpecificCulture("en-US")) + $" where id={customerId}";
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql, 0);
        }
        readonly static Uri SomeBaseUri = new Uri("http://canbeanything");
        public static string GetFileNameFromUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return Path.GetFileName(uri.LocalPath);
        }
        public static void InstallNewCardanoCli()
        {
            // Downloads and extracts the new cardano node version 9
            // The new node is then in directory /app/bin
            string operatingsystem = Environment.OSVersion.VersionString;
            if (operatingsystem.Contains("Unix") && !File.Exists("/app/bin/cardano-cli") && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string download = "https://nodesdownload.nmkr.io/cardano-node-10.1.2-linux.tar.gz";//GetWebsiteSettingsString(db, "cardanoclidownload");
                string filename = GetFileNameFromUrl(download);

                Directory.CreateDirectory("/app");
                Directory.CreateDirectory("/app/bin");

                WebClient webClient = new WebClient();
                webClient.DownloadFile(download,filename);


                
              //  Command.Run("apt", "install -y wget");
              //  Command.Run("wget", download);
                Command.Run("tar", $"-zxvf {filename} ./bin/cardano-cli");
            }
        }

        public static async Task<bool> CheckForBlockedAddresses(EasynftprojectsContext db, string address)
        {
           var adminMintAddresses=await (from a in db.Adminmintandsendaddresses
                                         where a.Address == address 
                                         select a).AsNoTracking().FirstOrDefaultAsync();

           if (adminMintAddresses!=null)
                return true;

           var customerAddresses= await (from a in db.Customers
                                         where a.Adaaddress == address || a.Solanapublickey==address || a.Aptosaddress==address
                                         select a).AsNoTracking().FirstOrDefaultAsync();

            if (customerAddresses != null)
                return true;

            var projectaddresses = await (from a in db.Nftprojects
                where a.Solanapublickey == address || a.Policyaddress == address ||a.Aptosaddress== address
                                          select a).AsNoTracking().FirstOrDefaultAsync();


            if (projectaddresses != null) 
                return true;


            return false;
        }

     
        public static async Task<NewRatesClass> GetNewRatesAsync(IConnectionMultiplexer redis, Coin coin)
        {
            string rediskey = "ActualNewRates" + coin.ToString();
            var ratesredis = RedisFunctions.GetData<NewRatesClass>(redis, rediskey);
            if (ratesredis != null && ratesredis.BtcRate != 0 && ratesredis.EurRate != 0 && ratesredis.JpyRate != 0 && ratesredis.UsdRate != 0)
                return ratesredis;


            await using var db = new EasynftprojectsContext(optionsBuilder.Options);

            var rates = await (from a in db.Newrates
                where a.Coin == coin.ToString()
                orderby a.Id descending
                select a).AsNoTracking().Take(4).ToListAsync();

            if (!rates.Any())
                return null;

            NewRatesClass r = new()
            {
                BtcRate = rates.FirstOrDefault(x => x.Currency == "BTC")?.Price ?? 0,
                EurRate = rates.FirstOrDefault(x => x.Currency == "EUR")?.Price ?? 0,
                UsdRate = rates.FirstOrDefault(x => x.Currency == "USD")?.Price ?? 0,
                JpyRate = rates.FirstOrDefault(x => x.Currency == "JPY")?.Price ?? 0,
                Coin = coin,
                EffectiveDate = rates.FirstOrDefault()?.Effectivedate ?? DateTime.Now
            };


            RedisFunctions.SetData(redis, rediskey, r, 600);
            return r;
        }


        public static async Task UnlockPremintedTokenWalletAsync(EasynftprojectsContext db, Premintedpromotokenaddress premintedTokenWallet)
        {
            if (premintedTokenWallet== null)
                return;

            var pw = await (from a in db.Premintedpromotokenaddresses
                where a.Id == premintedTokenWallet.Id
                select a).FirstOrDefaultAsync();

            if (pw == null)
                return;
            pw.State="active";
            pw.Blockdate=null;
            pw.Lasttxhash = null;
            pw.Reservationtoken = null;
            await db.SaveChangesAsync();
        }

        public static async Task<int> GetRequiredMintCouponsAsync(EasynftprojectsContext db, IConnectionMultiplexer redis,long btcMintPriceSats, Coin coin)
        {
            var btcprices = await GetNewRatesAsync(redis, coin);
            var adaprices= await GetNewRatesAsync(redis, Coin.ADA);

            // Eingabewerte
            decimal btcEurPrice = (decimal)btcprices.EurRate;   // 1 BTC = 60.000 EUR
            decimal adaEurPrice = (decimal)adaprices.EurRate;    // 1 ADA = 0,35 EUR
            decimal adaMintPriceAda = 2m;   // 2 ADA pro Mint

            // 1. BTC-Mintpreis in BTC
            decimal btcMintPriceBtc = btcMintPriceSats / 100_000_000m;

            // 2. BTC-Mintpreis in EUR
            decimal btcMintPriceEur = btcMintPriceBtc * btcEurPrice;

            // 3. ADA-Mintpreis in EUR
            decimal adaMintPriceEur = adaMintPriceAda * adaEurPrice;

            // 4. Anzahl Mintcoupons
            decimal mintCouponsNeeded = btcMintPriceEur / adaMintPriceEur;

            // Optional: Aufrunden, falls nur ganze Coupons verwendet werden dürfen
            int mintCouponsNeededRounded = (int)Math.Ceiling(mintCouponsNeeded);

         /*   Console.WriteLine($"BTC-Mintpreis: {btcMintPriceEur:0.00} EUR");
            Console.WriteLine($"ADA-Mintpreis: {adaMintPriceEur:0.00} EUR");
            Console.WriteLine($"Benötigte Mintcoupons: {mintCouponsNeededRounded}");*/
            return mintCouponsNeededRounded;

        }

        public static void CheckMainPassword()
        {

            if (string.IsNullOrEmpty(GeneralConfigurationClass.Masterpassword))
            {
                using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
                byte[] bytes = Convert.FromBase64String(GetWebsiteSettingsString(db, "masterpassword"));
                GeneralConfigurationClass.Masterpassword = Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
