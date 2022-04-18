using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Finance.PciDss.Bridge.Certus.Finance.Server.Services;
using NUnit.Framework;

namespace Finance.PciDss.Bridge.Certus.Finance.Test
{
    public class TestsZipGenerator
    {
        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        [Test]
        public async Task Generate_New_Zip()
        {
            for (int i = 0; i < 100000; i++)
            {
                string zip = RequestValidator.RandomString(6);
                Assert.AreEqual(zip.Length, 6);
                Assert.AreEqual(IsDigitsOnly(zip), true);

            }
        }
    }
}
