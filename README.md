# ModularMonolith

This is sample repository using modular monolith approach separate parts of code and allow
easier maintainability and potential switch to microservices.
Apart from splitting code into modules, project also uses vertical slices within each module
to allow for more elastic approach to define layering. Additionally dependencies are created
using ports & adapters approach.

## Makefile

Project uses [GNU Make](https://www.gnu.org/software/make/manual/make.html) to help with
setup and running different commands, such as adding migrations and seeding database in a
OS independent way.

### Installing Make

#### Linux

In order to install `Make` on Ubuntu Linux run following command:

`apt install make`

#### Windows

To install `Make` on Windows run following command:

`choco install make`

### Running Make targets

To run target from `Makefile` use following command schema:

`make <target> <variable_name>=<variable_value>`

where:

- `target` - indicates which target from makefile to run
- `<variable_name>` - is the name of the variable required by target
- `<variable_value>` - is value of the variable that target will use

## Project startup

To start up the project a few requirements need to be met:

- [Install .NET 8 SDK](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net80)
- [Install Docker](https://www.docker.com/products/docker-desktop/)
- [Dotnet EF Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

### Starting dependencies

Before project can be started all the dependencies should be started, in order to do that
run target: `start-infrastructure` from `Makefile` like so:

`make start-infrastructure`

This should start docker container with [PostgreSQL](https://www.postgresql.org/) database exposed over
port `5432`.

### Database migration

To keep database up to date with project Entity Framework Migrations are used,
database should be migrated to the newest version after checkout in order to do that
run `update-database` target from `Makefile`:

`make update-database`

### Database seed

When database is first initialized it will be empty, in order to have some useful data
to work with a data seed files are prepared. To put initial test data into the database
run following `Makefile` target: `seed-database`

`make seed-database`

## Git

Project follows conventional commit approach and uses [Pre Commit](https://pre-commit.com/#install) with
[Conventional Commit Plugin](https://github.com/compilerla/conventional-pre-commit).

After `pre-commit` is installed on your platform make sure to run:

`pre-commit install`

## CI & CD

TODO: add github actions description..
