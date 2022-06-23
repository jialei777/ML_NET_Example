// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using GrpcService;


var input = new HelloRequest { Name = "Jialei" };

//var channel = GrpcChannel.ForAddress("http://localhost:5251");
var channel = GrpcChannel.ForAddress("https://localhost:7251");
var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(input);

Console.WriteLine(reply.Message);

Console.ReadLine();
