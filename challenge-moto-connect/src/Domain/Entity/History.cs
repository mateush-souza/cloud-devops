namespace challenge_moto_connect.Domain.Entity;
    public class History
    {
        public Guid MaintenanceHistoryID { get; set; }
        public Guid VehicleID { get; set; }
        public Guid UserID { get; set; }

        public StatusModel StatusModel { get; set; } = StatusModel.NAO_INICIADO;

        public DateTime MaintenanceDate { get; set; }

        public string Description { get; set; }

        public History() { }
        public History(Guid maintenanceHistoryID, Guid userID, Guid vehicleID, DateTime maintenanceDate, StatusModel statusModel, string description)
        {
            if (userID == Guid.Empty)
                throw new ArgumentNullException(nameof(userID), "User não pode ser nulo.");
            if (vehicleID == Guid.Empty)
                throw new ArgumentNullException(nameof(vehicleID), "Vehicle não pode ser nulo.");

            MaintenanceHistoryID = maintenanceHistoryID;
            StatusModel = statusModel;
            UserID = userID; 
            VehicleID = vehicleID; 

            if (maintenanceDate == default || maintenanceDate == DateTime.MinValue)
                MaintenanceDate = DateTime.Now;
            else
                MaintenanceDate = maintenanceDate;

            Description = description ?? "";

        }

        public void ValidateForSave()
        {
            if (UserID == Guid.Empty)
                throw new ArgumentNullException(nameof(UserID), "User não pode ser nulo.");
            if (VehicleID == Guid.Empty)
                throw new ArgumentNullException(nameof(VehicleID), "Vehicle não pode ser nulo.");
            if (string.IsNullOrEmpty(Description))
                throw new ArgumentNullException(nameof(Description), "Description não pode ser nulo.");
            if (MaintenanceDate == default || MaintenanceDate == DateTime.MinValue)
                throw new ArgumentException("A data da manutenção está em formato inválido.");
        }

        private bool IsValidDate(DateTime date)
        {
            return date != default && date != DateTime.MinValue;
        }
    }



