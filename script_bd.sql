CREATE TABLE TelemetryData (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    VehicleId UNIQUEIDENTIFIER NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL,
    SpeedKmH FLOAT NOT NULL,
    BatteryLevel INT NOT NULL,
    Status NVARCHAR(255)
);
-- Adicionar chave estrangeira para Vehicle
-- ALTER TABLE TelemetryData ADD CONSTRAINT FK_TelemetryData_Vehicle FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId);

