using System;
using challenge_moto_connect.Application.DTOs.HATEOAS;

namespace challenge_moto_connect.Application.DTOs
{
    public class HistoryDTO : BaseDto
    {
        public Guid MaintenanceHistoryID { get; set; }
        public string Description { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public Guid VehicleID { get; set; }
        public Guid UserID { get; set; }
    }
}

