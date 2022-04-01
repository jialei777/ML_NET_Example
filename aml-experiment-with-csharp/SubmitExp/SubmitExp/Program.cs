// See https://aka.ms/new-console-template for more information
using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using Newtonsoft.Json;

// ... in its own class and method ... 


var codeName = $"code-name-{RandomUtil.GetRandomString()}";
var codeVersion = 1;
var jobName = $"unique-name-{RandomUtil.GetRandomString()}";
var clusterName = "cpucluster-jialei";
var experimentName = "submit-job-via-csharp-3";


var cred = new DefaultAzureCredential();
var token = await cred.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
Console.WriteLine($"Got token of size: {token.Token.Length}");

var subscriptionID = "48bbc269-ce89-4f6f-9a12-c6f91fcb772d";
var region = "westus2";
var resourceGroupName = "aml1p-rg";
var workspaceName = "aml1p-ml-wus2";
var apiVersion = "api-version=2021-03-01-preview";

var amlDetials = $"/subscriptions/{subscriptionID}/resourceGroups/{resourceGroupName}/providers/Microsoft.MachineLearningServices/workspaces/{workspaceName}";
var baseAddress = $"https://management.azure.com{amlDetials}";
var codeEndpoint = $"{baseAddress}/codes/{codeName}/versions/{codeVersion}?{apiVersion}";
var expEndpoint = $"{baseAddress}/jobs/{jobName}?{apiVersion}";


Console.WriteLine("-------------------------------------------------------------------------------------");
Console.WriteLine("-------------------------------------------------------------------------------------");

using (HttpClient client = new HttpClient())
{

    var registerCodeRequest = new RegisterCodeRequest()
    {
        Properties = new CodeProperty()
        {
            DatastoreId = $"{amlDetials}/datastores/azureml",
            Path = "train.py",
        },
    };

    var submitExperiemntRequest = new SubmitExperiemntRequest()
    {
        Properties = new ExperimentProperty()
        {
            JobType = "Command",
            CodeID = $"{amlDetials}/codes/{codeName}/versions/{codeVersion}",
            Command = "python train.py",
            Compute = new AmlCompute()
            {
                Target = $"{amlDetials}/computes/{clusterName}",
                InstanceCount = 1,
            },
            ExperimentName = experimentName,
        },
    };


    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

    // register the code
    var requestPayloadCode = JsonConvert.SerializeObject(registerCodeRequest);
    var httpContentCode = new StringContent(requestPayloadCode, Encoding.UTF8, "application/json");
    var submitResponseCode = await client.PutAsync(codeEndpoint, httpContentCode).ConfigureAwait(false);
    Console.WriteLine($"\t Step 1: code registeration {submitResponseCode.StatusCode}");
    if (!submitResponseCode.IsSuccessStatusCode)
    {
        // await WriteFailedResponse(submitResponse); // ... method not shown ...
        Console.WriteLine("Fail");
        return;
    }

    var resultCode = await submitResponseCode.Content.ReadAsStringAsync().ConfigureAwait(false);
    Console.WriteLine(JObject.Parse(resultCode));

    Console.WriteLine("-------------------------------------------------------------------------------------");
    Console.WriteLine("-------------------------------------------------------------------------------------");

    // submit the experiment
    var requestPayloadExp = JsonConvert.SerializeObject(submitExperiemntRequest);
    var httpContentExp = new StringContent(requestPayloadExp, Encoding.UTF8, "application/json");
    var submitResponseExp = await client.PutAsync(expEndpoint, httpContentExp).ConfigureAwait(false);
    Console.WriteLine($"\t Step 1: experiment submitted {submitResponseExp.StatusCode}");
    if (!submitResponseExp.IsSuccessStatusCode)
    {
        // await WriteFailedResponse(submitResponse); // ... method not shown ...
        Console.WriteLine("Fail");
        return;
    }

    var resultExp = await submitResponseExp.Content.ReadAsStringAsync().ConfigureAwait(false);
    Console.WriteLine(JObject.Parse(resultExp));




}

static class RandomUtil
{
    /// <summary>
    /// Get random string of 11 characters.
    /// </summary>
    /// <returns>Random string.</returns>
    public static string GetRandomString()
    {
        string path = Path.GetRandomFileName();
        path = path.Replace(".", ""); // Remove period.
        return path;
    }
}


// https://docs.microsoft.com/en-us/rest/api/azureml/jobs/create-or-update#commandjob
[DataContract]
public class SubmitExperiemntRequest
{
    [DataMember]
    public ExperimentProperty Properties { get; set; }
}

[DataContract]
public class ExperimentProperty
{
    [DataMember]
    public string JobType { get; set; }

    [DataMember]
    public string CodeID { get; set; }

    [DataMember]
    public string Command { get; set; }

    [DataMember]
    public AmlCompute Compute { get; set; }

    [DataMember]
    public string ExperimentName { get; set; }

}

public class AmlCompute
{
    [DataMember]
    public string Target { get; set; }

    [DataMember]
    public int InstanceCount { get; set; }
}



//  https://docs.microsoft.com/en-us/rest/api/azureml/code-versions/create-or-update
public class RegisterCodeRequest
{
    [DataMember]
    public CodeProperty Properties { get; set; }
}

[DataContract]
public class CodeProperty
{
    [DataMember]
    public string DatastoreId { get; set; }

    [DataMember]
    public string Path { get; set; }
}