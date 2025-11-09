using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using challenge_moto_connect.Application.Services;
using challenge_moto_connect.Application.DTOs;

namespace challenge_moto_connect.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize]
    public class MLController : ControllerBase
    {
        private readonly IMLService _mlService;

        public MLController(IMLService mlService)
        {
            _mlService = mlService;
        }

        [HttpPost("predict-maintenance")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(MaintenancePredictionResult), 200)]
        [ProducesResponseType(400)]
        public ActionResult<MaintenancePredictionResult> PredictMaintenance([FromBody] MaintenanceInputDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dados de entrada inválidos" });
            }

            try
            {
                var result = _mlService.PredictMaintenance(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
        }
        }

        [HttpPost("detect-anomaly")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(AnomalyDetectionResult), 200)]
        [ProducesResponseType(400)]
        public ActionResult<AnomalyDetectionResult> DetectAnomaly([FromBody] TelemetryInputDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dados de entrada inválidos" });
            }

            try
            {
                var result = _mlService.DetectAnomaly(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("predict-mileage")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RegressionResult), 200)]
        [ProducesResponseType(400)]
        public ActionResult<RegressionResult> PredictMileage([FromBody] VehicleDataDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dados de entrada inválidos" });
            }

            try
            {
                var result = _mlService.PredictMileage(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("metrics")]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(ModelMetrics), 200)]
        public ActionResult<ModelMetrics> GetMetrics()
        {
            try
            {
                var metrics = _mlService.GetModelMetrics();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("v2/predict-maintenance-enhanced")]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(MaintenancePredictionResult), 200)]
        [ProducesResponseType(400)]
        public ActionResult<MaintenancePredictionResult> PredictMaintenanceV2([FromBody] MaintenanceInputDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dados de entrada inválidos" });
            }

            try
            {
                var result = _mlService.PredictMaintenance(input);
                result.Recommendation += " (API v2.0 - Enhanced)";
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
