using CloneUZ.DAL.Interfaces;
using CloneUZ.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.DAL.Repositories;

public class RouteRepository:IRouteRepository
{
    readonly AppDBContext _context;
    public RouteRepository(AppDBContext context)
    {
        _context = context;
    }
    
    public async Task<List<Route>> GetRotesFromTo(int From , int To)
    {
        return await _context.Routes.Include(r => r.Sations)          
            .ThenInclude(rs => rs.Station).Where(r =>
                r.Sations.Any(rs => rs.Station.Code == From) &&
                r.Sations.Any(rs => rs.Station.Code == To))
            .ToListAsync();
    }
    public async Task<List<Route>> GetRoutes(int city)
    {
        return await _context.Routes.Include(r => r.Sations)
            .ThenInclude(rs => rs.Station).Where(r =>
                r.Sations.Any(rs => rs.Station.Code == city)).ToListAsync();
    }
}