import os
import azureml.core
from azureml.core import (
    Workspace,
    Experiment,
    Dataset,
    Datastore,
    ComputeTarget,
    Environment,
    ScriptRunConfig
)
from azureml.data import OutputFileDatasetConfig
from azureml.core.compute import AmlCompute
from azureml.core.compute_target import ComputeTargetException
from azureml.pipeline.steps import PythonScriptStep
from azureml.pipeline.core import Pipeline
from azureml.pipeline.core.graph import PipelineParameter


# check core SDK version number
print("Azure ML SDK Version: ", azureml.core.VERSION)


workspace = Workspace.from_config()
exp = Experiment(workspace=workspace, name="keras-mnist-fashion-2")

use_gpu = True

# choose a name for your cluster
cluster_name = "gpucluster-jialei" if use_gpu else "cpucluster-jialei"

found = False
# Check if this compute target already exists in the workspace.
cts = workspace.compute_targets
if cluster_name in cts and cts[cluster_name].type == "AmlCompute":
    found = True
    print("Found existing compute target.")
    compute_target = cts[cluster_name]
if not found:
    print("Creating a new compute target...")
    compute_config = AmlCompute.provisioning_configuration(
        vm_size= "STANDARD_NC6" if use_gpu else "STANDARD_D2_V2",
        # vm_priority = 'lowpriority', # optional
        max_nodes = 4,
    )

    # Create the cluster.
    compute_target = ComputeTarget.create(workspace, cluster_name, compute_config)

    # Can poll for a minimum number of nodes and for a specific timeout.
    # If no min_node_count is provided, it will use the scale settings for the cluster.
    compute_target.wait_for_completion(
        show_output=True, min_node_count=None, timeout_in_minutes=10
    )
# For a more detailed view of current AmlCompute status, use get_status().print(compute_target.get_status().serialize())

data_urls = ["https://data4mldemo6150520719.blob.core.windows.net/demo/mnist-fashion"]
fashion_ds = Dataset.File.from_files(data_urls)

# list the files referenced by fashion_ds
print(fashion_ds.to_path())


datastore = workspace.get_default_datastore()
prepared_fashion_ds = OutputFileDatasetConfig(
    destination=(datastore, "outputdataset/{run-id}")
).register_on_complete(name="prepared_fashion_ds")


pipeline_param = PipelineParameter(
  name="train_epochs",
  default_value="10")

script_folder = "./keras-mnist-fashion"

prep_step = PythonScriptStep(
    name="prepare step",
    script_name="prepare.py",
    # On the compute target, mount fashion_ds dataset as input, prepared_fashion_ds as output
    arguments=[fashion_ds.as_named_input("fashion_ds").as_mount(), prepared_fashion_ds],
    source_directory=script_folder,
    compute_target=compute_target,
    allow_reuse=True,
)


keras_env = Environment.from_conda_specification(
    name="keras-env", file_path="./conda_dependencies.yml"
)

train_cfg = ScriptRunConfig(
    source_directory=script_folder,
    script="train.py",
    compute_target=compute_target,
    environment=keras_env,
)

train_step = PythonScriptStep(
    name="train step",
    arguments=[
        prepared_fashion_ds.read_delimited_files().as_input(name="prepared_fashion_ds"), pipeline_param
    ],
    source_directory=train_cfg.source_directory,
    script_name=train_cfg.script,
    runconfig=train_cfg.run_config,
)


pipeline = Pipeline(workspace, steps=[prep_step, train_step])
run = exp.submit(pipeline)

run.wait_for_completion(show_output=True)
run.find_step_run("train step")[0].get_metrics()



# run.find_step_run("train step")[0].register_model(
#     model_name="keras-model",
#     model_path="outputs/model/",
#     datasets=[("train test data", fashion_ds)],
# )



# from azureml.pipeline.core import PipelineEndpoint

# published_pipeline = pipeline
# # published_pipeline = PublishedPipeline.get(workspace=ws, name="My_Published_Pipeline")
# pipeline_endpoint = PipelineEndpoint.publish(workspace=workspace, name="PipelineEndpointTest1",
#                                             pipeline=published_pipeline, description="Test endpoint")
