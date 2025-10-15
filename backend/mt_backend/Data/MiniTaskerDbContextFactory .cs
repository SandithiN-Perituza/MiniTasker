using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using mt_backend.Data;
using System.IO;

public class MiniTaskerDbContextFactory : IDesignTimeDbContextFactory<MiniTaskerDbContext>
{
    public MiniTaskerDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<MiniTaskerDbContext>();
        optionsBuilder.UseMySQL(connectionString);

        return new MiniTaskerDbContext(optionsBuilder.Options);
    }
}