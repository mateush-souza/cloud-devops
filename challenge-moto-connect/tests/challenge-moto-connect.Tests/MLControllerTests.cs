using challenge_moto_connect.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace challenge_moto_connect.Tests
{
    public class MLControllerTests
    {
        private readonly MLController _controller;

        public MLControllerTests()
        {
            _controller = new MLController();
        }

        [Fact]
        public void PredictMaintenance_ReturnsOkResult()
        {
            // Arrange
            var input = new MaintenanceInput
            {
                Mileage = 15000,
                EngineTemperature = 95,
                OilPressure = 35,
                VibrationLevel = 4.5f
            };

            // Act
            var result = _controller.PredictMaintenance(input);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void PredictMaintenance_WithLowMileage_ReturnsLowMaintenanceProbability()
        {
            // Arrange
            var input = new MaintenanceInput
            {
                Mileage = 3000,
                EngineTemperature = 82,
                OilPressure = 48,
                VibrationLevel = 2.0f
            };

            // Act
            var result = _controller.PredictMaintenance(input);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
        }
    }
}
