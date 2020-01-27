#!/bin/bash

docker build . -t kedacore/durable-keda:latest -t kedacore/durable-keda:0.1
docker push kedacore/durable-keda
