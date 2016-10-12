#!/bin/bash

# Set variables
export IMAGE_NAME=netcoreapps-stackapis
export IMAGE_VERSION=latest

export AWS_DEFAULT_REGION=us-east-1
export AWS_ECS_CLUSTER_NAME=default
#AWS_ACCOUNT_NUMBER={} set in private variable
export AWS_ECS_REPO_DOMAIN=$AWS_ACCOUNT_NUMBER.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com

export ECS_SERVICE=$IMAGE_NAME-service
export ECS_TASK=$IMAGE_NAME-task
