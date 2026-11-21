using DTOs.Requests;
using DTOs.Responses;

namespace CloneUZ.BLL.Interfaces;

public interface ITrainRunService
{
    public Task<List<TicketsAnswer>> FindTickets(FindTicket request);
}