using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using static TorchSharp.torch;

using static AggregationExample.LEB128Codec;
using System.Diagnostics;
using TorchSharp;

namespace AggregationExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sd = new Dictionary<string, Tensor>();
            string location = "..\\..\\..\\Model\\model_0_epoch.dat";
            using (var stream = System.IO.File.OpenRead(location))
            using (var reader = new System.IO.BinaryReader(stream))
            {
                var streamEntries = reader.Decode();
                Console.WriteLine($"streamEntries = {streamEntries}");

                for (int i = 0; i < streamEntries; ++i)
                {
                    var key = reader.ReadString();
                    Console.WriteLine($"The key is {key}");
                    var dims = new long[] { 64, 3, 3, 3 };
                    var ten = torch.zeros(dims); 
                    sd.Add(key, ten);
                    sd[key].Load(reader);
                    Console.WriteLine($"The value is {sd[key]}");
                }
            }
            Console.WriteLine("Hello World!");
        }

    }
}
