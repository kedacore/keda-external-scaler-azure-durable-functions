#!/bin/bash

cd ..
docker build . -t tsuyoshiushio/durableexternalscaler:latest -t tsuyoshiushio/durableexternalscaler:0.1
docker push tsuyoshiushio/durableexternalscaler
cd deploy