using CloneUZ.BLL.Interfaces;
using CloneUZ.BLL.Mappers;
using CloneUZ.DAL.Interfaces;
using DTOs.Requests;
using DTOs.Responses;

namespace CloneUZ.BLL.Services;

public class TrainRunService:ITrainRunService
{
    private readonly IRouteService _routeService;
    private readonly ITrainRunRepository _repository;

    public TrainRunService(IRouteService routeService, ITrainRunRepository repository)
    {
        _routeService = routeService;
        _repository = repository;
    }
    public async Task<List<TicketsAnswer>> FindTickets(FindTicket request)
    {
        var routes = await _routeService.GetFromTo(request.From, request.To,request.FullRouteTimeLimit,request.TimeStartLimits);
        List<TicketsAnswer> result = new List<TicketsAnswer>();
        foreach (var route in routes)
        {
            var train = await _repository.GetTrainRunsBy(route.RoutesOrigin[0].Id);
            if (train == null) continue;
            if( train.Date != request.Date ) continue;
            if (request.TrainsType != null && request.TrainsType.Count > 0 && request.TrainsType.Contains(train.Train.Type) != true)continue;
            double killometrs = train.Train.AvgSpeed * route.FullDuration.TotalHours;
            double price = killometrs * train.Train.MoneyPerMeter;
            if(request.PriceTo > 0 && price>request.PriceTo || price<request.PriceFrom) continue;
            
            result.Add(new TicketsAnswer()
            {
                TrainId = train.TrainId,
                RoutesIds = new List<Guid>(){route.RoutesOrigin[0].Id},
                
                StartedTime = route.StartedAt,
                EndedTime = route.EndedAt,
                
                FullRouteTime = route.FullDuration,
                
                StartedDate = train.Date,
                EndedDate =  train.Date.AddDays((int)route.DaysInRun),
                
                RoutesOrigin = route.RoutesOrigin.ListToDTO(),
                StationsTransfer = null,
                RoutesClientOrder = route.RoutesClientOrder.ListToDTO(),
                
                StationFromName = route.RoutesClientOrder[0].Sations.FirstOrDefault().Station.Name,
                StationToName = route.RoutesClientOrder[0].Sations.LastOrDefault().Station.Name,
                
                Number = train.Train.Number,
                Type = train.Train.Type,
                Price = price
            });
        }
        return result;
    }
}