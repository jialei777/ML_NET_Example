import greet_pb2_grpc
import greet_pb2
import amlpipeline_pb2_grpc
import amlpipeline_pb2
import grpc


def greeting():
    with grpc.insecure_channel('localhost:5251') as channel:
        stub = greet_pb2_grpc.GreeterStub(channel)

        hello_request = greet_pb2.HelloRequest(name = "Jialei")
        hello_reply = stub.SayHello(hello_request)
        print("SayHello Response Received:")
        print(hello_reply)

        
def submit_aml_pipeline():
    with grpc.insecure_channel('localhost:5251') as channel:
        stub = amlpipeline_pb2_grpc.AmlJobStub(channel)

        job_detail = amlpipeline_pb2.AmlJobDetails(pipelineEndpoint = "https://westus2.api.azureml.ms/pipelines/v1.0/subscriptions/48bbc269-ce89-4f6f-9a12-c6f91fcb772d/resourceGroups/aml1p-rg/providers/Microsoft.MachineLearningServices/workspaces/aml1p-ml-wus2/PipelineRuns/PipelineEndpointSubmit/Id/f4240264-c272-4732-9483-02458f1043f7")
        submission_status = stub.SubmitAmlJob(job_detail)
        print("submission status\n",submission_status)
    
    return submission_status.runId


def check_aml_status(run_id):
    with grpc.insecure_channel('localhost:5251') as channel:
        stub = amlpipeline_pb2_grpc.AmlJobStub(channel)

        aml_run = amlpipeline_pb2.AmlRun(runId = run_id)
        run_status = stub.CheckAmlJobStatus(aml_run)
        print(run_status)



if __name__ == "__main__":
    print(">>> connect to the c# server")
    greeting()
    print(">>> submit an AMP pipeline")
    run_id = submit_aml_pipeline()
    print(">>> now check its status")
    check_aml_status(run_id)
    print("Finished!")