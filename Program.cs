using PetAmigoChat.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenAI Assistant
builder.Services.AddSingleton<OpenAIAssistantService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔴 LINHA ESSENCIAL (ADICIONE)
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
