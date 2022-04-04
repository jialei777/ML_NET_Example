using System;
using System.IO;
using FlatBuffers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;

namespace AggregationExample
{
    internal class Program
    {
        private readonly static string _dataset = "CIFAR10";
        private readonly static string _dataLocation = Path.Join("..//..//..//Data", _dataset);

        private readonly static int _numClasses = 10;
        private readonly static string location0 = "..//..//..//Model//model_8_epoch.dat";
        private readonly static string locationNew = "..//..//..//Model//model_new.dat";
        private static int _testBatchSize = 16;

        private static double _learningRate = 0.001;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            var config = builder.Build();

            // Read input gradient
            byte[] data = File.ReadAllBytes(config["input_gradients_path"]);
            var bytebuffer = new ByteBuffer(data);
            var gradient = Gradient.Gradient.GetRootAsGradient(bytebuffer);

            var gradFromFile = gradient.GetGradientsArray();

            // Assuming this is my gradient array
            /*
            float[] gradFromFile = new float[11173962];
            for (int i = 0; i < gradFromFile.Length; i++)
            {
                gradFromFile[i] = 0.2f;
            }
            */

            var gradFromFileList = gradFromFile.ToList();
            Console.WriteLine(gradFromFileList[0]);

            // now load the model
            var model = ResNet.ResNet18(_numClasses, location0);

            // var sd = new Dictionary<string, Tensor>();
            var model_sd = model.state_dict();
            model.Dispose();

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
                pre_index += total_len;

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


            // model evaluation
            var model_new = ResNet.ResNet18(_numClasses, locationNew);
            torch.random.manual_seed(1);
            var device = torch.cuda.is_available() ? torch.CUDA : torch.CPU;
            var sourceDir = _dataLocation;
            var targetDir = Path.Combine(_dataLocation, "test_data");

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                Decompress.ExtractTGZ(Path.Combine(sourceDir, "cifar-10-binary.tar.gz"), targetDir);
            }

            using (var test = new CIFARReader(targetDir, true, _testBatchSize, device: device))
                Test(model_new, nll_loss(), test.Data(), test.Size);


        }



        private static void Test(
            Module model,
            Loss loss,
            IEnumerable<(Tensor, Tensor)> dataLoader,
            long size)
        {
            model.eval();

            double testLoss = 0;
            long correct = 0;
            int batchCount = 0;

            foreach (var (data, target) in dataLoader)
            {

                using var prediction = model.forward(data);
                using var lsm = log_softmax(prediction, 1);
                using (var output = loss(lsm, target))
                {

                    testLoss += output.ToSingle();
                    batchCount += 1;

                    using (var predicted = prediction.argmax(1))
                    using (var eq = predicted.eq(target))
                    using (var sum = eq.sum())
                    {
                        correct += sum.ToInt64();
                    }
                }

                GC.Collect();
            }

            Console.WriteLine($"\rTest set: Average loss {(testLoss / batchCount).ToString("0.0000")} | Accuracy {((float)correct / size).ToString("0.0000")}");
        }

    }

}
