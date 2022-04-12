## submit job:

> az account set --subscription "aims"
> az login
> az ml job create -f ./job.yml --web -g "CD-FL-rg" -w "fl-workspace-test"