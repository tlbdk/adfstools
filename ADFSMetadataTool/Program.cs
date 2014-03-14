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

            XmlSerializer s = new XmlSerializer(typeof(EntityDescriptor));
            System.IO.TextReader r = new System.IO.StreamReader(Path.Combine(samplespath, "metadata-test.xml"));
            EntityDescriptor metadata = (EntityDescriptor)s.Deserialize(r); 
            r.Close();
            Console.WriteLine(metadata.entityID);

        }
    }
}
