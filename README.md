# c# stack

A CRUD stack

- c# api (TODO)
- postgres backend

Tooling

- pgadmin

## Getting started

Config for pgadmin (automates registering the database with pgadmin):

```shell
./pgadmin-dev-setup/setup.sh
```

Start the stack:

```shell
docker-compose up
```

## Using PGAdmin

- Load it here: http://localhost:8080/
- Servers -> postgres -> type in password (as in the `.env` file)
