namespace challenge_moto_connect.Application.DTOs
{
    public class AnomalyDetectionResult
    {
        public bool IsAnomaly { get; set; }
        public float Score { get; set; }
        public string AlertLevel { get; set; }
    }
}

