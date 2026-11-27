using CloneUZ.Domain.DTOs;
using CloneUZ.Domain.Models;

namespace CloneUZ.BLL.Mappers;

public static class RouteStationMapper
{
    public static RouteStationDTO ToDTO(this RouteStation routeStation)
    {
        return new RouteStationDTO()
        {
            StationOrder = routeStation.StationOrder,
            ArrivalTime = routeStation.ArrivalTime,
            DepartTime = routeStation.DepartTime,
            Station = routeStation.Station.ToDTO()
        };
    }
}