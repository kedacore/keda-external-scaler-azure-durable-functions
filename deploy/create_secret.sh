#!/bin/bash

kubectl create secret generic keda-durable-external-scaler --from-file grpcsv.pfx --from-file connection-string --namespace keda