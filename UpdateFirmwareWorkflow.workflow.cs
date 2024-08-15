namespace DbExample;

using Temporalio.Workflows;
using Demo;

[Workflow]
public class UpdateFirmwareWorkflow
{
    [WorkflowRun]
    public async Task<int> RunAsync()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("== Starting Workflow ==");
        Console.ResetColor();
        
        List<Device> devices = await Workflow.ExecuteActivityAsync(
            (Activities act) => act.GetDevices(),
            new() { ScheduleToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        foreach (Device device in devices) {
            await Workflow.ExecuteActivityAsync(
                (Activities act) => act.UpdateFirmware(device),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            await Workflow.ExecuteActivityAsync(
                (Activities act) => act.UpdateDevice(device),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }

        Console.WriteLine("Finished: Updated " + devices.Count + " devices.");

        return devices.Count;
    }
}