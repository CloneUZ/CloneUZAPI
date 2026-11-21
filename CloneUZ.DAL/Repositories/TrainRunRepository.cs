using CloneUZ.DAL.Interfaces;
using CloneUZ.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CloneUZ.DAL.Repositories;

public class TrainRunRepository:ITrainRunRepository
{
    private readonly AppDBContext _context;
    public TrainRunRepository(AppDBContext context)
    {
        _context = context;
    }
    public async Task<TrainRun?> GetTrainRunsBy(Guid routeId)
    {
        return await _context.TrainRuns.Include(tr=>tr.Train).Where(tr=>tr.RouteId == routeId).FirstOrDefaultAsync();
    }
}