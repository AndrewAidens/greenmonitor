namespace greenmonitor.Models
{
    public class SensorReading
    {
        public int Id { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
