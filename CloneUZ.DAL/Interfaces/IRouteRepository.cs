using CloneUZ.Domain.Models;

namespace CloneUZ.DAL.Interfaces;

public interface IRouteRepository
{
    public Task<List<Route>> GetRotesFromTo(int From, int To);

    public Task<List<Route>> GetRoutes(int city);
}