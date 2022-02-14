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


            var pipeline1 = mlContext.Transforms.Text.FeaturizeText("FeatureOceanProximity", "OceanProximity")
                .Append(mlContext.Transforms.Concatenate("floatFeatures", floatFeatures))
                .Append(mlContext.Transforms.Concatenate("Features", "floatFeatures", "FeatureOceanProximity"))
                .Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());

            var pipeline2 = mlContext.Transforms.Text.FeaturizeText("FeatureOceanProximity", "OceanProximity")
                .Append(mlContext.Transforms.Concatenate("floatFeatures", floatFeatures))
                .Append(mlContext.Transforms.Concatenate("Features", "floatFeatures", "FeatureOceanProximity"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var modelPoisR = pipeline1.Fit(split.TrainSet);

            var modelTreeR = pipeline2.Fit(split.TrainSet);

            var predictionsPoisR = modelPoisR.Transform(split.TestSet);

            var predictionsTreeR = modelTreeR.Transform(split.TestSet);

            var metricsPoisR = mlContext.Regression.Evaluate(predictionsPoisR);
            var metricsTreeR = mlContext.Regression.Evaluate(predictionsTreeR);

            Console.WriteLine($"R^2 by Poisson Regression - {metricsPoisR.RSquared}");
            Console.WriteLine($"R^2 by Regression Tree - {metricsTreeR.RSquared}");

        }
    }
}
