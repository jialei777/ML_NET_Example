using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using TorchSharp;
using ResNetExample;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;

namespace ResNetExample
{
    class CIFAR10
    {
        private readonly static string _dataset = "CIFAR10";
        private readonly static string _dataLocation = Path.Join("..//..//..//..//Data", _dataset);

        private static int _epochs = 1;
        private static int _trainBatchSize = 8;

        private static int _testBatchSize = 16;

        private readonly static int _logInterval = 25;
        private readonly static int _numClasses = 10;

        private readonly static string _modelCheckpoint = "..//..//..//..//Model//model.dat";
        private readonly static bool _saveModel = false;

        private readonly static int _timeout = 3600;    // One hour by default.

        internal static void CIFAR10Classification(string[] args)
        {
            torch.random.manual_seed(1);

            var device = torch.cuda.is_available() ? torch.CUDA : torch.CPU;
            //  var device = torch.CPU;

            if (device.type == DeviceType.CUDA)
            {
                _trainBatchSize *= 8;
                _testBatchSize *= 8;
                _epochs *= 8;
            }

            var modelName = args.Length > 0 ? args[0] : "resnet18";
            var epochs = args.Length > 1 ? int.Parse(args[1]) : _epochs;
            var timeout = args.Length > 2 ? int.Parse(args[2]) : _timeout;

            Console.WriteLine();
            Console.WriteLine($"\tRunning {modelName} with {_dataset} on {device.type.ToString()} for {epochs} epochs, terminating after {TimeSpan.FromSeconds(timeout)}.");
            Console.WriteLine();

            var sourceDir = _dataLocation;
            var targetDir = Path.Combine(_dataLocation, "test_data");

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                Decompress.ExtractTGZ(Path.Combine(sourceDir, "cifar-10-binary.tar.gz"), targetDir);
            }

            Console.WriteLine($"\tCreating the model...");

            Module model = null;

            switch (modelName.ToLower())
            {
                /*
                case "alexnet":
                    model = new AlexNet(modelName, _numClasses, device);
                    break;
                case "mobilenet":
                    model = new MobileNet(modelName, _numClasses, device);
                    break;
                case "vgg11":
                case "vgg13":
                case "vgg16":
                case "vgg19":
                    model = new VGG(modelName, _numClasses, device);
                    break;
                */
                case "resnet18":
                    model = ResNet.ResNet18(_numClasses, _modelCheckpoint, device);
                    break;
                case "resnet34":
                    _testBatchSize /= 4;
                    model = ResNet.ResNet34(_numClasses, _modelCheckpoint, device);
                    break;
                case "resnet50":
                    _trainBatchSize /= 6;
                    _testBatchSize /= 8;
                    model = ResNet.ResNet50(_numClasses, _modelCheckpoint, device);
                    break;
            }


            var hflip = TorchSharp.torchvision.transforms.HorizontalFlip();
            var gray = TorchSharp.torchvision.transforms.Grayscale(3);
            var rotate = TorchSharp.torchvision.transforms.Rotate(90);
            var contrast = TorchSharp.torchvision.transforms.AdjustContrast(1.25);


            Console.WriteLine($"\tPreparing training and test data...");
            Console.WriteLine();

            using (var train = new CIFARReader(targetDir, false, _trainBatchSize, shuffle: true, device: device, transforms: new TorchSharp.torchvision.ITransform[] { }))
            using (var test = new CIFARReader(targetDir, true, _testBatchSize, device: device))
            using (var optimizer = torch.optim.Adam(model.parameters(), 0.001))
            {

                Stopwatch totalSW = new Stopwatch();
                totalSW.Start();

                for (var epoch = 1; epoch <= epochs; epoch++)
                {

                    Stopwatch epchSW = new Stopwatch();
                    epchSW.Start();

                    Train(model, optimizer, nll_loss(), train.Data(), epoch, _trainBatchSize, train.Size);
                    Test(model, nll_loss(), test.Data(), test.Size);
                    GC.Collect();

                    epchSW.Stop();
                    Console.WriteLine($"Elapsed time for this epoch: {epchSW.Elapsed.TotalSeconds} s.");

                    if (totalSW.Elapsed.TotalSeconds > timeout) break;
                }

                totalSW.Stop();
                Console.WriteLine($"Elapsed training time: {totalSW.Elapsed} s.");
            }


            if (_saveModel)
            {
                model.save(_modelCheckpoint);
                Console.WriteLine($"\tSaving model checkpoint to {_modelCheckpoint}");
            }
            model.Dispose();


        }

        private static void Train(
            Module model,
            torch.optim.Optimizer optimizer,
            Loss loss,
            IEnumerable<(Tensor, Tensor)> dataLoader,
            int epoch,
            long batchSize,
            long size)
        {
            model.train();

            int batchId = 1;
            long total = 0;
            long correct = 0;

            Console.WriteLine($"Epoch: {epoch}...");

            foreach (var (data, target) in dataLoader)
            {

                optimizer.zero_grad();

                using var prediction = model.forward(data);
                using var lsm = log_softmax(prediction, 1);
                using (var output = loss(lsm, target))
                {

                    output.backward();

                    optimizer.step();

                    total += target.shape[0];

                    using (var predicted = prediction.argmax(1))
                    using (var eq = predicted.eq(target))
                    using (var sum = eq.sum())
                    {
                        correct += sum.ToInt64();
                    }

                    if (batchId % _logInterval == 0)
                    {
                        var count = Math.Min(batchId * batchSize, size);
                        Console.WriteLine($"\rTrain: epoch {epoch} [{count} / {size}] Loss: {output.ToSingle().ToString("0.000000")} | Accuracy: { ((float)correct / total).ToString("0.000000") }");
                    }

                    batchId++;
                }

                GC.Collect();
            }
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