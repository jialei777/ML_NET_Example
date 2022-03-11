namespace azureml_csharp.Utils
{
    public class Workspace
    {
        public string Name { get; set; }

        public WorkspaceProperties Properties { get; set; }
    }

    public class WorkspaceProperties
    {
        public string MlFlowTrackingUri { get; set; }
    }
}
