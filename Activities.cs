namespace DbExample;

using Temporalio.Activities;
using Temporalio.Exceptions;
using Demo;
using MySqlConnector;

public class Activities
{
    private string connectionString = "server=localhost;user=root;database=demo;port=3309;password=password";
        
    [Activity]
    public async Task<List<Device>> GetDevices() {
        Console.WriteLine("Fetching devices");

        List<Device> devices = new List<Device>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM device;";

        using var reader = await command.ExecuteReaderAsync();

        while (reader.Read())
        {
            Device device = new Device();

            device.Id = reader.GetInt32("id");
            device.Description = reader.GetString("description");
            device.FirmwareVersion = reader.GetInt32("firmware_version");
            device.NetworkQuality = reader.GetInt32("network_quality");
            device.UpdatedAt = reader.GetDateTime("updated_at");
            
            devices.Add(device);
        }

        return devices;
    }

    [Activity]
    public void UpdateFirmware(Device device) {
        Console.WriteLine("Updating Firmware: " + device.Description +
                            ", Network quality: " + device.NetworkQuality +
                            ", Firmware version: " + device.FirmwareVersion +
                            ", Last updated: " + device.UpdatedAt);

        // Simulate a lossy network
        Random random = new Random();
        int randomNumber = random.Next(0,100);

        if (randomNumber > device.NetworkQuality) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(device.Description + " : network failure");
            Console.ResetColor();
            throw new Exception("Network error");
        }

        Console.WriteLine(device.Description + " updated");
    }

    [Activity]
    public async Task UpdateDevice(Device device) {
        Console.WriteLine("Updating record: " + device.Description);

        // update the firmware
        int FirmwareVersion = device.FirmwareVersion + 1;

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE device set firmware_version = @FirmwareVersion WHERE id = @Id;";
        command.Parameters.AddWithValue("@FirmwareVersion", FirmwareVersion);
        command.Parameters.AddWithValue("@Id", device.Id);
        using var reader = await command.ExecuteReaderAsync();

        Console.WriteLine(device.Description + " updated");
    }
}