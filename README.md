# LocalStack AWS Dev Kit for .NET

This project provides a reference implementation for setting up local AWS cloud stack using [LocalStack](https://docs.localstack.cloud/overview/), with automated resource creation (in LocalStack) for S3, SQS, SNS, and DynamoDB. The project also includes a sample Web API (Order Management System) that connects to a local PostgreSQL database  created using Flyway migrations.

The code presented here should be treated as a POC code and should not be used as-is for production code.

While this reference implementation would work for both LocalStack and real AWS Services, you should take care of the security recommendations provided in the 'Security' section before using it in your solution.

Creation of AWS resources on a real AWS account is not in scope of this repository.
Follow AWS security best-practices while provisioning AWS resources:
https://docs.aws.amazon.com/prescriptive-guidance/latest/terraform-aws-provider-best-practices/security.html

## Key Benefits

- **Cost Savings**: No AWS charges during development and testing.
- **Fast Development**: Local services speed up feedback loops.
- **Easy Testing**: Mock AWS services for repeatable, isolated tests.
- **AWS Compatibility**: Code works with real AWS services.
- **Full Stack Emulation**: Supports S3, SNS, SQS, DynamoDB, and Aurora PostgreSQL locally.
- **CI/CD Friendly**: Lightweight Docker setup for automated testing.
- **Team Consistency**: Uniform local environment for all developers.

## Architecture

![architecture](assets/oms-localstack-architecture.png)

## 📑 Table of Contents
- [Prerequisites](#prerequisites)
- [Environment Configuration](#environment-configuration)
- [Quick Start](#quick-start)
- [API Usage Examples](#api-usage-examples)
- [Docker Commands](#docker-commands)
- [Infrastructure Components](#infrastructure-components)
  - [LocalStack](#localstack)
  - [PostgreSQL Database](#postgresql-database)
  - [Flyway Migration Service](#flyway-migration-service)
- [Health Checks](#health-checks)
- [Database Management](#database-management)
  - [Connect with Docker Desktop UI](#database-connection)
  - [Database Queries](#database-queries)
  - [Database Migrations](#database-migrations)
- [LocalStack AWS Commands](#localstack-aws-commands)
  - [SQS Commands](#sqs-commands)
  - [SNS Commands](#sns-commands)
  - [S3 Commands](#s3-commands)
  - [DynamoDB Commands](#dynamodb-commands)
  - [SecretsManager Commands](#secrets-manager-commands)
- [References](#references)
- [Security](#security)
- [License](#license)

## Prerequisites

- Docker
- Docker Compose
- AWS CLI (optional - local testing)
- .NET Runtime and SDK (version 8 or later)
- Visual Studio 2022 (optional)

## Hardware 

- Processor: 64-bit processor
- RAM: 8 GB+ of RAM
- Storage: 40 GB+

## Environment Configuration

The application fetches database credentials & API Key from AWS Secrets Manager. When running locally, using LocalStack, these values should be configured in `infra/local/.env` file. For security reasons, the default configuration contains placeholders that must be replaced with actual values.

1. Locate the placeholder in the configuration:
   ```json
   DATABASE_CREDENTIALS_SECRET_STRING={"Host": "database","Database":"order-management-system", "Port": "5432", "Username": "postgres", "Password": "<DB_PASSWORD>"}
   API_KEY_SECRET_STRING={"ApiKey":"<API_KEY>"}
   ```
   
2. Replace <DB_PASSWORD> & <API_KEY> with your actual database password & API Key.

**Note**: 
The database 'host' value is different for local & docker environments; you should update the DATABASE_CREDENTIALS_SECRET_STRING:
- For localhost: Use `host":"localhost"` in the connection string
- For Docker: Use `host":"database"` in the connection string (container name defined in docker-compose)

The database password can be set in multiple ways:
- Add "DATABASE_PASSWORD=your_db_password" in the '.env' file
- Pass as environment variable to docker compose command:
https://docs.docker.com/compose/how-tos/environment-variables/set-environment-variables/
- As a security best practice, you should not save passwords in plain-text. Use base-64 encoded password string and decode it while constructing the connection string.

## Quick Start

1. Clone the repository
2. Update environment variables in `infra/local/.env` file
3. Build docker image for the sample web api
   ```bash
   docker build -t localstack-demo .
   ```
4. Start the containers:
   ```bash
   docker-compose -f ./infra/local/docker-compose.yml --profile all up -d --wait
   ```
5. Verify the containers are running:
   ```bash
   docker-compose ps
   ```

## API Usage Examples

### Create new Order

**Bash**
```bash
curl -X POST "http://localhost:7126/api/orders" -H "Content-Type: application/json" -H "ApiKey: <your-api-key>" -d '{
  "customerId": "4638e60b-e741-4e3b-b7f3-37419a5c8ad6",
  "totalAmount": 7000
}'
```

**Windows Command Prompt**
```
curl -X POST "http://localhost:7126/api/orders" -H "Content-Type: application/json" -H "ApiKey: <your-api-key>" -d "{\"customerId\":\"4638e60b-e741-4e3b-b7f3-37419a5c8ad6\", \"totalAmount\": 5000}"
```

## Docker Commands

```bash
# Build Web API Docker Image
docker build -t localstack-demo .

# Clean build (ignoring cache)
docker build --no-cache -t localstack-demo .
```

```sh
# Start Only Infrastructure (DB & LocalStack)
docker-compose -f ./infra/local/docker-compose.yml --profile infra up -d --wait

# Start Only Database**
docker-compose -f ./infra/local/docker-compose.yml --profile db up -d --wait

# Start All Services (Including App)**
docker-compose -f ./infra/local/docker-compose.yml --profile all up -d --wait

# Terminate All Services**
docker-compose -f ./infra/local/docker-compose.yml --profile all down
```

## Infrastructure Components

### LocalStack
- Runs as a docker container
- Simulates AWS cloud services on local machine
- Accessible at `localhost:4566`
- Services: S3, SQS, SNS, DynamoDB, Secrets Manager

**Note** : Infra code (e.g., Terraform or CloudFormation) is not part of this repository and must be created explicitly to run the solution in an AWS account. 

### PostgreSQL Database
- Runs as a docker container
- Automatically initialized with credentials configured in 'infra\local\.env'
- Data persisted in docker volume

### Flyway Migration Scripts
- Scripts are auto-executed by Flyway service that runs as a docker container
- Allows schema evolution with versioned & repeatable scripts
- Configuration:
  - Migration files location: `./flyway/sql`
  - Automatic baseline on migrate enabled
  - 10 connection retries
  - Connects to: `jdbc:postgresql://database/${DATABASE_NAME}`

## Health Checks

- API health check endpoint: 
  - http://localhost:8080/health (Docker) 
  - http://localhost:5128/health (Visual Studio)
- LocalStack health check endpoint: 
  - http://localhost:4566/_localstack/health
- PostgreSQL health check is done with shell command: `pg_isready` 
- Flyway scripts are executed only after database is created and healthy

## Database

### Database Connection

You can use any DB Client of your choice, that supports PostgreSQL, to access the Order-management-system database.

Instructions to connect to the database from Docker Desktop UI:
1. Open Docker Desktop
2. Go to the "Containers" tab
3. Find your database container: `${COMPOSE_PROJECT_NAME}-database`
4. Click on the container
5. Go to the "Terminal" tab
6. Connect to PostgreSQL using: `psql -U postgres -d order-management-system`
   ```bash
   psql -U ${DATABASE_USER} -d ${DATABASE_NAME}
   ```

### Database Queries

```sql
-- List all tables
SELECT * FROM pg_catalog.pg_tables WHERE schemaname = 'oms';

-- View customers
SELECT * FROM oms.customers;

-- View orders
SELECT * FROM oms.orders;

-- Join query to see customer orders
SELECT 
    c.name,
    o.id as order_id,
    o.total_amount,
    o.status
FROM oms.customers c
JOIN oms.orders o ON c.id = o.customer_id;
```

### Database Migrations

To add new database migrations:

- Create new SQL file(s) in `./flyway/sql` folder and restart the containers to apply the new migration.
- Follow Flyway naming conventions: 
  - Versioned migrations: `V{version}__{description}.sql`
  - Repeatable migrations: `R__{description}.sql`

Read more about Flyway migrations, here: 
https://documentation.red-gate.com/fd/migrations-271585107.html


## LocalStack AWS Commands

These commands should be executed from within the LocalStack container.

#### SQS Commands

```bash
# List all SQS queues
awslocal sqs list-queues

# Receive message from queue
awslocal sqs receive-message --queue-url "http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/order-queue"
```

#### SNS Commands

```bash
# List all SNS topics
awslocal sns list-topics

# List all SNS subscriptions
awslocal sns list-subscriptions

# Publish message to Topic
awslocal sns publish --topic-arn "arn:aws:sns:us-east-1:000000000000:order-topic" --message "{\"Key\":\"sample\"}}"
```

#### S3 Commands

```bash
# List S3 bucket content
awslocal s3 ls s3://order-bucket --recursive

# Example: Copy a file from S3 bucket
awslocal s3 cp s3://order-bucket/orders/6.json .
```

#### DynamoDB Commands

```bash
# List all DynamoDB tables
awslocal dynamodb list-tables

# Scan a DynamoDB table
awslocal dynamodb scan --table-name order-metadata
```

#### Secrets Manager Commands

```bash
# Get Secret
awslocal secretsmanager get-secret-value --secret-id oms-api-key
```

## References
- [Localstack](https://docs.localstack.cloud/overview/)
- [Flyway](https://documentation.red-gate.com/flyway/getting-started-with-flyway)

## Security FAQ

[Security Recommendation and FAQ ](FAQ.md)

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.

