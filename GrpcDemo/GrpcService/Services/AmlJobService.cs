using Grpc.Core;
using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;


namespace GrpcService.Services
{
    public class AmlJobService : AmlJob.AmlJobBase
    {
        private readonly ILogger<AmlJobService> _logger;

        public AmlJobService(ILogger<AmlJobService> logger)
        {
            _logger = logger;
        }

        public override async Task<SubmissionStatus> SubmitAmlJob(AmlJobDetails request, ServerCallContext context)
        {
            SubmissionStatus status = new SubmissionStatus();


            //const string RestEndpoint = "https://westus2.api.azureml.ms/pipelines/v1.0/subscriptions/48bbc269-ce89-4f6f-9a12-c6f91fcb772d/resourceGroups/aml1p-rg/providers/Microsoft.MachineLearningServices/workspaces/aml1p-ml-wus2/PipelineRuns/PipelineEndpointSubmit/Id/f4240264-c272-4732-9483-02458f1043f7";
            
            var RestEndpoint = request.PipelineEndpoint;
            var cred = new DefaultAzureCredential();

            var token = await cred.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
            Console.WriteLine($"Got token of size: {token.Token.Length}");


            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);


                var submitPipelineRunRequest = new SubmitPipelineRunRequest()
                {
                    ExperimentName = "FL-endpoint-from-csharp-3",
                    DisplayName = "model3",
                    Description = "Asynchronous C# REST api call",
                    ParameterAssignments = new Dictionary<string, string>
                    {
                        {
                            // Replace with your pipeline parameter keys and values
                            "model_url", "https://storagefakedata.blob.core.windows.net/testdatacontainer/model3.csv"
                        }
                    }
                };


                // submit the job
                var requestPayload = JsonConvert.SerializeObject(submitPipelineRunRequest);
                var httpContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                var submitResponse = await client.PostAsync(RestEndpoint, httpContent).ConfigureAwait(false);

                status.StatusCode = (int)submitResponse.StatusCode;


                var result = await submitResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                var obj = JObject.Parse(result);
                Console.WriteLine("resulting json file");
                Console.WriteLine(obj);
                status.AmlUrl = (string)obj.GetValue("RunUrl");
                status.RunId = (string)obj.GetValue("PipelineRunId");
               
            }

            return status;
        }

        public override async Task<RunStatus> CheckAmlJobStatus(AmlRun request, ServerCallContext context)
        {
            var runStatus = new RunStatus();
            var runId = request.RunId;
            var cred = new DefaultAzureCredential();

            var token = await cred.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
            Console.WriteLine($"Got token of size: {token.Token.Length}");

            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                var subscriptionID = "48bbc269-ce89-4f6f-9a12-c6f91fcb772d";
                var region = "westus2";
                var resourceGroupName = "aml1p-rg";
                var workspaceName = "aml1p-ml-wus2";

                var mlFlowBaseUri = $"https://{region}.api.azureml.ms/mlflow/v1.0/subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName}/providers/Microsoft.MachineLearningServices/workspaces/{workspaceName}";

                var mlFlowUri = $"{mlFlowBaseUri}/api/2.0/mlflow/runs/get?run_id={(string)runId}";

                for (int i = 0; i < 1; i++)
                {
                    // var requestStatus = await client.PostAsync(mlFlowUri, httpContentStatus).ConfigureAwait(false);

                    Thread.Sleep(2000);
                    var requestStatus = await client.GetAsync(mlFlowUri).ConfigureAwait(false);

                    Console.WriteLine(requestStatus.StatusCode);
                    var resultStaus = await requestStatus.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JObject runInfo = JObject.Parse(resultStaus);
                    runStatus.ResultJson = (string)requestStatus.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(runStatus.ResultJson);
                    // Console.WriteLine(runInfo.GetValue("info"));
                }
            }
            return runStatus;
        }
    }
}


[DataContract]
public class SubmitPipelineRunRequest
{
    [DataMember]
    public string ExperimentName { get; set; }

    [DataMember]
    public string DisplayName { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember(IsRequired = false)]
    public IDictionary<string, string> ParameterAssignments { get; set; }
}

[DataContract]
public class StatusRequest
{
    [DataMember]
    public string RunId { get; set; }
}