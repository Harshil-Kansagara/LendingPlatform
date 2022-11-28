#!/bin/bash
if [ "${BUILD_SOURCEBRANCHNAME}" = "master" -o "${BUILD_SOURCEBRANCHNAME}" = "dev" -o "${BUILD_SOURCEBRANCHNAME}" = "staging" ]; then
    export REGION=us-east-2
    export PROJECT_NAME=lendingplatform-$BUILD_SOURCEBRANCHNAME-backend
    export CLUSTER=lendingplatform-$BUILD_SOURCEBRANCHNAME
    dotnet publish -r linux-x64 -f netcoreapp3.1 --configuration Release -o ./published-app --self-contained false
    cd ./published-app/ 
    docker build --rm=false -t $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/backend:$BUILD_SOURCEBRANCHNAME.$BUILD_BUILDID .
    #Install ECS-CLI
    sudo curl -Lo /usr/local/bin/ecs-cli https://amazon-ecs-cli.s3.amazonaws.com/ecs-cli-linux-amd64-latest
    sudo chmod +x /usr/local/bin/ecs-cli
    ecs-cli --version
    cd ../.ecs
    ecs-cli configure --region $REGION --cluster $CLUSTER
    ecs-cli push $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/backend:$BUILD_SOURCEBRANCHNAME.$BUILD_BUILDID
    DOCKER_IMAGE=$AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/backend:$BUILD_SOURCEBRANCHNAME.$BUILD_BUILDID
    sed "s|docker_image|"$DOCKER_IMAGE"|g; s|container_name|"$PROJECT_NAME"|g; s|REGION|"$REGION"|g" $BUILD_SOURCEBRANCHNAME.temp.yml >$BUILD_SOURCEBRANCHNAME.yml
    sed "s|AWS_ACCOUNT_ID|"$AWS_ACCOUNT_ID"|g; s|container_name|"$PROJECT_NAME"|g" $BUILD_SOURCEBRANCHNAME.ecs-params.temp.yml >ecs-params.yml
    ecs-cli compose --file $BUILD_SOURCEBRANCHNAME.yml --project-name $PROJECT_NAME service up --timeout 30
fi
