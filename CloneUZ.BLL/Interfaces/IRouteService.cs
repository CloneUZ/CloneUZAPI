using CloneUZ.Domain.DTOs;
using CloneUZ.Domain.Models;

namespace CloneUZ.BLL.Interfaces;

public interface IRouteService
{
    public Task<List<OperationRouteDTO>> GetFromTo(int From, int To, uint? FullTimeLimit, List<string>? StartLimits);
}