using System.Data.Common;
using System.Reflection;
using DotNet.Testcontainers.Builders;
using EffectiveMobile;
using EffectiveMobile.BackgroundJobs;
using EffectiveMobile.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using Testcontainers.Elasticsearch;
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

    private readonly ElasticsearchContainer _elasticsearchContainer = new ElasticsearchBuilder()
        .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.7.1")
        .WithEnvironment("discovery.type", "single-node")
        .WithEnvironment("xpack.security.enabled", "false")
        // .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("curl -fsSL http://localhost:9200/_cluster/health?wait_for_status=yellow"))
        .WithCleanUp(true)
        .Build();
    
    public AppDbContext Db = null!;
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_connection);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(c =>
        {
            c.AddInMemoryCollection([
                new KeyValuePair<string,string?>("ElasticsearchUrl", _elasticsearchContainer.GetConnectionString()),
                new KeyValuePair<string,string?>("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString())
            ]);
        });
        
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptorType1 =
                typeof(DbContextOptions<AppDbContext>);
            var descriptor1 = services
                .SingleOrDefault(s => s.ServiceType == descriptorType1);
            if (descriptor1 is not null)
            {
                services.Remove(descriptor1);
            }

            var hostedServiceDescriptors = services
                .Where(s => s.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var descriptor in hostedServiceDescriptors)
            {
                if (descriptor.ImplementationType != typeof(MigrateAndSeedDb)) continue;
                services.Remove(descriptor);
                break;
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgresContainer.GetConnectionString());
                dataSourceBuilder.EnableDynamicJson();
                options.UseNpgsql(dataSourceBuilder.Build(),
                        x =>
                        {
                            x.MigrationsAssembly(Assembly.GetAssembly(typeof(Program))!.FullName);
                        })
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            });
        });
    }

    public async Task InitializeAsync()
    {
        // starting integration containers
        var t1 = _postgresContainer.StartAsync();
        var t2 = _elasticsearchContainer.StartAsync();
        await Task.WhenAll(t1, t2);

        // applying migrations
        Db = Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        await Db.Database.MigrateAsync();
        
        // initializing respawner
        _connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
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
        await _connection.DisposeAsync();
        var pgDispose = _postgresContainer.StopAsync();
        var elDispose =  _elasticsearchContainer.StopAsync();
        await Task.WhenAll(pgDispose, elDispose);
        await Db.DisposeAsync();
        await base.DisposeAsync();
    }
}