import numpy as np
import os
from azureml.core import Run, Dataset
import pandas as pd


# dataset object from the run
run = Run.get_context()

model_url = sys.argv[2]

print(sys.argv)
print('model url is:', model_url)
model = np.genfromtxt(model_url, delimiter=",")[1,]

print('current model is:', model)

# start an Azure ML run
run = Run.get_context()

score = model


# log a single value
run.log("Slope", score[0])
run.parent.log("Slope", score[0])
print("Slope", score[0])

run.log("Inter", score[1])
run.parent.log("Inter", score[1])
print("Inter", score[1])


# create a ./outputs/model folder in the compute target
# files saved in the "./outputs" folder are automatically uploaded into run history
os.makedirs("./outputs/model", exist_ok=True)

model_df = pd.DataFrame(model.reshape(1,2), columns=['Slope', 'Inter'])
model_df.to_csv("./outputs/model/model.csv", index=False)

# save model JSON
# with open("./outputs/model/model.json", "w") as f:
#     f.write(model_json)
    
print("model saved in ./outputs/model folder")