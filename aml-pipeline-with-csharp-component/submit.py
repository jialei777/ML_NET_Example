from azureml.core import Workspace
from azure.ml.component import (
    Component,
    dsl,
)
import argparse

parser = argparse.ArgumentParser(description='write model and data')
parser.add_argument('--clents_group_number', type=int, default = 10)
parser.add_argument('--dist_component', type=bool, default = True)
args = parser.parse_args()

print("clents group number", "is", args.clents_group_number)

workspace = Workspace.from_config()

cluster_name = "cpu-cluster" 

if args.dist_component:
    component_func = Component.from_yaml(workspace, yaml_file="C:/Users/jialeichen/source/mycode/aml-pipeline-with-csharp-compoennt/aml-csharp-component/spec-dist.yaml")
else:
    component_func = Component.from_yaml(workspace, yaml_file="C:/Users/jialeichen/source/mycode/aml-pipeline-with-csharp-compoennt/aml-csharp-component/spec.yaml")


@dsl.pipeline(
    name="pipeline-many-clinets",
    description="my description",
    default_compute_target=cluster_name,
)
def finetuning_pipeline():

    for _ in range(args.clents_group_number):
        my_csharp_component = component_func()
    
pipeline = finetuning_pipeline()
run = pipeline.submit()
