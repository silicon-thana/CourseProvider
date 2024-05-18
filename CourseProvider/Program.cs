using CourseProvider.Infrastructure.Data.Contexts;
using CourseProvider.Infrastructure.GraphQL.Mutations;
using CourseProvider.Infrastructure.GraphQL.ObjectType;
using CourseProvider.Infrastructure.GraphQL.Queries;
using CourseProvider.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();


        services.AddPooledDbContextFactory<DataContext>(x => x.UseCosmos(Environment.GetEnvironmentVariable("COSMOS_URI")!, Environment.GetEnvironmentVariable("COSMOS_DB")!).UseLazyLoadingProxies());
        var sb = services.BuildServiceProvider();
        using var scope = sb.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
        using var context = dbContextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        services.AddScoped<ICourseService, CourseService>();

        services.AddGraphQLFunction()
        .AddQueryType<CourseQuery>()
        .AddType<CourseType>()
        .AddMutationType<CourseMutation>();


            
    })
    .Build();

host.Run();
