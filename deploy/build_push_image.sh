#!/bin/bash

cd ..
docker build . -t kedacore/keda-scaler-durable-functions:latest -t kedacore/keda-scaler-durable-functions:0.1
docker push kedacore/keda-scaler-durable-functions
cd deploy