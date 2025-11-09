using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.DTOs.Pagination;
using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace challenge_moto_connect.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IRepository<Vehicle> _vehicleRepository;

        public VehicleService(IRepository<Vehicle> vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<IEnumerable<VehicleDTO>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return vehicles.Select(v => new VehicleDTO
            {
                VehicleId = v.VehicleId,
                LicensePlate = v.LicensePlate,
                VehicleModel = v.VehicleModel.ToString()
            });
        }

        public async Task<VehicleDTO> GetVehicleByIdAsync(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return null;

            return new VehicleDTO
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VehicleModel = vehicle.VehicleModel.ToString()
            };
        }

        public async Task<VehicleDTO> CreateVehicleAsync(VehicleDTO vehicleDto)
        {
            var vehicle = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                LicensePlate = vehicleDto.LicensePlate,
                VehicleModel = (VehicleModel)Enum.Parse(typeof(VehicleModel), vehicleDto.VehicleModel)
            };
            await _vehicleRepository.AddAsync(vehicle);
            return new VehicleDTO
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VehicleModel = vehicle.VehicleModel.ToString()
            };
        }

        public async Task UpdateVehicleAsync(Guid id, VehicleDTO vehicleDto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) throw new KeyNotFoundException("Vehicle not found.");

            vehicle.LicensePlate = vehicleDto.LicensePlate;
            vehicle.VehicleModel = (VehicleModel)Enum.Parse(typeof(VehicleModel), vehicleDto.VehicleModel);

            await _vehicleRepository.UpdateAsync(vehicle);
        }

        public async Task DeleteVehicleAsync(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) throw new KeyNotFoundException("Vehicle not found.");

            await _vehicleRepository.DeleteAsync(vehicle.VehicleId);
        }

        public async Task<PagedListDto<VehicleDTO>> GetPagedVehiclesAsync(PaginationParams paginationParams)
        {
            var vehicles = _vehicleRepository.GetAllAsQueryable();
            var pagedVehicles = PagedListDto<Vehicle>.ToPagedList(vehicles, paginationParams.PageNumber, paginationParams.PageSize);

            var vehicleDtos = pagedVehicles.Items.Select(v => new VehicleDTO
            {
                VehicleId = v.VehicleId,
                LicensePlate = v.LicensePlate,
                VehicleModel = v.VehicleModel.ToString()
            }).ToList();

            return new PagedListDto<VehicleDTO>(vehicleDtos, pagedVehicles.TotalCount, pagedVehicles.CurrentPage, pagedVehicles.PageSize);
        }
    }
}

