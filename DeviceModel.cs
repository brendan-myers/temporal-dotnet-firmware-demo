namespace Demo {
    public class Device
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int FirmwareVersion { get; set; }
        public int NetworkQuality { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}