using Microsoft.ML.Data;

namespace ObjectDetect.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
