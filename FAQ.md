## Security

The sample code presented here should be treated like a POC code and should not be used as-is for production code.

The following security measures should be implemented before using this code in your solution:
- Database credentials should not be stored in .env file. Get DB credentials from AWS Services such as [Secrets Manager](https://docs.aws.amazon.com/redshift/latest/mgmt/data-api-secrets.html) or [Systems Manager Parameter store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-parameter-store.html). Enable SSL for connections (`SslMode=Require` in connection string).
- Implement [authentication for the Web API](https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-control-access-to-api.html) endpoints.
- Log sensitive data securely & Keep dependencies updated (e.g., AWSSDK.*, Npgsql) via .csproj files.
- Access all external services over https.
- There is no authentication for LocalStack services by default. Use HTTPS when [running LocalStack remotely](https://docs.localstack.cloud/references/configuration/).
- Ensure [IAM policies and security groups](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html) are properly configured in AWS.
- [Encrypt sensitive data at rest using KMS](https://docs.aws.amazon.com/network-firewall/latest/developerguide/kms-encryption-at-rest.html) when running in AWS.
- LocalStack provides partial support for [IAM](https://docs.localstack.cloud/user-guide/security-testing/iam-enforcement/), but does not enforce role-based access control. Be aware of the limitations in LocalStack.
- Apply & follow [docker security](https://docs.docker.com/engine/security/) best practices. 


## FAQ

### 1. What are the limitations with LocalStack? Does it support all AWS services?

- [Feature Coverage](https://docs.localstack.cloud/user-guide/aws/feature-coverage/) : LocalStack emulates many AWS services but its feature coverage has limitations compared to actual AWS services.
- [LocalStack FAQ](https://docs.localstack.cloud/getting-started/faq/) : This page answers the frequently asked questions about differet LocalStack Editions.

### 2. What is the process for switching from the local development environment to actual AWS environment?

- *Update AWS endpoints* : Replace localhost configuration with actual AWS endpoints. Remove override for AWS_SERVICE_URL and use a valid AWS RegionEndpoint.
- *Configure IAM roles and policies*: Ensure appropriate [permissions in AWS](https://docs.aws.amazon.com/IAM/latest/UserGuide/access_policies.html).
- *Enable security configurations*: Use [S3 bucket encryption](https://docs.aws.amazon.com/AmazonS3/latest/userguide/bucket-encryption.html), Use KMS to encrypt sensitive data, Use IAM authentication for API calls
- *Add Infra code and deploy infrastructure* - using Terraform, CloudFormation, etc..

**Note** : Infra code (e.g., Terraform or CloudFormation) must be created explicitly to run the solution in an AWS account. LocalStack emulates AWS services on local machine.

### 3. How to Debug and Troubleshoot issues encountered while running the solution?

When issues arise in the Order Management System API, follow these steps to identify and resolve them:
- Check Application Logs: Review console output or logs configured via ILogger. 
- Check [LocalStack logs](https://docs.localstack.cloud/references/logging/) in docker.
- Check Database connectivity: For PostgreSQL, Use a DB Client like psql or DBeaver to connect with credentials from appsettings.json. Verify that the OMS schema and tables exist.

### 4. What is the Backup and Recovery strategy for this solution?

For the local development environment using LocalStack and PostgreSQL, the following backup and recovery strategy is recommended:
- PostgreSQL: Use pg_dump to export the order-management-system database. Reapply Flyway migrations if the schema is missing.
- LocalStack: By default, LocalStack is ephemeral, i.e., data resets on container stop. Enable persistence by [mounting a volume](https://docs.localstack.cloud/references/filesystem/).
