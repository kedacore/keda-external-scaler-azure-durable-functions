#!/bin/bash

docker build . -t tsuyoshiushio/durable-keda:latest -t tsuyoshiushio/durable-keda:0.1
docker push tsuyoshiushio/durable-keda
