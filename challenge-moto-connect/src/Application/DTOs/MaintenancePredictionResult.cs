namespace challenge_moto_connect.Application.DTOs
{
    public class MaintenancePredictionResult
    {
        public bool NeedsMaintenance { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
        public string Recommendation { get; set; }
    }
}

