#!/bin/bash
source ./set-envs.sh

# Build process
docker build -t $IMAGE_NAME .
docker tag $IMAGE_NAME $AWS_ECS_REPO_DOMAIN/$IMAGE_NAME:$IMAGE_VERSION
