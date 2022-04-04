import os
from azureml.core.workspace import Workspace
from azureml.core.runconfig import RunConfiguration
from azureml.core.compute import AmlCompute
from azureml.core.conda_dependencies import CondaDependencies
from azureml.core.experiment import Experiment
from azureml.core import ScriptRunConfig



ws = Workspace.from_config(path="config.json")
list_vms = AmlCompute.supported_vmsizes(workspace=ws)


compute_config = RunConfiguration()
compute_config.target = "cpucluster-jialei"
#compute_config.amlcompute.vm_size = "STANDARD_D1_V2"


dependencies = CondaDependencies()
dependencies.add_pip_package("scikit-learn")
dependencies.add_pip_package("numpy==1.15.4")
dependencies.add_pip_package("pandas")
compute_config.environment.python.conda_dependencies = dependencies


script_run_config = ScriptRunConfig(source_directory=os.getcwd(), script="evaluation.py", run_config=compute_config)
experiment = Experiment(workspace=ws, name="jialei-exp-test-2")
run = experiment.submit(config=script_run_config)
run.wait_for_completion(show_output=True)