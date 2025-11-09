using System;
using challenge_moto_connect.Application.DTOs.HATEOAS;

namespace challenge_moto_connect.Application.DTOs
{
    public class VehicleDTO : BaseDto
    {
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleModel { get; set; }
    }
}

