## publish the c# project to `src`

- self-contained, linux

## submit job:

```
az account set --subscription "aims"
az login
az ml job create -f ./job.yml --web -g "CD-FL-rg" -w "fl-workspace-test"
```

## successful run:
[fl-workspace-test workspace](https://ml.azure.com/experiments/id/35d7dad5-5ffe-4220-8f2f-5bc324766353?wsid=/subscriptions/48bbc269-ce89-4f6f-9a12-c6f91fcb772d/resourcegroups/CD-FL-rg/workspaces/fl-workspace-test&tid=72f988bf-86f1-41af-91ab-2d7cd011db47)