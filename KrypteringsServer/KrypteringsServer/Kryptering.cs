using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace KrypteringsServer
{
    class Kryptering
    {
        //Nyckellängden definierar den övre gränsen för en algoritms säkerhet, eftersom säkerheten för alla algoritmer kan brytas av brute-force attacker.
        private const int nyckelLängd = 256;

        //Denna konstant bestämmer antalet iterationer för lösenords bytes genereings funktion
        private const int derivationIterationer = 1000;

        public static string Avkryptera(string krypteringsText)
        {
            string krypteringsLösenord = "<3 Amos <3";

            //Hämta den kompletta strömmen av bytes som reprensentera:
            //[32 bytes av salt] + [32 bytes av IV] + [n bytes av KrypteringsText]
            byte[] krypteraTextBytesMedSaltOchIV = Convert.FromBase64String(krypteringsText);

            //Hämta SaltByten genom att extrahera dom första 32 byten från den krypterade texten
            byte[] saltStringByte = krypteraTextBytesMedSaltOchIV.Take(nyckelLängd / 8).ToArray();

            //Hämta IVByten genom att extrahera dom nästa 32 byten från den krypterade texten
            byte[] ivStringBytes = krypteraTextBytesMedSaltOchIV.Skip(nyckelLängd / 8).Take(nyckelLängd / 8).ToArray();

            //Hämta de faktiska kryterings text byten genom att extrahera dom första 64 byten från den krypterade text stringen
            byte[] krypteringsTextBytes = krypteraTextBytesMedSaltOchIV.Skip((nyckelLängd / 8) * 2).Take(krypteraTextBytesMedSaltOchIV.Length - ((nyckelLängd / 8) * 2)).ToArray();

            using (Rfc2898DeriveBytes lösenord = new Rfc2898DeriveBytes(krypteringsLösenord, saltStringByte, derivationIterationer))
            {
                byte[] nyckelBytes = lösenord.GetBytes(nyckelLängd / 8);
                using (RijndaelManaged systemeriskNyckel = new RijndaelManaged())
                {
                    systemeriskNyckel.BlockSize = 256;
                    systemeriskNyckel.Mode = CipherMode.CBC;
                    systemeriskNyckel.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform dekrypterare = systemeriskNyckel.CreateDecryptor(nyckelBytes, ivStringBytes))
                    {
                        using (MemoryStream minnesStröm = new MemoryStream(krypteringsTextBytes))
                        {
                            using (CryptoStream krypteringsStröm = new CryptoStream(minnesStröm, dekrypterare, CryptoStreamMode.Read))
                            {
                                byte[] oKrypteradText = new byte[krypteringsTextBytes.Length];
                                int dekrypteradByteMängd = krypteringsStröm.Read(oKrypteradText, 0, oKrypteradText.Length);

                                minnesStröm.Close();
                                krypteringsStröm.Close();

                                return Encoding.Unicode.GetString(oKrypteradText, 0, dekrypteradByteMängd);
                            }
                        }
                    }
                }
            }
        }
    }
}


