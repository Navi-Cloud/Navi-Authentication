using NaviAuth.Configuration;
using NaviAuth.Repository;
using NaviAuth.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Configuration
var mongoConfiguration = builder.Configuration.GetSection("MongoSection").Get<MongoConfiguration>();
builder.Services.AddSingleton(mongoConfiguration);

// Add Data Logic(Singleton)
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();

// Add Service Logic(Scoped)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/custom", "Navi Auth Service"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();