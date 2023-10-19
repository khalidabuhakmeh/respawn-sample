# Respawn and xUnit Sample

This repository shows how you can use Respawn to manage a test database across multiple tests. Rather than creating a brand-new database for each test, you can have Respawn reset tables with each test. This helps speed up tests.

I've included everything you need to get started.

You'll need the following:

- .NET SDK 8 (using C# 12 features)
- Docker for Desktop

## Getting Started

Run the [`docker-compose.yml`](./docker-compose.yml) file to create an instance of MS SQL Server, then run the tests. Cheers.

You can also feel free to downgrade the project to a lower version of the .NET SDK, but currently I'm using .NET 8 and C# 12 features.