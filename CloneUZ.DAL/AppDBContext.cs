using Microsoft.EntityFrameworkCore;

namespace CloneUZ.DAL;

public class AppDBContext:DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
}
