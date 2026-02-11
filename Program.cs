using PetAmigoChat.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Carrega .env
Env.Load();

// Variáveis do OpenAI
builder.Configuration["OpenAI:ApiKey"] =
    Environment.GetEnvironmentVariable("OPENAI_API_KEY");

builder.Configuration["OpenAI:AssistantId"] =
    Environment.GetEnvironmentVariable("OPENAI_ASSISTANT_ID");

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<OpenAIAssistantService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// LINHA ESSENCIAL (SEM ISSO HTML NÃO FUNCIONA)
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
