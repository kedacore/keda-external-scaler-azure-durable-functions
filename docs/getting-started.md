# Getting Started

Welcome to KEDA Durable Scaler! 
This getting started guide introduces how to set up the KEDA Durable Scaler and walks you thorough their basic usage. 

## Prerequisite 

* Kubernetes Cluster with kubectl command is configured. If you are using Azure, See [Quickstart: Deploy an Azure Kubernetes Service cluster using the Azure CLI](https://docs.microsoft.com/en-us/azure/aks/kubernetes-walkthrough).
* Docker works on your local machine. See [Get started with Docker Desktop for Mac](https://docs.docker.com/docker-for-mac/) or [Get started with Docker for Windows](https://docs.docker.com/docker-for-windows/).
* Latest [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/get-started-with-azure-cli?view=azure-cli-latest) is installed 
* Linux based shell terminal include [WSL](https://docs.microsoft.com/en-us/windows/wsl/install-win10).

## Deploy KEDA

Clone the KEDA repo

```bash
$ git clone git@github.com:kedacore/keda.git
$ cd keda
```

Currently, [External Scaler](https://github.com/kedacore/keda/pull/294) is not supported for helm chart and [Azure Function Core Tools](https://github.com/Azure/azure-functions-core-tools). Deploy with Yaml files. Double check the procedure has not changed in [KEDA](https://github.com/kedacore/keda)repo.

```bash
$ kubectl create namespace keda
$ kubectl apply -f deploy/crds/keda.k8s.io_scaledobjects_crd.yaml
$ kubectl apply -f deploy/crds/
$ keda.k8s.io_triggerauthentications_crd.yaml
$ kubectl apply -f deploy/
```

## Deploy Durable Scaler 

Clone the KEDA Durable Scaler repo

```bash
$ git clone git@github.com:kedacore/keda-scaler-durable-functions.git
$ cd keda-durable-scaler
```

Create a self-signed certificate. You can find the script to create it. The self-signed certificate is used for gRPC between KEDA and Durable Scale Server.

```bash
$ cd deploy
$ ./generate_cert.sh
$ ls
grpcsv.pfx  private.pem  public.pem
```

Save a connection string of Storage account used for Durable Functions. This Scale server watches the queue. 

```bash
$ echo -n '<YOUR_CONNECITON_STRING_HERE>' > ./connection-string
```

Create a secret. Using the certificate, you need to create a secret for kubernetes resources. You can find the script. It will create `keda-durable-external-scaler` secret on `keda` namespace. 

```bash
$ ./create_secrets.sh
$ kubectl describe secret keda-durable-external-scaler -n keda
Name:         keda-durable-external-scaler
Namespace:    keda
Labels:       <none>
Annotations:  <none>

Type:  Opaque

Data
====
connection-string:  186 bytes
grpcsv.pfx:         2629 bytes
```

Deploy Durable Scaler Server. The yaml file will create a service, deployment, ServiceAccount, and ClusterRole bindings. It requires ClusterRoleBindings for accessing Kubernetes API server. 

```bash
$ kubectl apply -f deployment.yml
```
That's it. Now you are ready for deploying Durable Functions App. 

## Deploy Durable Functions 

Create a secret for the Durable Functions. Go to the `examples/durable-keda` directory. 

```bash
$ cd examples/durable-keda
```

Copy the `secret.yml.example` to `secret.yml`

```bash
$ cp secret.yml.example secret.yml
```

Modify the `secret.yml`. You need to put your Connection String for Durable Functions. Modify the `<YOUR_CONNECTION_STRING_BASE64_HERE>`.

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: durable-keda
  namespace: default
data:
  AzureWebJobsStorage: <YOUR_CONNECTION_STRING_BASE64_HERE>
  FUNCTIONS_WORKER_RUNTIME: ZG90bmV0
  WEBSITE_HOSTNAME: bG9jYWxob3N0Ojgw
```

To obtain the base64 string, you can use `connection-string` file that you created when you deploy KEDA Durable Scaler. You will see the base64 representation of the connection string. 

```bash
$ pushd . 
$ cd ../../deploy
$ base64 < connection-string 
<YOU WILL SEE THE BASE64>
$ popd 
```

FYI, `FUNCTIONS_WORKER_RUNTIME` is `dotnet` and `WEBSITE_HOSTNAME` is `localhost:80`. 

**NOTE:** Currently, the yaml file for dotnet durable functions that is generated from [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools) doesn't work. I've got this yaml files from the generated yaml from the tool and modify it to make it work. 

Create secret and deploy Durable Functions. 

```bash
$ kubectl apply -f secret.yml
$ kubectl apply -f deployone.yml
```

If you want to modify Durable Functions, you can refer the `build_push_image.sh`. Please copy and modify the docker image name to your DockerHub account. Also you need to modify `deployone.yml` script. 

## Scale testing

Now you can test the scaler. 

Get the Endpoint of your durable functions. You can see the endpoints. Get the EXTERNAL-IP of `durable-keda`. It is durable 

```bash
$ kubectl get svc
NAME           TYPE           CLUSTER-IP   EXTERNAL-IP    PORT(S)        AGE
durable-keda   LoadBalancer   10.0.221.5   40.90.221.xxx   80:31180/TCP   44h
kubernetes     ClusterIP      10.0.0.1     <none>         443/TCP        4d21h
```

Find the pod for the keda durable scaler and see the logs. 

```bash
$ kubectl get pods -n keda
NAME                                            READY   STATUS    RESTARTS   AGE
keda-durable-external-scaler-54854fc478-n2mjs   1/1     Running   0          3m14s
keda-operator-57c9c8d7fc-rbxq2                  2/2     Running   0          4d1h
```

Open the log stream. Specify the pod name of the `keda-durable-external-scaler-xxxxxx` and put it after `-f` flags. You will see the log stream of the KEDA durable scaler. 

```bash
$ kubectl logs -f keda-durable-external-scaler-54854fc478-n2mjs -n keda
```

Open the browser and hit this URL. The URL will create a new orchestration with sleep at the activity for a while. Hit many times. The IP ADDRESS is taken from the `kubectl get svc` command.

```
40.90.221.xxx/api/LoadOrchestration_HttpStart
```

Wait for a while, you will see several containers are started. 

```bash
$ kubectl get pods
NAME                            READY   STATUS    RESTARTS   AGE
durable-keda-77875c7d7d-5ddfl   1/1     Running   0          13s
durable-keda-77875c7d7d-bbfmw   1/1     Running   0          3m28s
durable-keda-77875c7d7d-ndphh   1/1     Running   0          44h
durable-keda-77875c7d7d-sxprf   1/1     Running   0          3m16s
```

It gradually go back to one if you don't send addition request. It will take time to spin up a new container. 



