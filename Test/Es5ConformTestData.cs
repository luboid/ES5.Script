using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Es5ConformTestData : TheoryData<string>
    {
        string fTestRoot;

        public Es5ConformTestData(string aTestRoot)
        {
            fTestRoot = aTestRoot;
            ScanJS(aTestRoot);
        }

        public void ScanJS(string aScan)
        {
            foreach (var f in Directory.EnumerateFiles(aScan).OrderBy(a => a))
            {
                if (f.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) && !f.EndsWith("lib.js", StringComparison.InvariantCultureIgnoreCase))
                    Add(f.Substring(fTestRoot.Length+1));
            }

            foreach (var dir in Directory.EnumerateDirectories(aScan).OrderBy(a => a))
            {
                if (!dir.EndsWith(".svn", StringComparison.InvariantCultureIgnoreCase))
                    ScanJS(dir);
            }
        }
    }
}
