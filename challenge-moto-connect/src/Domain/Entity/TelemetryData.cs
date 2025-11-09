using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace challenge_moto_connect.Domain.Entity
{
    public class TelemetryData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Dados de simulação IoT (ex: localização, velocidade, nível de bateria)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmH { get; set; }
        public int BatteryLevel { get; set; } // 0-100

        // Dados de simulação Visão Computacional (ex: status de estacionamento)
        public string Status { get; set; } // Ex: "Estacionada", "Em Movimento", "Fora da Área"
    }
}
