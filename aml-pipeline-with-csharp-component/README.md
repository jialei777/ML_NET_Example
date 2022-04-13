### Preparation

Build Conda environment:
```
conda create --name aml-python-sdk python=3.8
conda activate aml-python-sdk
pip install azureml-core, azureml-pipeline, azure-ml-component
```

### Pipeline Endpoint Instantiation (for linear regression job in Florida)
```
conda activate aml-python-sdk
python .\submit.py --clents_group_number 16
```