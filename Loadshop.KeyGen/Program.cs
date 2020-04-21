using System;
using TMS.Infrastructure.Common.Utilities;

namespace Loadshop.KeyGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write(CryptoUtils.Generate3DESKey());
        }
    }
}
