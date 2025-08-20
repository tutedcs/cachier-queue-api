using CashierQueueAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

 var builder = WebApplication.CreateBuilder(args);
string cors = "ConfigurarCors";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar SignalR
builder.Services.AddSignalR();

// Configuración de CORS
builder.Services.AddCors(options => {
    options.AddPolicy(name: cors, builder =>
    {
        builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // Permitir cualquier origen
            .AllowCredentials(); // Necesario para WebSockets con credenciales
    });
});

var app = builder.Build();

// Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(cors);

app.MapControllers();

// Mapear el hub de SignalR
app.MapHub<GlobalHub>("/globalHub");

app.Run();
