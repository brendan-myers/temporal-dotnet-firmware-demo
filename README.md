# Temporal .NET DB/Firmware Update Demo

In `Program.cs` add your Temporal Cloud namespace details

```
var temporalAddress = "<namespace>.<account_id>.tmprl.cloud:7233";
var temporalNamespace = "<namespace>.<account_id>";
var temporalCertPath = "<path>/<to>/client.pem";
var temporalKeyPath = "<path>/<to>/client.key";
```

In `Activities.cs` replace the very secure `connectionString` with the connection details for your database.

`db.txt` contains the db schema that this demo expects, and some sample data.

## How to run

In one terminal, run the Temporal worker

```
dotnet run run-worker
```

In another terminal, execute the workflow

```
dotnet run execute-workflow
```