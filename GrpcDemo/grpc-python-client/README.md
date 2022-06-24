- env
```
pip install grpcio grpcio-tools
```

- generate files
```
python -m grpc_tools.protoc -I protos --python_out=. --grpc_python_out=. protos/greet.proto
python -m grpc_tools.protoc -I protos --python_out=. --grpc_python_out=. protos/amlpipeline.proto
```

- python server and client (only implement the greeter service)
```
python server.py
python client_connect_to_python.py
```


- c-sharp server and python client (both greeter and aml submission)
run c-sharp project `GrpcService` only 
```
python client_connect_to_csharp.py
```