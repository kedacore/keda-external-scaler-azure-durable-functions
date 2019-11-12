# Configuration Reference

This document covers the details of configration of the Durable Scaler.

## KEDA Durable Scaler Configration

The configuration of the Scale Server. You can refer the actual yaml file in [here](../../deploy/deployment.yml).

### Example Deployment 

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: keda-durable-external-scaler
  namespace: keda
spec:
  selector:
    matchLabels:
      service: keda-durable-external-scaler
  replicas: 1
  template:
    metadata:
      labels:
        service: keda-durable-external-scaler
    spec:
      serviceAccountName: keda-durable-scaler
      containers:
      - image: tsuyoshiushio/durableexternalscaler:latest
        name: scaler
        ports:
          - containerPort: 5000
        env:
        - name: CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: keda-durable-external-scaler
              key: connection-string 
        - name: TASK_HUB
          value: DurableFunctionsHub
        - name: MAX_POLLING_INTERVAL
          value: "5000"
        - name: Logging__LogLevel__Grpc
          value: Debug
        - name: Logging__LogLevel__Keda.Durable.Scaler.Server.Services.ExternalScalerService
          value: Debug
        - name: Logging__LogLevel__Default
          value: Information
        - name: CERT_PATH
          value: /certs/grpcsv.pfx
        - name: CERT_PASS
          value: keda
        volumeMounts:
        - name: certs
          mountPath: /certs
          readOnly: true
      volumes:
      - name: certs
        secret:
          secretName: keda-durable-external-scaler
```
### Argument Reference
The following arguments are supported:
* `containerPort` - (Required) gRPC configuration for the protocol. 5000 for http, 5001 for https. (currently, 5000 only. 5001 will be supporeted soon.)
* `TASK_HUB` - (Required) [TaskHub](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs) name of your Durable Functions. 
* `MAX_POLLING_INTERVAL` - (Optional) Polling interval (millisecond) for DurableTask. It is not the polling interval of the KEDA scaler. DurableTask, that is used in this Scaler, is polling individually. Default Value is 5000.
* `Logging__LogLevel__Grpc` (Optional) Log Level for gRPC. Possible values are `Trace, Debug, Information, Warning, Error, and Critical`. For more information [Log level](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/index?view=aspnetcore-3.0#log-level), [Logging and diagnostics in gRPC on .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/diagnostics?view=aspnetcore-3.0)
* `Logging__LogLevel__Keda.Durable.Scaler.Server.Services.ExternalScalerService`: (Optional) Log Level for core service part of KEDA Durable Scaler Server. Possible values are the same as above. 
* `Logging__LogLevel__Default`: (Optional) Log Level for all classes of KEDA Durable Scale Server. Possible values are the same above. 
* `CERT_PATH`: (Required) The path for the certificate of TLS for https. 
* `CERT_PASS`: (Required) The password the certificate of TLS for https. It recommended to make it secret for production.

## Durable Functions Scale Object

Scale Object is configration object of this scaler. Please refer actual example in [here](../../examples/durable-keda/deployone.yml).

### Example ScaleObject

```yaml
apiVersion: keda.k8s.io/v1alpha1
kind: ScaledObject
metadata:
  name: durable-keda
  namespace: default
  labels:
    deploymentName: durable-keda
spec:
  scaleTargetRef:
    deploymentName: durable-keda
  triggers:
  - type: external
    metadata:
      scalerAddress: durable-external-scaler-service.keda.svc.cluster.local:5000
```

### Argument Reference
The following arguments are supported:

* `metadata.name`: (Required) The name of the ScaleObject. It should be the same as deploymentName. 
* `metadata.namespace`: (Required) The namespace. It should be the same as deployment of durable functions. 
* `metadata.labels.deploymentName`: (Required) The name of the deployment of durable functions. 
* `spec.scaleTargetRef.deploymentName`: (Required) The name of the deployment of durable functions. 
* `spec.triggers.metadata.scaleAddress`: (Required) Address and port of KEDA Durable Scale Server's endpoint.
* `spec.triggers.metadata.tlsCertFile`: (Optional) TLS certfile for https between KEDA and KEDA Durable Scaler Server. Currently not supported. Coming soon. 


