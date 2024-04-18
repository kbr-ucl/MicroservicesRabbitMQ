using ProductUser.Microservice.BackgroundServices;
using ProductUser.Microservice.Data;
using ProductUser.Microservice.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<DbContextClass>();
builder.Services.AddHostedService<RabbitMQBackgroundConsumerService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
