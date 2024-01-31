using Contacts.api.Models;
using Contacts.data;
using Contacts.data.Repositories;
using Contacts.domain;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Using controllers (not minimal api)
builder.Services.AddControllers();

// Swagger/OpenAPI https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContactsDbContext>(
    opt => opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.CommandTimeout(180).ExecutionStrategy(c => new SqlServerRetryingExecutionStrategy(c))
    )
);

builder.Services.AddScoped<IRepository<Contact>, ContactsRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<ContactUpdateModelValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// map routes to controllers
app.MapControllers();

app.Run();