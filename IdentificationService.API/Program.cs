using IdentificationService.API;
using IdentificationService.Application;
using IdentificationService.Application.Constants;
using IdentificationService.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureApi();
builder.Services.ConfigureApplication();
builder.Services.ConfigureIdentitication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("allConnections", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("allConnections");

    _ = builder.Services.ConfigureDatabaseForDevelopmentAsync(app);
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    var a = context.User.IsInRole(RoleTypes.Admin);
    await next(context);
});

app.UseAuthorization();

app.MapControllers();

app.Run();
