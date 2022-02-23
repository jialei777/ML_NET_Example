using System;
using System.Collections.Generic;
using System.Linq;
using static TorchSharp.torch;
using TorchSharp;


namespace AggregationExample
{
    internal class Program
    {
        private readonly static int _numClasses = 10;
        private readonly static string location0 = "..//..//..//Model//model_8_epoch.dat";
        private readonly static string locationNew = "..//..//..//Model//model_new.dat";

        private static double _learningRate = 0.001;

        static void Main(string[] args)
        {

            // Assuming this is my gradient array
            float[] gradFromFile = new float[11173962];
            for (int i = 0; i < gradFromFile.Length; i++)
            {
                gradFromFile[i] = 0.2f;
            }


            var gradFromFileList = gradFromFile.ToList();
            Console.WriteLine(gradFromFileList[0]);

            // now load the model
            var model = ResNet.ResNet18(_numClasses, location0);
            
            // var sd = new Dictionary<string, Tensor>();
            var model_sd = model.state_dict();
            long pre_index = 0;

            foreach (var kv_pair in model_sd)
            {
                var layer_name = kv_pair.Key;
                var ten_old = model_sd[layer_name].cpu().detach();

                long total_len = 1;
                foreach (int dim in ten_old.shape)
                    total_len *= dim;

                var gradList = new List<float>();
                gradList.AddRange(gradFromFileList.Where((fv, i) => i >= pre_index && i < pre_index + total_len));
                pre_index = pre_index + total_len;

                model_sd[layer_name] -= tensor(gradList).reshape(ten_old.shape) * _learningRate;
            }

            /*
            using (var stream = System.IO.File.OpenRead(location0))
            using (var reader = new System.IO.BinaryReader(stream))
            {
                var streamEntries = reader.Decode();
                Console.WriteLine($"streamEntries = {streamEntries}");

                for (int i = 0; i < streamEntries; ++i)
                {
                    var key = reader.ReadString();
                    Console.WriteLine($"The key is {key}");
                    sd[key].Load(reader);
                    var ten = sd[key].clone();

                    long total_len = 1;
                    foreach (int dim in ten.shape)
                    {
                        total_len *= dim;
                    }
                    var subList = new List<float>();
                    subList.AddRange(gradFromFileList.Where((f, i) => i >= pre_index && i < pre_index+total_len));
                    pre_index = pre_index + total_len;
                    // Console.WriteLine($"---> sd[key] {sd[key]}");
                    // Console.WriteLine($"---> reshaped grad {tensor(subList).reshape(ten.shape)}");
                    // Console.WriteLine($"\tBefore {sd[key][0, 0, 0, 0].item<float>()}");

                    sd[key] = sd[key] - tensor(subList).reshape(ten.shape);

                    // Console.WriteLine($"\tAfter {sd[key][0, 0, 0, 0].item<float>()}");
                    // break;
                }
            }
            */

            // save the model.state_dict() 
            Console.WriteLine($"save model.state_dict() to {locationNew}");
            using (var stream = System.IO.File.OpenWrite(locationNew))
            using (var writer = new System.IO.BinaryWriter(stream))
            {
                writer.Encode(model_sd.Count); // 4 bytes

                foreach (var (key, value) in model_sd)
                {
                    writer.Write(key);
                    value.Save(writer);
                    // Console.WriteLine($"\tFirst value {v[0, 0, 0, 0].item<float>()}");
                }
            }

            Console.WriteLine("Hello World!");
        }

    }
}
