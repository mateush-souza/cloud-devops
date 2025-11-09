using challenge_moto_connect.Application.DTOs;

namespace challenge_moto_connect.Application.Services
{
    public interface IMLService
    {
        MaintenancePredictionResult PredictMaintenance(MaintenanceInputDto input);
        AnomalyDetectionResult DetectAnomaly(TelemetryInputDto input);
        RegressionResult PredictMileage(VehicleDataDto input);
        ModelMetrics GetModelMetrics();
    }
}

