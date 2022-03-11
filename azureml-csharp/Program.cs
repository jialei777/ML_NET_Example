using Azure.Core;
using Azure.Identity;
using azureml_csharp.Utils;
using System.Net.Http.Headers;
using System.Net.Http.Json;


// Get credential and token

var cred = new DefaultAzureCredential();
var token = await cred.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
Console.WriteLine($"Got token of size: {token.Token.Length}");



// Get workspace uri

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
var subscriptionID = "48bbc269-ce89-4f6f-9a12-c6f91fcb772d";
var region = "westus2";
var resourceGroupName = "aml1p-rg";
var workspaceName = "aml1p-ml-wus2";
var apiVersion = "2021-03-01-preview";

// var workspaceUri = $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName}/providers/Microsoft.MachineLearningServices/workspaces/{workspaceName}?api-version={apiVersion}";
// var workspace = await httpClient.GetFromJsonAsync<Workspace>(workspaceUri);
// Console.WriteLine($"We got a workspace! It's name is {workspace.Name}, mlflow {workspace.Properties.MlFlowTrackingUri}");
// string uri = workspace.Properties.MlFlowTrackingUri;
// string workspaceUri = $"https://{uri.Split("://")[1]}";
var workspaceUri = $"https://{region}.api.azureml.ms/mlflow/v1.0/subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName}/providers/Microsoft.MachineLearningServices/workspaces/{workspaceName}";
Console.WriteLine($"Connecting to workspace: {workspaceName}, resourceGroup: {resourceGroupName}\n");



// Create a new experiment

var experimentName = "Log-metric-from-csharp-1";
Console.WriteLine($"Creating a new experiment: {experimentName} ...");

var mlflowCreateExperimentUri = $"{workspaceUri}/api/2.0/mlflow/experiments/create";
var resultExperiment = await httpClient.PostAsJsonAsync(mlflowCreateExperimentUri, new
{
    Name = experimentName
});

var experimentStatus = resultExperiment.IsSuccessStatusCode ? "Successful" : "Failed";
var experimentResponse = await resultExperiment.Content.ReadFromJsonAsync<Experiment>();
var experimentId = experimentResponse.experiment_id;

Console.WriteLine($"\t... {experimentStatus}! Experiment id: {experimentId} \n");

// Create a new run under the experiment

Console.WriteLine($"Creating a new run under experiment {experimentName} ...");

var mlflowCreateRunUri = $"{workspaceUri}/api/2.0/mlflow/runs/create";
var resultCreateRun = await httpClient.PostAsJsonAsync(mlflowCreateRunUri, new
{
    experiment_id = experimentId,
    start_time = (Int64)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds * 1000
});

var runCreateStatus = resultCreateRun.IsSuccessStatusCode ? "Successful" : "Failed";

// var returnValueRun1 = resultRun.Content.ReadAsStringAsync();
// Console.WriteLine($"Run Response String: {returnValueRun1.Result}");

var runResponse = await resultCreateRun.Content.ReadFromJsonAsync<RunResult>();
var runId = runResponse.run.info.run_id;

Console.WriteLine($"\t... {runCreateStatus}! Run id: {runId} \n");



// Log metric

var metricName = "my-cool-metric";
Console.WriteLine($"Log metric {metricName} agaist the run: {runId} ...");
var mlflowLogMetricUri = $"{workspaceUri}/api/2.0/mlflow/runs/log-metric";

Random rnd = new Random();
int totalMetricCount = rnd.Next(15, 20);
double metricSlope = rnd.Next(50, 150)/100.0;

for (int i = 0; i < totalMetricCount; i++)
{
    double metricValue = metricSlope * i + rnd.Next(1000)/500.0;
    var resultLog = await httpClient.PostAsJsonAsync(mlflowLogMetricUri, new
    {
        run_id = runId,
        key = metricName,
        value = metricValue,
        timestamp = (Int64)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds * 1000
    });
    var logStatus = resultLog.IsSuccessStatusCode ? "Successful" : "Failed";
    Console.WriteLine($"\t... # {i} / {totalMetricCount} with value {metricValue}: {logStatus}!");
}


// Stop (or complete) the run

Console.WriteLine($"\nFinishing run {runId} ...");
var mlflowStopRunUri = $"{workspaceUri}/api/2.0/mlflow/runs/update";
var resultStopRun = await httpClient.PostAsJsonAsync(mlflowStopRunUri, new
{
    run_id = runId,
    status = "FINISHED",
    end_time = (Int64)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds * 1000
});
var runStopStatus = resultCreateRun.IsSuccessStatusCode ? "Successful" : "Failed";

Console.WriteLine($"\t... {runStopStatus}!\n");


Console.WriteLine($"Link to the experiment:");
Console.WriteLine($"https://ml.azure.com/experiments/id/{experimentId}?wsid=/subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName}/providers/Microsoft.MachineLearningServices/workspaces/{workspaceName}\n");
