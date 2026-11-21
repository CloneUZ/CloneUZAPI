using CloneUZ.BLL.Interfaces;
using CloneUZ.DAL.Interfaces;
using CloneUZ.Domain.DTOs;
using CloneUZ.Domain.Models;

namespace CloneUZ.BLL.Services;

public class RouteService:IRouteService
{
    readonly private IRouteRepository _repository;
    public RouteService(IRouteRepository repository)
    {
        _repository = repository;
    }
    
    private int GetDaysInRun(List<Route> ClientRoutes)
    {
        int daysInRun = 0;
        foreach (var route in ClientRoutes)
        {
            var currentStation = route.Sations.FirstOrDefault();
            foreach (var nextStation in route.Sations)
            {
                if(currentStation.Id == nextStation.Id) continue;
                if(nextStation.ArrivalTime > nextStation.DepartTime ) daysInRun++;
                if (currentStation.DepartTime > nextStation.ArrivalTime) daysInRun++;
                currentStation = nextStation;
            }
        }
        return daysInRun;
    }
    private  Route GetClientRoutePart(int FromOrder, int ToOrder, Route OriginRoute)
    {
        return new Route
        {
            Id = OriginRoute.Id,
            Name = OriginRoute.Name,
            // Робимо копію списку станцій тільки для цього відрізка
            Sations = OriginRoute.Sations
                .Where(r => r.StationOrder >= FromOrder && r.StationOrder <= ToOrder)
                .ToList()
        };;
    }
    
    public async Task<List<OperationRouteDTO>> GetFromTo(int From , int To,uint? FullTimeLimit,List<string>? StartLimits)
        {
            List<OperationRouteDTO> Opers = new List<OperationRouteDTO>();
            
            var Routes = await _repository.GetRotesFromTo(From, To);
            
            foreach (var route in Routes)
            {
                route.Sations = route.Sations.OrderBy(s => s.StationOrder).ToList();
                RouteStation stationFrom = null;
                RouteStation stationTo = null;
                    
                foreach (var station in route.Sations)
                {
                    if (station.Station.Code == From) stationFrom = station;
                    if (station.Station.Code == To) stationTo = station;
                }
                if (stationFrom==null || stationTo == null || stationFrom.StationOrder == 0 || stationTo.StationOrder == 0 || stationFrom.StationOrder >= stationTo.StationOrder) continue;
                if (StartLimits != null && StartLimits.Count > 0)
                {
                    bool IsInLimit = false;
                    foreach (var limit in StartLimits)
                    {
                        switch(limit)
                        {
                            case "Night": 
                                if (stationFrom.DepartTime >= TimeOnly.Parse("00:00") &&
                                    stationFrom.DepartTime < TimeOnly.Parse("06:00")) IsInLimit = true; 
                                break;
                            case "Morning":
                                if (stationFrom.DepartTime >= TimeOnly.Parse("06:00") &&
                                    stationFrom.DepartTime < TimeOnly.Parse("12:00")) IsInLimit = true; 
                                break;
                            case "Day":
                                if (stationFrom.DepartTime >= TimeOnly.Parse("12:00") &&
                                    stationFrom.DepartTime < TimeOnly.Parse("18:00")) IsInLimit = true; 
                                break;
                            case "Evening":
                                if (stationFrom.DepartTime >= TimeOnly.Parse("18:00") &&
                                    stationFrom.DepartTime <= TimeOnly.Parse("23:59")) IsInLimit = true; 
                                break;
                        
                        }
                        if (IsInLimit) break;
                    }
                    if (!IsInLimit) continue;
                }
                OperationRouteDTO  oper = new OperationRouteDTO();
                
                oper.RoutesOrigin = new List<Route>() {route};

                oper.RoutesClientOrder = new List<Route>()
                {
                    GetClientRoutePart(stationFrom.StationOrder, stationTo.StationOrder, oper.RoutesOrigin.FirstOrDefault()),
                };
                oper.DaysInRun = GetDaysInRun(oper.RoutesClientOrder);
                oper.StartedAt = stationFrom.DepartTime;
                oper.EndedAt = stationTo.ArrivalTime;
                
                oper.FullDuration = (oper.EndedAt.ToTimeSpan()+TimeSpan.FromDays(oper.DaysInRun))-oper.StartedAt.ToTimeSpan();
                
                if(oper.FullDuration.TotalHours > FullTimeLimit && FullTimeLimit!=0) continue;
                
                oper.StationsTransfer = null;
                oper.RouteTag = null;
                
                Opers.Add(oper);
            }
            return Opers;
        }
}