using Elsa.Persistence.EntityFramework.Core;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.PostgreSql;
using Rebus.Config;
using Rebus.PostgreSql;

var builder = WebApplication.CreateBuilder(args);
AddElsa(builder.Services);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

static IServiceCollection AddElsa(IServiceCollection services)
{
    /*
        ALTER SYSTEM SET shared_preload_libraries='pg_stat_statements';

        CREATE EXTENSION pg_stat_statements;

        --$systemctl restart postgresql
        
        select query, calls, (total_exec_time/calls)::integer as avg_time_ms
        from pg_stat_statements
        where query ilike 'DELETE from %recipient%'
        order by calls desc
        limit 10;
     */
    const string connectionString = "Host=localhost;Database=postgres;Username=postgres;Password=postgres;Persist Security Info=True;Include Error Detail=True";

    return services
        .AddElsa(options =>
        {
            var provider = new PostgresConnectionHelper(connectionString);

            options
                .UseEntityFrameworkPersistence<ElsaContext>(ef => ef.UsePostgreSql(connectionString), true)
                .UseServiceBus(sb => sb.Configurer
                    .Transport(t =>
                        t.UsePostgreSql(provider, "sb", sb.QueueName,
                            schemaName: "wf"))
                    .Subscriptions(s =>
                        s.StoreInPostgres(provider, "sub", true,
                            schemaName: "wf")));
        });
}