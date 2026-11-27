using CloneUZ.Domain.DTOs;
using CloneUZ.Domain.Models;

namespace CloneUZ.BLL.Mappers;

public static class RouteMapper
{
    public static RouteDTO ToDTO(this Route route)
    {
        RouteDTO routesDTO = new RouteDTO()
        {
            Name = route.Name,
            Sations = new List<RouteStationDTO>()
        };
        foreach (var routestation in route.Sations)
        {
            routesDTO.Sations.Add(routestation.ToDTO());
        }
        return routesDTO;
    }

    public static List<RouteDTO> ListToDTO(this List<Route> routes)
    {
        List<RouteDTO> listDTO = new  List<RouteDTO>();
        foreach (var route in routes)
        {
            listDTO.Add(route.ToDTO());
        }
        return listDTO;
    }
}