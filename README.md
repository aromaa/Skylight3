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
- RELEASE9:
	- CryptoKey: LNZ6W3JPEI1zbs/Y7kE7MrpGgPhrDCH3Qk812EpRkfmzb+k4MVx7ohoutmDQXXKq/xOk0E9b8YCeGfzC2RQWLAH9LVtxWctQInBjUvMIWvAnEeboULStpHCj2ZuqKbuc1cewtLTsp4Afm9LQN8YF8xvQTg2OQFAVEhOv/H7CC75jXrj4p00tBY2ASCota1iMkx7489BSibVFsYDYGQPvs6CfgYcXPsBaW6x3/4cnTtgMvC3MXTYepYGyl/1cH8R+BEi2tNiQTv+55IZcZ40CkHuhZbuRu6s+FfQR58t4sFeWWfQHHRXrpVZ9uFro6JEPxqVnDPWxlx0tGrhbFBDnru3PpftyufVEUnTYAMtZ6q5k3DwqPGcRXdBI8nSUVOZzOIqGa8cRSTpLu8j9jfn2SsmmwpxI3RQGW7/zZANxTzuvXnBRRaaRWaNvtG6SnCvO+Ba8G3uYQYjUuVNookUV0HROwQKz3m1CSzguFWmM7A1OOh43cuRgnFmzdB4/BzQKthlXHaZLQFkebigyeWssl/aTgydp4zpCOFJrSVuF0squOGwddW2AZ+3jDYqxtJKOUlNzwpQ+SlyaX8Jo2AKmO5aJpDG9IewuUqlJTbFRQ2K1dDFMYczjHctxbvL/jC7MkCfqpx6WbtuKiFgMs0cXlulQ2fT4b0H/Rdk3MSvk4Qp7RymtBw/CCFfRS9SzkJcwhi9t1AgYQmbG0yO4mkyTqlr3NR+kBb0M0GO5NEqaietwhAUQQXxXbVOqJRRYhgJW2qnegMocV1Gax3zvgi9Y2z1hEl9RkHtAMe8YV4YYZuapkVMLfqbmlR+kXsUb4SMRGPGMESoKKHzZcnT86DdNWEsFMLTc2nxhsbjAzTs2WZhPBkAdp5s+DsW1Qo6ZW+YrYG56u+vRvvGAMhc1citvamMP6HNl0ur17qQ4e15932HSl1vMBEiMKY8TXdSZZrbzZl3WIETsklyoYy6W+SKxy2l+gSucpgOoK1G3g6hvg52bw8OxL7RSPeE+lrDUv4F1YkitwCTLD+D+NH+u5ybV73g0smVhhIKQmPviWhLpSilYHBE6sVTid/EZwAedfaq8v7pLYeFzuGSohQDcX6DyDrnb1mydjiCHRVZAWuyziUCA1j+EmLGnngh6i1lzCxtVXi0MpBKp1UrEPTc87iFNtVimPWCYi9Eq38uVGV1HhChNH7uoWNJq+7UdD57Ct7DmWwJ8rlalOWy/42qkn24jzfj+aYEZTQakXbDAzRpgbb8j7y58NdDdr6n2ROSeJ91C6qqaBsCEGQaoqRr7txfMwCJgfhS3hxTfc4n+9w1HB7CiuLj/gOXsaypQRHB/BDlZGk77sRWX4BrjcE7wC/dXZw==
- RELEASE39-200910220522-22363:
	- CryptoPrime: 71312178008343951072484563026069418184188158030081109761458956729410724992257
	- CryptoGenerator: 770272
	- CryptoKey: mWxFRJnGJ5T9Si0OMVvEBBm8laihXkN8GmH6fuv7ldZhLyGRRKCcGzziPYBaJom
	- CryptoPremix: NV6VVFPoC7FLDlzDUri3qcOAg9cRoFOmsYR9ffDGy5P8HfF6eekX40SFSVfJ1mDb3lcpYRqdg28sp61eHkPukKbqTu1JsVEKiRavi04YtSzUsLXaYSa5BEGwg5G2OF
