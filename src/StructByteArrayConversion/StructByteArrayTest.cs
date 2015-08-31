using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace genericgamedev_tests.StructByteArrayConversion
{
    static class StructByteArrayTest
    {
        private const int count = 100000;

        public static void Test()
        {
            Console.WriteLine("testing correctness: ");
            var s = Struct16Bytes.Filled();
            var array = s.ToArrayWithBinaryWriter();
            var s2 = default(Struct16Bytes);
            s2.FromArrayWithBinaryReader(array);
            var array2 = StructToArray.MarshalInterop(s);
            var s3 = ArrayToStruct.MarshalInterop<Struct16Bytes>(array2);
            var array3 = StructToArray.BinaryFormatter(s);
            var s4 = ArrayToStruct.BinaryFormatter<Struct16Bytes>(array3);
            Console.WriteLine("base:       " + s);
            Console.ForegroundColor = s.Equals(s2) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("bin writer: " + s2 + " array bytes: " + array.Length);
            Console.ForegroundColor = s.Equals(s3) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("marshal:    " + s3 + " array bytes: " + array2.Length);
            Console.ForegroundColor = s.Equals(s4) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("bin format: " + s4 + " array bytes: " + array3.Length);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("any key to continue..");
            Console.ReadKey();

            Console.WriteLine("preparing tests..");

            var tests = new List<Tuple<string, Action>>()
            {
                new Tuple<string, Action>("s->a BinaryWriter", StructToArray.BinaryWriter<Struct16Bytes>),
                new Tuple<string, Action>("s->a Marshal", StructToArray.MarshalInterop<Struct16Bytes>),
                new Tuple<string, Action>("s->a BinaryFormatter", StructToArray.BinaryFormatter<Struct16Bytes>),

                new Tuple<string, Action>("a->s BinaryReader", ArrayToStruct.BinaryReader<Struct16Bytes>),
                new Tuple<string, Action>("a->s Marshal", ArrayToStruct.MarshalInterop<Struct16Bytes>),
                new Tuple<string, Action>("a->s BinaryFormatter", ArrayToStruct.BinaryFormatter<Struct16Bytes>),
            };

            foreach (var tuple in tests)
            {
                tuple.Item2();
            }

            var lists = tests.Select(t => new Tuple<string, List<TimeSpan>>(t.Item1, new List<TimeSpan>())).ToList();

            Console.Write("running real tests..");

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < tests.Count; j++)
                {
                    lists[j].Item2.Add(TestHelper.Measure(tests[j].Item2));

                    TestHelper.PrintTimes(lists);

                    GC.Collect(10, GCCollectionMode.Forced, true);
                }
            }

            Console.Write("done.");

            while (Console.KeyAvailable)
            {
                Console.ReadKey(false);
            }

            Console.ReadLine();
        }

        private static class StructToArray
        {
            public static void BinaryWriter<T>()
                where T : struct, ISerialisable
            {
                var s = default(T);

                var output = new byte[count][];

                for (int i = 0; i < count; i++)
                {
                    var array = s.ToArrayWithBinaryWriter();
                    output[i] = array;
                }
                Console.WriteLine(output.Length);
            }
            
            public static void MarshalInterop<T>()
                where T : struct
            {
                var s = default(T);

                var output = new byte[count][];

                for (int i = 0; i < count; i++)
                {
                    var array = MarshalInterop(s);
                    output[i] = array;
                }
                Console.WriteLine(output.Length);
            }

            public static byte[] MarshalInterop<T>(T s)
                where T : struct
            {
                var size = Marshal.SizeOf(typeof(T));
                var array = new byte[size];
                var ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(s, ptr, true);
                Marshal.Copy(ptr, array, 0, size);
                Marshal.FreeHGlobal(ptr);
                return array;
            }

            public static void BinaryFormatter<T>()
                where T : struct
            {
                var s = default(T);

                var output = new byte[count][];

                for (int i = 0; i < count; i++)
                {
                    output[i] = BinaryFormatter(s);
                }
                Console.WriteLine(output.Length);
            }

            public static byte[] BinaryFormatter<T>(T s)
                where T : struct
            {
                var formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, s);
                return stream.ToArray();
            }
        }

        private static class ArrayToStruct
        {
            public static void BinaryReader<T>()
                where T : struct, ISerialisable
            {
                var bytes = Marshal.SizeOf(typeof (T));

                var array = new byte[bytes];

                var output = new T[count];

                for (int i = 0; i < count; i++)
                {
                    var s = default(T);
                    s.FromArrayWithBinaryReader(array);
                    output[i] = s;
                }
                Console.WriteLine(output.Length);
            }

            public static void MarshalInterop<T>()
                where T : struct
            {
                var bytes = Marshal.SizeOf(typeof(T));

                var array = new byte[bytes];

                var output = new T[count];

                for (int i = 0; i < count; i++)
                {
                    var s = MarshalInterop<T>(array);
                    output[i] = s;
                }
                Console.WriteLine(output.Length);
            }

            public static T MarshalInterop<T>(byte[] array)
                where T : struct
            {
                var size = Marshal.SizeOf(typeof(T));
                var ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(array, 0, ptr, size);
                var s = (T)Marshal.PtrToStructure(ptr, typeof(T));
                Marshal.FreeHGlobal(ptr);
                return s;
            }

            public static void BinaryFormatter<T>()
                where T : struct
            {
                var array = StructToArray.BinaryFormatter(default(T));

                var output = new T[count];

                for (int i = 0; i < count; i++)
                {
                    output[i] = BinaryFormatter<T>(array);
                }
                Console.WriteLine(output.Length);
            }

            public static T BinaryFormatter<T>(byte[] array)
                where T : struct
            {
                var stream = new MemoryStream(array);
                var formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }


        }
    }
}