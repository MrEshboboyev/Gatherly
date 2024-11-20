using Gatherly.Persistence;
using Microsoft.EntityFrameworkCore;
using Gatherly.Persistence.Interceptors;
using MediatR;
using Quartz;
using Gatherly.Infrastructure.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
       .Scan(selector => selector
       .FromAssemblies(
           Gatherly.Infrastructure.AssemblyReference.Assembly,
           Gatherly.Persistence.AssemblyReference.Assembly)
           .AddClasses(false)
           .AsImplementedInterfaces()
           .WithScopedLifetime());

builder.Services.AddMediatR(Gatherly.Application.AssemblyReference.Assembly);

string connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
builder.Services.AddDbContext<ApplicationDbContext>(
    (sp, optionsBuilder) =>
    {
        var inteceptor = sp.GetService<ConvertDomainEventsToOutboxMessagesInterceptor>();
        optionsBuilder.UseNpgsql(connectionString)
            .AddInterceptors(inteceptor);
    });

builder.Services.AddQuartz(configure =>
{
    var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));
    configure
        .AddJob<ProcessOutboxMessagesJob>(jobKey)
        .AddTrigger(
            trigger =>
                trigger.ForJob(jobKey)
                    .WithSimpleSchedule(
                        schedule =>
                            schedule.WithIntervalInSeconds(10)
                                .RepeatForever()));
    configure.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService();

builder.Services.AddControllers()
    .AddApplicationPart(Gatherly.Presentation.AssemblyReference.Assembly);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
