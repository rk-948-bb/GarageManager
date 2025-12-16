using GarageApi.Models;
using GarageApi.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register CORS to allow requests from Angular dev server (adjust origin as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind Mongo settings
var mongoSection = builder.Configuration.GetSection("MongoSettings");
var mongoSettings = mongoSection.Get<MongoSettings>();

// Create Mongo client and register
var client = new MongoClient(mongoSettings.ConnectionString);
var db = client.GetDatabase(mongoSettings.Database);

builder.Services.AddSingleton(client);
builder.Services.AddSingleton(db);
builder.Services.AddSingleton(sp => db.GetCollection<Garage>(mongoSettings.GaragesCollection));

builder.Services.AddHttpClient();
builder.Services.AddScoped<GarageRepository>();

var app = builder.Build();

// Ensure index on ExternalId
using (var scope = app.Services.CreateScope())
{
    var collection = scope.ServiceProvider.GetRequiredService<IMongoCollection<Garage>>();
    var indexKeys = Builders<Garage>.IndexKeys.Ascending(g => g.ExternalId);
    var indexOptions = new CreateIndexOptions { Unique = true, Sparse = true };
    collection.Indexes.CreateOne(new CreateIndexModel<Garage>(indexKeys, indexOptions));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS using the policy
app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
