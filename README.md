# c# stack

A CRUD stack running on Docker.

- c# api
  - basic CRUD for a user
  - audit record functionality
- postgres backend (with pgadmin)

Tooling

- pgadmin

## Getting started

Copy the sample environment variables and change the passwords etc:

```bash
cp .env.sample .env
```

Config for pgadmin (automates registering the database with pgadmin):

```bash
./pgadmin-dev-setup/setup.sh
```

Start the stack:

```bash
docker-compose up
```

- pgadmin is running at http://localhost:8080/ (use the credentials you configured in the .env file)
- see API spec at http://localhost:8081/swagger/v1/swagger.json

## Making changes

A bit heavy handed, but the following will rebuild what's needed and restart the stack:

```bash
docker-compose up --build
```

## Updating DB entities

Make changes to the entities & then add a new migration:

```bash
dotnet ef migrations add <NameOfMigration>
```

## Using the CLI

Build the project, alias the CLI:

```bash
dotnet build ./src/Cli
alias cli=./src/Cli/bin/Debug/net9.0/Cli
```

Authenticate using Google auth (assuming you're calling against the local stack):

```bash
cli authenticate -e http://localhost:8081
```

List users:

```bash
cli list-users -e http://localhost:8081
```
