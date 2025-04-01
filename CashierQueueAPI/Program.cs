var builder = WebApplication.CreateBuilder(args);
string cors = "ConfigurarCors";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//para error de cords
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: cors, builder =>
    {
        builder.WithMethods("*");
        builder.WithHeaders("*");
        builder.WithOrigins("*");
    });
});
builder.Services.AddHttpClient(); // Add HttpClient service

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(cors);

app.MapControllers();

app.Run();