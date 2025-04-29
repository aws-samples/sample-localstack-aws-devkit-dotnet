#!/bin/bash
set -e

echo "Waiting for LocalStack..."
while ! curl -s http://localstack:4566/_localstack/health > /dev/null; do
  sleep 1
done
echo "LocalStack is ready!"

echo "Creating S3 bucket..."
if aws --endpoint-url=${AWS_ENDPOINT_URL} s3api head-bucket --bucket ${BUCKET_NAME} 2>/dev/null; then
  echo "Bucket ${BUCKET_NAME} already exists"
else
  aws --endpoint-url=${AWS_ENDPOINT_URL} s3 mb s3://${BUCKET_NAME} && echo "Bucket created successfully" || { echo "Failed to create bucket: $?"; exit 1; }
fi

echo "Creating SQS queue..."
aws --endpoint-url=${AWS_ENDPOINT_URL} sqs create-queue \
  --queue-name ${SQS_QUEUE_NAME} \
  --attributes "{\"ReceiveMessageWaitTimeSeconds\":\"20\"}" || { echo "Failed to create SQS queue: $?"; exit 1; }

echo "Creating SNS topic..."
aws --endpoint-url=${AWS_ENDPOINT_URL} sns create-topic \
  --name ${SNS_TOPIC_NAME} || { echo "Failed to create SNS topic: $?"; exit 1; }

echo "Setting up SNS-SQS subscription..."
aws --endpoint-url=${AWS_ENDPOINT_URL} sns subscribe \
  --topic-arn ${SNS_TOPIC_ARN} \
  --protocol sqs \
  --notification-endpoint ${SQS_QUEUE_ARN} \
  --attributes "{\"RawMessageDelivery\":\"true\"}" || { echo "Failed to subscribe SQS to SNS: $?"; exit 1; }

echo "Creating DynamoDB table..."
if aws --endpoint-url=${AWS_ENDPOINT_URL} dynamodb describe-table --table-name order-metadata 2>/dev/null; then
  echo "DynamoDB table order-metadata already exists"
else
  aws --endpoint-url=${AWS_ENDPOINT_URL} dynamodb create-table \
    --table-name order-metadata \
    --attribute-definitions AttributeName=OrderId,AttributeType=S \
    --key-schema AttributeName=OrderId,KeyType=HASH \
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 && echo "DynamoDB table created successfully" || { echo "Failed to create DynamoDB table: $?"; exit 1; }
fi

echo "Creating Database Secrets in Secrets Manager..."
_secret_check=$(aws --endpoint-url=${AWS_ENDPOINT_URL} secretsmanager list-secrets --query "SecretList[?Name==\`${DATABASE_CREDENTIALS_SECRET_ID}\`].Name" --output text)
if [ "${_secret_check}" != "" ]; then
  echo "Secret ${DATABASE_CREDENTIALS_SECRET_ID} already exists"
else
  aws --endpoint-url=${AWS_ENDPOINT_URL} secretsmanager create-secret \
      --name ${DATABASE_CREDENTIALS_SECRET_ID} \
      --description "Database credentials for OMS" \
      --secret-string "${DATABASE_CREDENTIALS_SECRET_STRING}" || { echo "Failed to create database secret: $?"; exit 1; }
fi

echo "Creating API Key Secrets in Secrets Manager..."
_secret_check=$(aws --endpoint-url=${AWS_ENDPOINT_URL} secretsmanager list-secrets --query "SecretList[?Name==\`${API_KEY_SECRET_ID}\`].Name" --output text)
if [ "${_secret_check}" != "" ]; then
  echo "Secret ${API_KEY_SECRET_ID} already exists"
else
  aws --endpoint-url=${AWS_ENDPOINT_URL} secretsmanager create-secret \
      --name ${API_KEY_SECRET_ID} \
      --description "API Key credentials for OMS" \
      --secret-string "${API_KEY_SECRET_STRING}" || { echo "Failed to create API key secret: $?"; exit 1; }
fi

echo "Verifying created resources..."
echo "S3 Buckets:"
aws --endpoint-url=${AWS_ENDPOINT_URL} s3 ls || { echo "Failed to list S3 buckets: $?"; exit 1; }

echo "SQS Queues:"
aws --endpoint-url=${AWS_ENDPOINT_URL} sqs list-queues || { echo "Failed to list SQS queues: $?"; exit 1; }

echo "SNS Topics:"
aws --endpoint-url=${AWS_ENDPOINT_URL} sns list-topics || { echo "Failed to list SNS topics: $?"; exit 1; }

echo "Queue Attributes:"
aws --endpoint-url=${AWS_ENDPOINT_URL} sqs get-queue-attributes \
  --queue-url ${SQS_QUEUE_URL} \
  --attribute-names All || { echo "Failed to get queue attributes: $?"; exit 1; }

echo "Verifying DynamoDB table..."
aws --endpoint-url=${AWS_ENDPOINT_URL} dynamodb list-tables || { echo "Failed to list DynamoDB tables: $?"; exit 1; }

echo "Verifying Secrets..."
aws --endpoint-url=${AWS_ENDPOINT_URL} secretsmanager get-secret-value \
  --secret-id ${DATABASE_CREDENTIALS_SECRET_ID} || { echo "Failed to get database secret: $?"; exit 1; }

aws --endpoint-url=${AWS_ENDPOINT_URL} secretsmanager get-secret-value \
  --secret-id ${API_KEY_SECRET_ID} || { echo "Failed to get API key secret: $?"; exit 1; }

echo "AWS setup completed successfully!"
