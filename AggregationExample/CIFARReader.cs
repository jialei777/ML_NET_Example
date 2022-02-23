using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TorchSharp;
using static TorchSharp.torch;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;



namespace AggregationExample
{
    public sealed class CIFARReader : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the folder containing the image files.</param>
        /// <param name="test">True if this is a test set, otherwise false.</param>
        /// <param name="batch_size">The batch size</param>
        /// <param name="shuffle">Randomly shuffle the images.</param>
        /// <param name="device">The device, i.e. CPU or GPU to place the output tensors on.</param>
        /// <param name="transforms">A list of image transformations, helpful for data augmentation.</param>
        public CIFARReader(string path, bool test, int batch_size = 32, bool shuffle = false, Device device = null, IList<TorchSharp.torchvision.ITransform> transforms = null)
        {
            _transforms = transforms == null ? new List<TorchSharp.torchvision.ITransform>() : transforms;

            // The MNIST data set is small enough to fit in memory, so let's load it there.

            var dataPath = Path.Combine(path, "cifar-10-batches-bin");

            if (test)
            {
                _size = ReadSingleFile(Path.Combine(dataPath, "test_batch.bin"), batch_size, shuffle, device);
            }
            else
            {
                _size += ReadSingleFile(Path.Combine(dataPath, "data_batch_1.bin"), batch_size, shuffle, device);
                _size += ReadSingleFile(Path.Combine(dataPath, "data_batch_2.bin"), batch_size, shuffle, device);
                _size += ReadSingleFile(Path.Combine(dataPath, "data_batch_3.bin"), batch_size, shuffle, device);
                _size += ReadSingleFile(Path.Combine(dataPath, "data_batch_4.bin"), batch_size, shuffle, device);
                _size += ReadSingleFile(Path.Combine(dataPath, "data_batch_5.bin"), batch_size, shuffle, device);
            }
        }

        private int ReadSingleFile(string path, int batch_size, bool shuffle, Device device)
        {
            const int height = 32;
            const int width = 32;
            const int channels = 3;
            const int count = 10000;

            byte[] dataBytes = File.ReadAllBytes(path);

            if (dataBytes.Length != (1 + channels * height * width) * count)
                throw new InvalidDataException($"Not a proper CIFAR10 file: {path}");

            // Set up the indices array.
            Random rnd = new Random();
            var indices = !shuffle ?
                Enumerable.Range(0, count).ToArray() :
                Enumerable.Range(0, count).OrderBy(c => rnd.Next()).ToArray();

            var imgSize = channels * height * width;

            // Go through the data and create tensors
            for (var i = 0; i < count;)
            {

                var take = Math.Min(batch_size, Math.Max(0, count - i));

                if (take < 1) break;

                var dataTensor = torch.zeros(new long[] { take, imgSize }, device: device);
                var lablTensor = torch.zeros(new long[] { take }, torch.int64, device: device);

                // Take
                for (var j = 0; j < take; j++)
                {
                    var idx = indices[i++];
                    var lblStart = idx * (1 + imgSize);
                    var imgStart = lblStart + 1;

                    lablTensor[j] = torch.tensor(dataBytes[lblStart], torch.int64);

                    var floats = dataBytes[imgStart..(imgStart + imgSize)].Select(b => (float)b).ToArray();
                    using (var inputTensor = torch.tensor(floats))
                        dataTensor.index_put_(inputTensor, TensorIndex.Single(j));
                }

                data.Add(dataTensor.reshape(take, channels, height, width));
                dataTensor.Dispose();
                labels.Add(lablTensor);
            }

            return count;
        }

        public int Size
        {
            get
            {
                return _size * (_transforms.Count + 1);
            }
        }
        private int _size = 0;

        private List<Tensor> data = new List<Tensor>();
        private List<Tensor> labels = new List<Tensor>();

        private IList<TorchSharp.torchvision.ITransform> _transforms;

        public IEnumerable<(Tensor, Tensor)> Data()
        {
            for (var i = 0; i < data.Count; i++)
            {
                yield return (data[i], labels[i]);

                foreach (var tfrm in _transforms)
                {
                    yield return (tfrm.forward(data[i]), labels[i]);
                }
            }
        }

        public void Dispose()
        {
            data.ForEach(d => d.Dispose());
            labels.ForEach(d => d.Dispose());
        }
    }
    public static class Decompress
    {
        public static void DecompressGZipFile(string gzipFileName, string targetDir)
        {
            byte[] buf = new byte[4096];

            using (var fs = File.OpenRead(gzipFileName))
            using (var gzipStream = new GZipInputStream(fs))
            {

                string fnOut = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(gzipFileName));

                using (var fsOut = File.Create(fnOut))
                {
                    StreamUtils.Copy(gzipStream, fsOut, buf);
                }
            }
        }
        public static void ExtractTGZ(string gzArchiveName, string destFolder)
        {
            var flag = gzArchiveName.Split(Path.DirectorySeparatorChar).Last().Split('.').First() + ".bin";
            if (File.Exists(Path.Combine(destFolder, flag))) return;

            Console.WriteLine($"Extracting.");
            var task = Task.Run(() => {
                using (var inStream = File.OpenRead(gzArchiveName))
                {
                    using (var gzipStream = new GZipInputStream(inStream))
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
#pragma warning restore CS0618 // Type or member is obsolete
                            tarArchive.ExtractContents(destFolder);
                    }
                }
            });

            while (!task.IsCompleted)
            {
                Thread.Sleep(200);
                Console.Write(".");
            }

            File.Create(Path.Combine(destFolder, flag));
            Console.WriteLine("");
            Console.WriteLine("Extraction completed.");
        }

    }
}
