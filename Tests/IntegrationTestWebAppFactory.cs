using System.Data.Common;
using System.Reflection;
using DotNet.Testcontainers.Builders;
using EffectiveMobile;
using EffectiveMobile.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;

namespace Tests;

public class IntegrationTestWebAppFactory:
    WebApplicationFactory<Program>,
    IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    public AppDbContext Db = null!;
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;
    
    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_connection);
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptorType =
                typeof(DbContextOptions<AppDbContext>);

            var descriptor = services
                .SingleOrDefault(s => s.ServiceType == descriptorType);

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString(), x =>
                {
                    x.MigrationsAssembly(Assembly.GetAssembly(typeof(Program))!.FullName);
                }));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        Db = Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        
        await Db.Database.EnsureDeletedAsync();
        // applying migrations
        await Db.Database.MigrateAsync();
        
        // initializing respawner
        _connection = Db.Database.GetDbConnection();
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            WithReseed = true
        });
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _postgresContainer.StopAsync();
        await Db.DisposeAsync();
        await base.DisposeAsync();
    }
}