using challenge_moto_connect.Application.DTOs;
using challenge_moto_connect.Application.DTOs.Pagination;
using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge_moto_connect.Application.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IRepository<History> _historyRepository;

        public HistoryService(IRepository<History> historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<IEnumerable<HistoryDTO>> GetAllHistoriesAsync()
        {
            var histories = await _historyRepository.GetAllAsync();
            return histories.Select(h => new HistoryDTO
            {
                MaintenanceHistoryID = h.MaintenanceHistoryID,
                Description = h.Description,
                MaintenanceDate = h.MaintenanceDate,
                VehicleID = h.VehicleID,
                UserID = h.UserID
            });
        }

        public async Task<HistoryDTO> GetHistoryByIdAsync(Guid id)
        {
            var history = await _historyRepository.GetByIdAsync(id);
            if (history == null) return null;

            return new HistoryDTO
            {
                MaintenanceHistoryID = history.MaintenanceHistoryID,
                Description = history.Description,
                MaintenanceDate = history.MaintenanceDate,
                VehicleID = history.VehicleID,
                UserID = history.UserID
            };
        }

        public async Task<HistoryDTO> CreateHistoryAsync(HistoryDTO historyDto)
        {
            var history = new History
            {
                MaintenanceHistoryID = Guid.NewGuid(),
                Description = historyDto.Description,
                MaintenanceDate = historyDto.MaintenanceDate,
                VehicleID = historyDto.VehicleID,
                UserID = historyDto.UserID
            };
            await _historyRepository.AddAsync(history);
            return historyDto;
        }

        public async Task UpdateHistoryAsync(Guid id, HistoryDTO historyDto)
        {
            var history = await _historyRepository.GetByIdAsync(id);
            if (history == null) throw new KeyNotFoundException("History not found.");

            history.Description = historyDto.Description;
            history.MaintenanceDate = historyDto.MaintenanceDate;
            history.VehicleID = historyDto.VehicleID;
            history.UserID = historyDto.UserID;

            await _historyRepository.UpdateAsync(history);
        }

        public async Task DeleteHistoryAsync(Guid id)
        {
            var history = await _historyRepository.GetByIdAsync(id);
            if (history == null) throw new KeyNotFoundException("History not found.");

            await _historyRepository.DeleteAsync(history.MaintenanceHistoryID);
        }

        public async Task<PagedListDto<HistoryDTO>> GetPagedHistoriesAsync(PaginationParams paginationParams)
        {
            var histories = _historyRepository.GetAllAsQueryable();
            var pagedHistories = PagedListDto<History>.ToPagedList(histories, paginationParams.PageNumber, paginationParams.PageSize);

            var historyDtos = pagedHistories.Items.Select(h => new HistoryDTO
            {
                MaintenanceHistoryID = h.MaintenanceHistoryID,
                Description = h.Description,
                MaintenanceDate = h.MaintenanceDate,
                VehicleID = h.VehicleID,
                UserID = h.UserID
            }).ToList();

            return new PagedListDto<HistoryDTO>(historyDtos, pagedHistories.TotalCount, pagedHistories.CurrentPage, pagedHistories.PageSize);
        }
    }
}

