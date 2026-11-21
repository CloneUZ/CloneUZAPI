using CloneUZ.Domain.Models;

namespace CloneUZ.DAL.Interfaces;

public interface ITrainRunRepository
{
    public Task<TrainRun?> GetTrainRunsBy(Guid routeId);
}