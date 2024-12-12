using ConsoleApp1;
using MessagePack;
using ProtoBuf;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var i = 1024 * 1024 * 1024;

            Console.WriteLine($"{i}");
            Console.WriteLine($"{Int32.MaxValue}");

            var a = new ClassA(42);
            a.SetValue(42);

            var r1 = TestAllSerializers(a, "1");

            var c = new ClassC();
            c.classA = a;

            var r2 = TestAllSerializers(c, "2");


            var b = new ClassB();
            b.SetValue(42 - 5);

            var r3 = TestAllSerializers(b, "3");

           
            a.SetC(c); // circular reference
            //var r4 = TestAllSerializers(a, "4");
            var bfA =  TestBinaryFormatter(a, "4");
            var dcA = TestDataContractSerializer(a, "4");
            
            //var mpA = TestMessagePack(a, "4");
            //var pbA = TestProtoBuf(a, "4");
        }

        private static List<(string ser, T? result)> TestAllSerializers<T>(T t, string fileID)
        {
            var results = new List<(string ser, T? result)>();

            var daDc = TestDataContractSerializer<T>(t, $"ser{fileID}.xml");
            AddTestResult(t, daDc, "dc", results);

            var daBf = TestBinaryFormatter(t, $"binary{fileID}.dat");
            AddTestResult(t, daBf, "bf", results);


            var daMp = TestMessagePack(t, $"messagePack{fileID}.dat");
            AddTestResult(t, daMp,"mp", results);

            var daPb = TestProtoBuf(t, $"protobuf{fileID}.dat");
            AddTestResult(t, daPb, "pb", results);

            return results;
        }

        private static void AddTestResult<T>(T a, T daDc, string testName, List<(string ser, T? result)> results)
        {
            if (daDc == null || daDc.Equals(a) != true)
            {
                results.Add(($"{testName}-failed", daDc));
            }
            else
            {
                results.Add(($"{testName}-correct", daDc));
            }
        }

        private static T? TestDataContractSerializer<T>(T t, string fileName)
        {
            using (MemoryStream stream1 = new MemoryStream())
            {
                // ser
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(stream1, t);

                stream1.Position = 0;
                var fs = new FileStream(fileName, FileMode.Create);
                stream1.WriteTo(fs);
                fs.Close();

                // deser
                stream1.Position = 0;

                fs = new FileStream(fileName, FileMode.Open);
                var da = (T?)serializer.ReadObject(fs);
                fs.Close();

                return da;
            }
        }

        private static T TestBinaryFormatter<T>(T t, string fileName)
        {
            if(t == null)
            {
                throw new ArgumentNullException($"Parameter {nameof(t)} must not be null");
            }

            using (MemoryStream stream1 = new MemoryStream())
            {
                // ser
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                BinaryFormatter serializer = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                serializer.Serialize(stream1, t);

                stream1.Position = 0;
                var fs = new FileStream(fileName, FileMode.Create);
                stream1.WriteTo(fs);
                fs.Close();

                // deser
                fs = new FileStream(fileName, FileMode.Open);
                var dt = (T)serializer.Deserialize(fs);
                fs.Close();

                return dt;
            }
        }

        private static T TestMessagePack<T>(T t, string fileName)
        {
            MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options;
            using (MemoryStream stream1 = new MemoryStream())
            {
                // ser
                MessagePackSerializer.Serialize<T>(stream1, t);

                stream1.Position = 0;
                var fs = new FileStream(fileName, FileMode.Create);
                stream1.WriteTo(fs);
                fs.Close();

                // deser
                fs = new FileStream(fileName, FileMode.Open);
                var dt = MessagePackSerializer.Deserialize<T>(fs);
                fs.Close();

                return dt;
            }
        }

        private static T TestProtoBuf<T>(T t, string fileName)
        {
            using (MemoryStream stream1 = new MemoryStream())
            {
                // ser
                Serializer.Serialize<T>(stream1, t);
                
                stream1.Position = 0;
                var fs = new FileStream(fileName, FileMode.Create);
                stream1.WriteTo(fs);
                fs.Close();

                // deser
                fs = new FileStream(fileName, FileMode.Open);
                var dt = Serializer.Deserialize<T>(fs);
                fs.Close();

                return dt;
            }
        }
    }
}