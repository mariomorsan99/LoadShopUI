using System;
using TMS.Infrastructure.Common.Utilities;

namespace Loadshop.Encrypt
{
    class Program
    {
        private static string USAGE = @"
Usage: loadshopencrypt ""<encryption-key>"" ""<value to encrypt>""

<encryption-key>    REQUIRED

    Base64 encoded value of the 3DES encryption key stored in TOPS_Config.ProcessSettings with
    SettingId = 'LoadshopEncryptionKey'

<value to encrypt>  REQUIRED

    The UTF-8 plaintext value of the text you want to encrypt

OUTPUT:

    Base64 encoded value of the encrypted bytes, assuming a UTF-8 text encoding
";

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine(USAGE);
                return;
            }

            var key = args[0];
            var plainText = args[1];
            var cypherText = CryptoUtils.Encrypt3DES(plainText, key);

            Console.WriteLine(cypherText);
        }
    }
}
