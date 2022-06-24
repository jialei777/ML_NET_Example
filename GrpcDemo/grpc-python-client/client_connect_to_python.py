import greet_pb2_grpc
import greet_pb2
import grpc


def run():
    with grpc.insecure_channel('localhost:50051') as channel:
        stub = greet_pb2_grpc.GreeterStub(channel)

        hello_request = greet_pb2.HelloRequest(name = "Jialei")
        hello_reply = stub.SayHello(hello_request)
        print("SayHello Response Received:")
        print(hello_reply)


if __name__ == "__main__":
    print(">>> connect to the python server")
    run()