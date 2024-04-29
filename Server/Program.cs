using Cord.Server;
using Cord.Server.Application;
using Cord.Server.Application.BufferHandlers;
using Cord.Server.Application.Equipment;
using Cord.Server.Application.Hub;
using Cord.Server.Application.Rooms.CommandValidators;
using Cord.Server.Application.Scrapers;
using Cord.Server.Application.Users;
using Cord.Server.Domain;
using Cord.Server.Domain.Equipment;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Leveling;
using Cord.Server.Domain.Messages;
using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;
using Cord.Server.Domain.Verification;
using Cord.Server.Repository;
using Cord.Server.Services;
using FluentValidation;
using MediatR;
using Newtonsoft.Json;
using Rikarin.Repository;
using Rikarin.Runner;
using Rikarin.Runner.Configurations;
using System.Runtime;

var builder = WebApplication.CreateBuilder(args);
GCSettings.LatencyMode = GCLatencyMode.LowLatency;

builder.AddConfig();
builder.AddLogger();
builder.InitKestrel();
builder.AddHealthChecks(true, true);
builder.AddControllers();

builder.Services.AddSignalR()
    .AddNewtonsoftJsonProtocol(
        options => {
            options.PayloadSerializerSettings.ContractResolver = new InterfaceContractResolver();
            options.PayloadSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.PayloadSerializerSettings.Converters.Add(new IdConverter());
        }
    );

builder.AddCorsPolicy();

builder.Services.AddStackExchangeRedisCache(
    options => {
        var config = builder.Configuration.GetSection(RedisOptions.Section).Get<RedisOptions>();
        options.Configuration = config.Hostname;
    }
);

builder.Services.AddGrpc();
builder.Services.AddOpenApiDocument();
builder.Services.AddAutoMapper(typeof(EquipmentProfile).Assembly);

builder.Services.AddTransient<Room>();
builder.Services.AddTransient<Playlist>();
builder.Services.AddTransient<User>();
builder.Services.AddTransient<Member>();
builder.Services.AddTransient<Message>();

builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<ResourceUploader>();
builder.Services.AddScoped<RoomProvider>();
builder.Services.AddScoped<PlaylistProvider>();
builder.Services.AddScoped<SongProvider>();
builder.Services.AddScoped<UserProvider>();
builder.Services.AddScoped<IUserProvider, UserProvider>();
builder.Services.AddScoped<MessageProvider>();
builder.Services.AddScoped<VerificationProvider>();
builder.Services.AddScoped<IGatewaySender, GatewaySender>();
builder.Services.AddScoped<SpotifyScraper>();

builder.Services.AddSingleton<IItemsManager, ItemsManager>();
builder.Services.AddSingleton<ISanitizer, Sanitizer>();
builder.Services.AddSingleton<ICaptcha, HCaptcha>();
builder.Services.AddSingleton<YoutubeScraper>();

builder.Services.Configure<CDNOptions>(builder.Configuration.GetSection(CDNOptions.Section));

// Mediator CQRS + Domain Events + Integration Events (TODO)
builder.Services.AddMediatR(typeof(MemberAddedHandler));
builder.AddMediatorFluentValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateRoomCommandValidator>();

builder.InitDatabase();
builder.InitEmailSender();

builder.AddAuth("/gateway");
builder.AddMetrics("cord");

var app = builder.Build();

app.UseMetricsAllMiddleware();
app.UseMetricsAllEndpoints();
app.MapHealthChecks("/healthz");

// Documentation
app.UseOpenApi();
app.UseSwaggerUi3();
app.UseReDoc();

app.UseRouting();
app.UseExceptionHandler("/error");
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CorsPolicy");
app.UseDomainEvents();

app.MapHub<GatewayHub>("/gateway");
app.MapControllers();
app.MapGrpcService<AuthServiceImpl>();

// var itemsManager = app.Services.GetRequiredService<IItemsManager>();
// await itemsManager.AddUserItem(ID.Parse("9340506911801344"), ItemsManager.Batman.Id);
// await itemsManager.AddUserItem(ID.Parse("9340506911801344"), OgStuff.OgDick.Id);
// return;

Scripts.AutoDJ(app.Services);
Scripts.DisconnectTimeout(app.Services);
Scripts.MetricsUpdate(app.Services);

// for (var i = 0; i <= 99; i++) {
//     Log.Information("Level {Level} Old: {Old} New: {New}", i, ExpCalculator.MaxExpForLevel(i), ExpCalculator.ExperienceForLevel(i));
// }


app.Run();
