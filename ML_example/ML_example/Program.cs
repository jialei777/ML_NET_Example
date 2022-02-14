using Microsoft.ML;
using System;
using System.Linq;

namespace ML_example
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Start a regression model...");

            var mlContext = new MLContext(seed: 0);

            var data = mlContext.Data.LoadFromTextFile<HousingData>("./housing.csv", hasHeader: true, separatorChar: ',');

            var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            var floatFeatures = split.TrainSet.Schema
                .Select(col => col.Name)
                .Where(colName => colName != "Label" && colName != "OceanProximity")
                .ToArray();


            var pipeline = mlContext.Transforms.Text.FeaturizeText("FeatureOceanProximity", "OceanProximity")
                .Append(mlContext.Transforms.Concatenate("floatFeatures", floatFeatures))
                .Append(mlContext.Transforms.Concatenate("Features", "floatFeatures", "FeatureOceanProximity"))
                .Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);

            var metrics = mlContext.Regression.Evaluate(predictions);

            Console.WriteLine($"R^2 - {metrics.RSquared}");

        }
    }
}
