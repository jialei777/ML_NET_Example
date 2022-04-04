namespace azureml_csharp.Utils
{
    public class RunResult
    {
        public Run run { get; set; }
    }
    public class Run
    {
        public RunInfo info { get; set; }
    }

    public class RunInfo
    {
        public string run_id { get; set; }
        public string status { get; set; }
    }

}
