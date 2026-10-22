namespace Contracts.Infrastructure.Persistence;

public sealed class ContractsDbContextFactory : IDesignTimeDbContextFactory<ContractsDbContext>
{
    public ContractsDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ContractsDbContext> options = new();
        options.UseSqlite("Data Source=:memory:;Foreign Keys=True");
        options.AddInterceptors(new SqliteForeignKeysInterceptor());
        return new ContractsDbContext(options.Options);
    }
}
