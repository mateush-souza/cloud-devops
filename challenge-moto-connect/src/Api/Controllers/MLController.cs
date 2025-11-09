using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace challenge_moto_connect.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class MLController : ControllerBase
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public MLController()
        {
            _mlContext = new MLContext(seed: 0);
            _model = TrainModel();
        }

        /// <summary>
        /// Prediz se uma motocicleta precisa de manutenção com base em dados de telemetria
        /// </summary>
        /// <param name="input">Dados de entrada para predição</param>
        /// <returns>Resultado da predição</returns>
        [HttpPost("predict-maintenance")]
        [ProducesResponseType(typeof(MaintenancePrediction), 200)]
        public ActionResult<MaintenancePrediction> PredictMaintenance([FromBody] MaintenanceInput input)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<MaintenanceInput, MaintenancePrediction>(_model);
            var prediction = predictionEngine.Predict(input);
            
            return Ok(new
            {
                needsMaintenance = prediction.NeedsMaintenance,
                probability = prediction.Probability,
                score = prediction.Score
            });
        }

        private ITransformer TrainModel()
        {
            // Dados de treinamento simulados
            var trainingData = new[]
            {
                new MaintenanceInput { Mileage = 5000, EngineTemperature = 85, OilPressure = 45, VibrationLevel = 2.5f, NeedsMaintenance = false },
                new MaintenanceInput { Mileage = 15000, EngineTemperature = 95, OilPressure = 35, VibrationLevel = 4.5f, NeedsMaintenance = true },
                new MaintenanceInput { Mileage = 8000, EngineTemperature = 88, OilPressure = 42, VibrationLevel = 3.0f, NeedsMaintenance = false },
                new MaintenanceInput { Mileage = 20000, EngineTemperature = 100, OilPressure = 30, VibrationLevel = 5.5f, NeedsMaintenance = true },
                new MaintenanceInput { Mileage = 3000, EngineTemperature = 82, OilPressure = 48, VibrationLevel = 2.0f, NeedsMaintenance = false },
                new MaintenanceInput { Mileage = 18000, EngineTemperature = 98, OilPressure = 32, VibrationLevel = 5.0f, NeedsMaintenance = true },
                new MaintenanceInput { Mileage = 10000, EngineTemperature = 90, OilPressure = 40, VibrationLevel = 3.5f, NeedsMaintenance = false },
                new MaintenanceInput { Mileage = 25000, EngineTemperature = 105, OilPressure = 28, VibrationLevel = 6.0f, NeedsMaintenance = true }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Concatenate("Features", 
                    nameof(MaintenanceInput.Mileage),
                    nameof(MaintenanceInput.EngineTemperature),
                    nameof(MaintenanceInput.OilPressure),
                    nameof(MaintenanceInput.VibrationLevel))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: nameof(MaintenanceInput.NeedsMaintenance),
                    featureColumnName: "Features"));

            return pipeline.Fit(dataView);
        }
    }

    public class MaintenanceInput
    {
        [LoadColumn(0)]
        public float Mileage { get; set; }

        [LoadColumn(1)]
        public float EngineTemperature { get; set; }

        [LoadColumn(2)]
        public float OilPressure { get; set; }

        [LoadColumn(3)]
        public float VibrationLevel { get; set; }

        [LoadColumn(4)]
        public bool NeedsMaintenance { get; set; }
    }

    public class MaintenancePrediction
    {
        [ColumnName("PredictedLabel")]
        public bool NeedsMaintenance { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
