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
exp = Experiment(workspace=workspace, name="FL-linear-regression-2")


# choose a name for your cluster
cluster_name = "cpucluster-jialei"

compute_target = workspace.compute_targets[cluster_name]

data_urls = ["https://data4mldemo6150520719.blob.core.windows.net/demo/mnist-fashion"]
fashion_ds = Dataset.File.from_files(data_urls)

# list the files referenced by fashion_ds
print(fashion_ds.to_path())


# datastore = workspace.get_default_datastore()
# prepared_fashion_ds = OutputFileDatasetConfig(
#     destination=(datastore, "outputdataset/{run-id}")
# ).register_on_complete(name="prepared_fashion_ds")


pipeline_param = PipelineParameter(
  name="model_url",
  default_value="https://storagefakedata.blob.core.windows.net/testdatacontainer/model.csv")

script_folder = "./eval"


keras_env = Environment.from_conda_specification(
    name="keras-env", file_path="./conda_dependencies.yml"
)


eval_cfg = ScriptRunConfig(
    source_directory=script_folder,
    script="evaluation.py",
    compute_target=compute_target,
    environment=keras_env,
)

eval_step = PythonScriptStep(
    name="evaluation step",
    arguments=[ fashion_ds.as_named_input("fashion_ds").as_mount(),  pipeline_param],
    source_directory=eval_cfg.source_directory,
    script_name=eval_cfg.script,
    runconfig=eval_cfg.run_config,
)


pipeline = Pipeline(workspace, steps=[eval_step])
run = exp.submit(pipeline)


run.display_name = "My Run"
run.wait_for_completion(show_output=True)
run.find_step_run("evaluation step")[0].get_metrics()




from azureml.pipeline.core import PipelineEndpoint

published_pipeline = pipeline
# published_pipeline = PublishedPipeline.get(workspace=ws, name="My_Published_Pipeline")
pipeline_endpoint = PipelineEndpoint.publish(workspace=workspace, name="FL-linear-reg-3",
                                            pipeline=published_pipeline, description="Test endpoint")
