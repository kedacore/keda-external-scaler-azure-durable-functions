
# KEDA Durable Functions Scaler

[![Build Status](https://durabletaskframework.visualstudio.com/Durable%20Task%20Framework%20CI/_apis/build/status/kedacore.keda-scaler-durable-functions?branchName=master)](https://durabletaskframework.visualstudio.com/Durable%20Task%20Framework%20CI/_build/latest?definitionId=17&branchName=master)

KEDA Durable Functions Scaler is an extension that enables autoscaling of Durable Functions deployed on Kubernetes cluster.
This extension uses [External Scaler Support for KEDA](https://github.com/kedacore/keda/pull/294).

The key features of KEDA Durable Functions Scaler are:

* Intelligent Auto Scaling
* One-liner deployment using Helm

## What is KEDA Durable Functions Scaler?

KEDA supports [multiple scalers](https://github.com/kedacore/keda). As a part of the scalers, this project support Durable Functions Scaler for KEDA. You can deploy Durable Functions with auto scale feature on Kubernetes.

## How KEDA Durable Functions Scaler works

KEDA Durable Functions Scaler works as a gRPC server of the [External Scaler Support](https://github.com/kedacore/keda/pull/294).

* [gRPC services with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/grpc/aspnetcore?view=aspnetcore-3.0&tabs=visual-studio)
* Watch Control/Worker queues by DurableTask and use [Scale Recommendation](https://github.com/Azure/durabletask/blob/master/src/DurableTask.AzureStorage/Monitoring/DisconnectedPerformanceMonitor.cs#L89)
* Get the current worker count from Kubernetes API Server

![Overview](docs/images/overview.png)

### Limitations

#### Minimum Pod number is 1. Not zero.

Currently, KEDA Durable Scaler can't make functions scale down to zero. The minimum pod number is one. Durable Scaler needs to send data to the control/worker queue. To achieve this behavior, we need to separate the HTTP and non-HTTP deployments. However, the feature seems not working. We need to wait until this issue is fixed.

* [Add configuration for enabling only HTTP or only non-HTTP functions #4412](https://github.com/Azure/azure-functions-host/issues/4412)
* [Pods doesn't scale in to zero #17](https://github.com/kedacore/keda-scaler-durable-functions/issues/17)

## Getting Started & Documentation

* [Getting Started](docs/getting-started.md)
* [Configration Reference Documentation](docs/reference.md)
* [Developer's Guide](docs/developers-guide.md)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
