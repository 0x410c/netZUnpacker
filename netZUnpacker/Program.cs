using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace netZUnpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            
            
            if(args.Length !=1)
            {
                Console.Write("[-] Error\nnetZunpacker.exe packedfile.exe\nPress Enter to Exit");
                Console.Read();
                return;
            }
            try
            {
                String path;
                
                if (!Path.IsPathRooted(args[0]))
                {
                    path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + args[0];
                }
                else
                {
                    path = args[0];
                }
                Assembly a = Assembly.LoadFile(path);
                ResourceManager rm = new ResourceManager("app", a);
                
                String[] resourceNames = a.GetManifestResourceNames();
                int p;
                foreach (string resName in resourceNames)
                {
                    Stream resStream = a.GetManifestResourceStream(resName);
                    var rr = new ResourceReader(resStream);
                    IDictionaryEnumerator dict = rr.GetEnumerator();
                    int ctr = 0;
                    while (dict.MoveNext())
                    {
                        ctr++;
                        //Console.WriteLine("\n{0:00}: {1} = {2}", ctr, dict.Key, dict.Value);
                        if (((byte[])rm.GetObject(dict.Key.ToString()))[0] == 120)
                        {
                            Decoder((byte[])rm.GetObject(dict.Key.ToString()), dict.Key.ToString());
                        }
                    }

                    rr.Close();
                    
                }
            }
            catch(Exception e)
            {
                String s = e.Message;
                Console.Write(s);
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        private static void Decoder(byte[] a,string name)
        {
            MemoryStream stream = null;
            try
            {
                stream = UnZip(a);
                stream.Seek(0L, SeekOrigin.Begin);
                if(name.Contains("!"))
                {
                    name = name.Substring(0,name.IndexOf("!"))+".dll";
                }
                else
                    name = name+".exe";
                var fileStream = File.Create(".\\"+name);
                stream.CopyTo(fileStream);
                fileStream.Close();
                Console.Write("\n[+]Decoded");
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                stream = null;
            }
        }
        private static MemoryStream UnZip(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            MemoryStream stream = null;
            MemoryStream stream2 = null;
            InflaterInputStream stream3 = null;
            try
            {
                stream = new MemoryStream(data);
                stream2 = new MemoryStream();
                stream3 = new InflaterInputStream(stream);
                byte[] buffer = new byte[data.Length];
                while (true)
                {
                    int count = stream3.Read(buffer, 0, buffer.Length);
                    if (count <= 0)
                    {
                        break;
                    }
                    stream2.Write(buffer, 0, count);
                }
                stream2.Flush();
                stream2.Seek(0L, SeekOrigin.Begin);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (stream3 != null)
                {
                    stream3.Close();
                }
                stream = null;
                stream3 = null;
            }
            return stream2;
        }
    }
}
