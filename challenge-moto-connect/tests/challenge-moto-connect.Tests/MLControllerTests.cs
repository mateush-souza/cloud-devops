using challenge_moto_connect.Api.Controllers;
using challenge_moto_connect.Application.Services;
using challenge_moto_connect.Application.DTOs;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace challenge_moto_connect.Tests
{
    public class MLControllerTests
    {
        [Fact]
        public void PredictMaintenance_ReturnsOkResult()
        {
            var mockMLService = new Mock<IMLService>();
            mockMLService.Setup(x => x.PredictMaintenance(It.IsAny<MaintenanceInputDto>()))
                .Returns(new MaintenancePredictionResult 
                { 
                    NeedsMaintenance = true, 
                    Probability = 0.85f, 
                    Score = 0.95f,
                    Recommendation = "Manutenção recomendada"
                });

            var controller = new MLController(mockMLService.Object);
            var input = new MaintenanceInputDto
            {
                Mileage = 15000,
                EngineTemperature = 95,
                OilPressure = 35,
                VibrationLevel = 4.5f
            };

            var result = controller.PredictMaintenance(input);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var prediction = Assert.IsType<MaintenancePredictionResult>(okResult.Value);
            Assert.True(prediction.NeedsMaintenance);
            Assert.Equal(0.85f, prediction.Probability);
        }

        [Fact]
        public void PredictMaintenance_WithLowMileage_ReturnsLowMaintenanceProbability()
        {
            var mockMLService = new Mock<IMLService>();
            mockMLService.Setup(x => x.PredictMaintenance(It.IsAny<MaintenanceInputDto>()))
                .Returns(new MaintenancePredictionResult 
                { 
                    NeedsMaintenance = false, 
                    Probability = 0.15f, 
                    Score = 0.25f,
                    Recommendation = "Veículo em boas condições"
                });

            var controller = new MLController(mockMLService.Object);
            var input = new MaintenanceInputDto
            {
                Mileage = 3000,
                EngineTemperature = 82,
                OilPressure = 48,
                VibrationLevel = 2.0f
            };

            var result = controller.PredictMaintenance(input);
            var okResult = result.Result as OkObjectResult;

            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
            var prediction = Assert.IsType<MaintenancePredictionResult>(okResult.Value);
            Assert.False(prediction.NeedsMaintenance);
        }

        [Fact]
        public void DetectAnomaly_ReturnsOkResult()
        {
            var mockMLService = new Mock<IMLService>();
            mockMLService.Setup(x => x.DetectAnomaly(It.IsAny<TelemetryInputDto>()))
                .Returns(new AnomalyDetectionResult 
                { 
                    IsAnomaly = true, 
                    Score = 0.9f, 
                    AlertLevel = "Alto"
                });

            var controller = new MLController(mockMLService.Object);
            var input = new TelemetryInputDto 
            { 
                Speed = 120, 
                Temperature = 110, 
                Pressure = 20, 
                BatteryLevel = 10 
            };

            var result = controller.DetectAnomaly(input);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var detection = Assert.IsType<AnomalyDetectionResult>(okResult.Value);
            Assert.True(detection.IsAnomaly);
        }

        [Fact]
        public void PredictMileage_ReturnsOkResult()
        {
            var mockMLService = new Mock<IMLService>();
            mockMLService.Setup(x => x.PredictMileage(It.IsAny<VehicleDataDto>()))
                .Returns(new RegressionResult 
                { 
                    PredictedMileage = 15000, 
                    ConfidenceInterval = 1500 
                });

            var controller = new MLController(mockMLService.Object);
            var input = new VehicleDataDto 
            { 
                CurrentMileage = 10000, 
                VehicleAge = 2, 
                AverageSpeed = 60, 
                MaintenanceCount = 3 
            };

            var result = controller.PredictMileage(input);

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var regression = Assert.IsType<RegressionResult>(okResult.Value);
            Assert.Equal(15000, regression.PredictedMileage);
        }

        [Fact]
        public void GetMetrics_ReturnsOkResult()
        {
            var mockMLService = new Mock<IMLService>();
            mockMLService.Setup(x => x.GetModelMetrics())
                .Returns(new ModelMetrics 
                { 
                    Accuracy = 0.87, 
                    Precision = 0.85, 
                    Recall = 0.89,
                    F1Score = 0.87,
                    TotalPredictions = 100,
                    LastTrainingDate = DateTime.UtcNow
                });

            var controller = new MLController(mockMLService.Object);

            var result = controller.GetMetrics();

            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var metrics = Assert.IsType<ModelMetrics>(okResult.Value);
            Assert.Equal(0.87, metrics.Accuracy);
        }
    }
}
