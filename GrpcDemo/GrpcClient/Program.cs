// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using GrpcService;


var channel = GrpcChannel.ForAddress("http://localhost:5251");

var input = new HelloRequest { Name = "Jialei" };
var client1 = new Greeter.GreeterClient(channel);
var reply = await client1.SayHelloAsync(input);
Console.WriteLine(reply.Message);

Console.WriteLine("\n>>> submit an AML job...");

var client = new AmlJob.AmlJobClient(channel);

var jobDetail = new AmlJobDetails
{
    PipelineEndpoint = "https://westus2.api.azureml.ms/pipelines/v1.0/subscriptions/48bbc269-ce89-4f6f-9a12-c6f91fcb772d/resourceGroups/aml1p-rg/providers/Microsoft.MachineLearningServices/workspaces/aml1p-ml-wus2/PipelineRuns/PipelineEndpointSubmit/Id/f4240264-c272-4732-9483-02458f1043f7"
};

var status = await client.SubmitAmlJobAsync(jobDetail);

Console.WriteLine($"Submission status code is {status.StatusCode}");
Console.WriteLine(status.AmlUrl);

Thread.Sleep(1000);

Console.WriteLine("\n>>> Now check its status ...");


var amlRun = new AmlRun
{
    RunId = status.RunId
};

var runStatus = await client.CheckAmlJobStatusAsync(amlRun);

Console.WriteLine($"Run status is");
Console.WriteLine(runStatus.ResultJson);



