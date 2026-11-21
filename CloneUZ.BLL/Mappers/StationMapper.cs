using CloneUZ.Domain.DTOs;
using CloneUZ.Domain.Models;

namespace CloneUZ.BLL.Mappers;

public static class StationMapper
{
    public static StationDTO ToDTO(this Station station)
    {
        return new StationDTO()
        {
            City = station.City,
            Name = station.Name
        };
    }
}