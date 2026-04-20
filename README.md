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
- RELEASE4:
	- CryptoKey: bLPYQrOR6XJyXTXYb8r3n2v/ojU=
- RELEASE5:
	- CryptoKey: LDKRLDtvBVJS8x1I69gfmBYsy1s=
- RELEASE9:
	- CryptoKey: FC3eYws210fPqnhzrd7GGjwElw4
- RELEASE13:
	- CryptoKey: zDVKbT8Eo7bSuhOioHOLU+uxDg8=
	- CryptoPremix: eb11nmhdwbn733c2xjv1qln3ukpe0hvce0ylr02s12sv96rus2ohexr9cp8rufbmb1mdb732j1l3kehc0l0s2v6u2hx9prfmu
- RELEASE17:
	- CryptoPrime: 31375515601038227148494783618302639307515885356083124006144440838597245774509
	- CryptoGenerator: 28485078384723543041179727418344406162853710924878637158263652502777350724187
- RELEASE24:
	- CryptoPrime: 31375515601038227148494783618302639307515885356083124006144440838597245774509
	- CryptoGenerator: 28485078384723543041179727418344406162853710924878637158263652502777350724187
	- CryptoPremix: 1wz8rzgiv87708l9oi7ot8l9smdqv5yvzz8tavkyuoi9p3kgrrq7r5p53kchnb5hly8jkfx5hsoo6imx8o5ktczwdst8dooa7r331wkrw8zi8789io89mq5vztvyo93gr755khbhyjf5soixokcws8oar3wr
	- DecodePremix: wR9uuSLoPgt8pkCABkoZ
- RELEASE26:
	- CryptoPrime: 12239237269146396026179897667783351122333919537046572006472060412016570512431322289929475765209144234568134979837434236414318415399499279521121271977457797
	- CryptoGenerator: 664612156617898553177
	- CryptoPremix: NV6VVFPoC7FLDlzDUri3qcOAg9cRoFOmsYR9ffDGy5P8HfF6eekX40SFSVfJ1mDb3lcpYRqdg28sp61eHkPukKbqTu1JsVEKiRavi04YtSzUsLXaYSa5BEGwg5G2OF
	- CryptoKey: mWxFRJnGJ5T9Si0OMVvEBBm8laihXkN8GmH6fuv7ldZhLyGRRKCcGzziPYBaJom
	- DecodePremix: xllVGKnnQcW8aX4WefdKrBWTqiW5EwT
- RELEASE28:
	- CryptoPrime: 23507861061577827078506796680619296181342826999765525650671776868866473316022640576354207087394080900560760017185789
	- CryptoGenerator: 196659437071908510846941267302
	- CryptoPremix: NV6VVFPoC7FLDlzDUri3qcOAg9cRoFOmsYR9ffDGy5P8HfF6eekX40SFSVfJ1mDb3lcpYRqdg28sp61eHkPukKbqTu1JsVEKiRavi04YtSzUsLXaYSa5BEGwg5G2OF
	- CryptoKey: mWxFRJnGJ5T9Si0OMVvEBBm8laihXkN8GmH6fuv7ldZhLyGRRKCcGzziPYBaJom
- RELEASE33:
	- CryptoPrime: 34520025778538930063401503513286028577167961316483456956404976613915899391683685854596885023279130621535004911763459
	- CryptoGenerator: 13150786912187206565647417016191799895
	- CryptoPremix: NV6VVFPoC7FLDlzDUri3qcOAg9cRoFOmsYR9ffDGy5P8HfF6eekX40SFSVfJ1mDb3lcpYRqdg28sp61eHkPukKbqTu1JsVEKiRavi04YtSzUsLXaYSa5BEGwg5G2OF
	- CryptoKey: mWxFRJnGJ5T9Si0OMVvEBBm8laihXkN8GmH6fuv7ldZhLyGRRKCcGzziPYBaJom
- RELEASE36:
	- CryptoPrime: 109977927854599377304169717651738738265759536230681243029384217544356163595949
	- CryptoGenerator: 6577275250642772959516802234
	- CryptoPremix: NV6VVFPoC7FLDlzDUri3qcOAg9cRoFOmsYR9ffDGy5P8HfF6eekX40SFSVfJ1mDb3lcpYRqdg28sp61eHkPukKbqTu1JsVEKiRavi04YtSzUsLXaYSa5BEGwg5G2OF
	- CryptoKey: mWxFRJnGJ5T9Si0OMVvEBBm8laihXkN8GmH6fuv7ldZhLyGRRKCcGzziPYBaJom
- RELEASE39-200910220522-22363:
	- CryptoPrime: 71312178008343951072484563026069418184188158030081109761458956729410724992257
	- CryptoGenerator: 770272
	- CryptoKey: mWxFRJnGJ5T9Si0OMVvEBBm8laihXkN8GmH6fuv7ldZhLyGRRKCcGzziPYBaJom
	- CryptoPremix: NV6VVFPoC7FLDlzDUri3qcOAg9cRoFOmsYR9ffDGy5P8HfF6eekX40SFSVfJ1mDb3lcpYRqdg28sp61eHkPukKbqTu1JsVEKiRavi04YtSzUsLXaYSa5BEGwg5G2OF
