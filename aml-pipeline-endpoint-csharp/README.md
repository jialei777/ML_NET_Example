### Preparation

Build Conda environment:
```
conda create --name aml-python-sdk python=3.8
conda activate aml-python-sdk
pip install azureml-core, azureml-pipeline
```

### Pipeline Endpoint Instantiation (for linear regression job in Florida)
```
conda activate aml-python-sdk
python submit-lr.py
```
Florida [repo](https://msktg.visualstudio.com/CompliantML/_git/Florida?path=/UI_files/LinearRegressionExample)

### Resubmit from Csharp
- copy the pipeline endpoint to line 11
- change the "display name" for each run at line 24
- set a pipeline input value (i.e., link to the model) at line 30

current data storage for the models [here](https://ms.portal.azure.com/#blade/Microsoft_Azure_Storage/ContainerMenuBlade/overview/storageAccountId/%2Fsubscriptions%2F48bbc269-ce89-4f6f-9a12-c6f91fcb772d%2FresourceGroups%2FCD-FL-rg%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fstoragefakedata/path/testdatacontainer/etag/%220x8DA0D0933FFABA3%22/defaultEncryptionScope/%24account-encryption-key/denyEncryptionScopeOverride//defaultId//publicAccessVal/Container)

### links
MNIST pipeline [here](https://docs.microsoft.com/en-us/azure/machine-learning/tutorial-pipeline-python-sdk)
Rest endpoint [here](https://docs.microsoft.com/en-us/azure/machine-learning/how-to-deploy-pipelines).


