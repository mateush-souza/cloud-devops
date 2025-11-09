namespace challenge_moto_connect.Application.DTOs
{
    public class ModelMetrics
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public int TotalPredictions { get; set; }
        public DateTime LastTrainingDate { get; set; }
    }
}

