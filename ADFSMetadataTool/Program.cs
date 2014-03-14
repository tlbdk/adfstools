using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace ADFSMetadataTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string samplespath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "../../../samples");
            //var metadata = ADFSMetadata.DeserializeFromFile(Path.Combine(samplespath, "metadata-test.xml"));
            //metadata.SerializeToFile(Path.Combine(samplespath, "metadata-test2.xml"));
        }
    }
}
