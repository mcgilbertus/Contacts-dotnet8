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
    opt => opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
    db.Database.Migrate();
}

app.Run();

return;

public partial class Program
{
}
