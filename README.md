# Skylight
Implementation for Skylight.Protocol with focus on high performance.

## Features
- Badges
- Catalog
	- Show catalog pages and offers
	- Purchase items
	- Purchase badges
	- FurniMatic
- Items
	- Load inventory
	- Place room items
	- Pickup room items
	- Move items
	- Item interactions
		- FurniMatic gift
		- Sticky note
		- Sticky note pole
		- Traxmachine
- Navigator
	- Create room
	- Show own rooms
- Networking
	- Support multiple revisions
	- Implements full encryption with RC4 for older clients
- Room
	- Chatting
	- Pathfinding in 3D (Users can move under items)
- User
	- Load by SSO ticket

## Requirements
The server is written in C# and requires .NET 7 to be run. Additionally Redis is used for SSO login and Postgres as the database.

## Setup
1. Clone the repository recursively. `git clone --recursive https://github.com/aromaa/Skylight3.git`
2. If you do not have [dotnet ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) tool installed already, you can install it using `dotnet tool install --global dotnet-ef`
3. To get the initial database run `dotnet ef dbcontext script` on `src/Skylight.Infrastructure`
4. Open the project in Visual Studio and set the `Skylight.Bootstrap` as the startup project if it isn't already.
5. Upon launching the project from Visual Studio, protocols defined with `ProtocolReference` inside `Skylight.Bootstrap.csproj` are automatically compiled and moved to the `Protocol` folder.

## Configuration
The configuration uses the standard [IConfiguration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) for all of its basic settings. Additionally, the `settings` table is read from the database which has the highest priority.

Typical appsettings.json would look something like:
```json
{
  "Database": {
    "ConnectionString": "Host=localhost;User ID=<USER>;Password=<PASSWORD>;Database=skylight"
  },
  "Network": {
    "Listeners": [
      {
        "EndPoints": ["127.0.0.1:30000"],
        "Revision": "WIN63-202111081545-75921380"
      }
    ]
  }
}
```

Note: By default, no revisions are included with the server. Instead on load the `Protocol` folder is read to discover which revisions are provided. Developer note: When running the server in debug mode the app domain is scanned for additional revisions.

All of the configuration nobs:
- Database
	- ConnectionString - The connection string for Postgres.
- FurniMatic
	- ItemsRequired - How many items are required to recyclate.
	- GiftFurnitureId - The furniture id used for the gift.
- Network
	- EarlyBind - Whatever to bind listeners as soon as possible or after the initialization. Allows the client to establish the connection early to prepare for authentication. If initialization fails the connections are ungracefully dropped.
	- EarlyAccept - Whatever to let users in before the initialization has finished. If initialization fails the server is ungracefully killed and no state will be saved. Has no effect if EarlyBind is false.
	- Listeners
		- EndPoints - The end points that accept the connection.
		- Revision - The revision that is required.
		- CryptoPrime - The RC4 prime for encryption, if used.
		- CryptoGenerator - The RC4 generator for encryption, if used.
- Redis
	- ConnectionString - The connection string for Redis.

## RC4 encryption
- RELEASE39-200910220522-22363:
	- Prime: 71312178008343951072484563026069418184188158030081109761458956729410724992257
	- Generator: 770272
