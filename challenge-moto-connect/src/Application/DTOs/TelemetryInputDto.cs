namespace challenge_moto_connect.Application.DTOs
{
    public class TelemetryInputDto
    {
        public float Speed { get; set; }
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float BatteryLevel { get; set; }
    }
}

