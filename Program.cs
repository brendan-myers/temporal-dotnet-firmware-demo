using System.CommandLine;
using System.CommandLine.Invocation;
using DbExample;
using Temporalio.Client;
using Temporalio.Worker;
using MySqlConnector;

var rootCommand = new RootCommand("Firmware update sample");

// Helper for client commands
void AddClientCommand(
    string name,
    string desc,
    Func<ITemporalClient, InvocationContext, CancellationToken, Task> func)
{
    var cmd = new Command(name, desc);
    rootCommand!.AddCommand(cmd);

    var temporalAddress = "<namespace>.<account_id>.tmprl.cloud:7233";
    var temporalNamespace = "<namespace>.<account_id>";
    var temporalCertPath = "<path>/<to>/client.pem";
    var temporalKeyPath = "<path>/<to>/client.key";

    // Set handler
    cmd.SetHandler(async ctx =>
    {
        // Create client
        var clientOptions = new TemporalClientConnectOptions(temporalAddress)
        {
            Namespace = temporalNamespace!,
        };

        if (!string.IsNullOrEmpty(temporalCertPath) && !string.IsNullOrEmpty(temporalKeyPath))
        {
            clientOptions.Tls = new()
            {
                ClientCert = File.ReadAllBytes(temporalCertPath),
                ClientPrivateKey = File.ReadAllBytes(temporalKeyPath),
            };
        }

        var client = await TemporalClient.ConnectAsync(clientOptions);

        // Run
        await func(client, ctx, ctx.GetCancellationToken());
    });
}

AddClientCommand("run-worker", "Run worker", async (client, ctx, cancelToken) =>
{
    // Cancellation token to shutdown worker on ctrl+c
    using var tokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        tokenSource.Cancel();
        eventArgs.Cancel = true;
    };

    // Create an activity instance since we have instance activities. If we had
    // all static activities, we could just reference those directly.
    var activities = new Activities();

    // Create worker with the activity and workflow registered
    using var worker = new TemporalWorker(
        client,
        new TemporalWorkerOptions("updatefirmware-task-queue").
            AddActivity(activities.GetDevices).
            AddActivity(activities.UpdateFirmware).
            AddActivity(activities.UpdateDevice).
            AddWorkflow<UpdateFirmwareWorkflow>());

    // Run worker until cancelled
    Console.WriteLine("Running worker");
    try
    {
        await worker.ExecuteAsync(tokenSource.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Worker cancelled");
    }
});

AddClientCommand("execute-workflow", "Execute Workflow", async (client, ctx, cancelToken) =>
{
    // Run workflow
    var result = await client.ExecuteWorkflowAsync(
        (UpdateFirmwareWorkflow wf) => wf.RunAsync(),
        new(id: "updatefirmware-workflow-id", taskQueue: "updatefirmware-task-queue"));

    Console.WriteLine("Devices updated: {0}", result);
});

// Run
await rootCommand.InvokeAsync(args);