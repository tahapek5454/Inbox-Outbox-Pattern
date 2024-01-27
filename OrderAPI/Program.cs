using Microsoft.EntityFrameworkCore;
using OrderAPI.Data.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();




app.Run();
