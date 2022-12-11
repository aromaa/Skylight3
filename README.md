# Skylight
Implementation for Skylight.Protocol with focus on high performance.

## Features
- Catalog
	- Show catalog pages and offers
	- Purchase items
	- FurniMatic
- Items
	- Load inventory
	- Place room items
	- Pickup room items
	- Move items
	- Item interactions
		- Sticky note
		- Sticky note pole
		- FurniMatic gift
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

## Configuration
The configuration uses the standard [IConfiguration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) for all of its basic settings. Additionally, the `settings` table is read from the database.

Typical appsettings.json would look something like:
```json
{
  "Database": {
    "ConnectionString": "Host=localhost;User ID=<USER>;Password=<PASSWORD>;Database=skylight"
  },
  "Network": {
	  "Listeners":
	  [
		  {
			  "EndPoint": "127.0.0.1:30000",
			  "Revision": "WIN63-202111081545-75921380"
		  }
	  ]
  }
}
```

Note: By default, no revisions are included with the server. Instead on load the `Protocol` folder is read to discover which revisions are provided. Developer note: When running the server in debug mode the app domain is scanned for additional revisions.

All of the configuration nobs:
- Database
	- ConnectionString - The connection string for Postgres
- FurniMatic
	- ItemsRequired - How many items are required to recyclate?
	- GiftFurnitureId - The furniture id used for the gift
- Network
	- Listeners
		- EndPoint - The end point that accepts the connection
		- Revision - The revision that is required.
		- CryptoPrime - The RC4 prime for encryption, if used.
		- CryptoGenerator - The RC4 generator for encryption, if used.
- Redis
	- ConnectionString - The connection string for Redis

## RC4 encryption
- RELEASE39-200910220522-22363:
	- Prime: 71312178008343951072484563026069418184188158030081109761458956729410724992257
	- Generator: 770272