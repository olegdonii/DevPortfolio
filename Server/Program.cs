using Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
        builder =>
        {
            builder.WithOrigins("http://example.com/",
                                "http://www.contoso.com/%22")
                    .WithMethods("PUT", "DELETE", "GET");
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseSqlite("Data Source=./Data/AppDB.db"));
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI(swaggerUIOptions =>
{
    swaggerUIOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API");
    swaggerUIOptions.RoutePrefix = string.Empty;
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();