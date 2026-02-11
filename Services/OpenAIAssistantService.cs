namespace PetAmigoChat.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIAssistantService
{
    private readonly HttpClient _http;
    private readonly string _assistantId;

    public OpenAIAssistantService(IConfiguration config)
    {
        _assistantId = config["OpenAI:AssistantId"]
            ?? throw new Exception("❌ OpenAI:AssistantId não configurado");

        var apiKey = config["OpenAI:ApiKey"]
            ?? throw new Exception("❌ OpenAI:ApiKey não configurada");

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        _http.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }

    public async Task<string> AskAsync(string message)
    {
        // 1️⃣ Criar thread
        var thread = await Post("https://api.openai.com/v1/threads", new { });

        if (!thread.TryGetProperty("id", out var threadIdEl))
            return "❌ Erro ao criar conversa com o assistente.";

        var threadId = threadIdEl.GetString();

        // 2️⃣ Enviar mensagem do usuário
        await Post(
            $"https://api.openai.com/v1/threads/{threadId}/messages",
            new
            {
                role = "user",
                content = message
            }
        );

        // 3️⃣ Criar run
        var run = await Post(
            $"https://api.openai.com/v1/threads/{threadId}/runs",
            new
            {
                assistant_id = _assistantId
            }
        );

        if (!run.TryGetProperty("id", out var runIdEl))
            return "❌ Erro ao iniciar o assistente.";

        var runId = runIdEl.GetString();

        // 4️⃣ Aguardar processamento
        while (true)
        {
            await Task.Delay(1000);

            var status = await Get(
                $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}"
            );

            if (status.TryGetProperty("status", out var statusEl) &&
                statusEl.GetString() == "completed")
            {
                break;
            }
        }

        // 5️⃣ Buscar mensagens
        var messages = await Get(
            $"https://api.openai.com/v1/threads/{threadId}/messages"
        );

        // 6️⃣ Extrair resposta com segurança
        if (!messages.TryGetProperty("data", out var dataArray) ||
            dataArray.ValueKind != JsonValueKind.Array ||
            dataArray.GetArrayLength() == 0)
        {
            return "😿 O assistente não retornou nenhuma resposta.";
        }

        var lastMessage = dataArray[0];

        if (!lastMessage.TryGetProperty("content", out var contentArray) ||
            contentArray.ValueKind != JsonValueKind.Array ||
            contentArray.GetArrayLength() == 0)
        {
            return "😿 Resposta vazia do assistente.";
        }

        var content = contentArray[0];

        if (content.TryGetProperty("text", out var textObj) &&
            textObj.TryGetProperty("value", out var value))
        {
            return value.GetString() ?? "😿 Resposta vazia.";
        }

        return "😿 Não foi possível interpretar a resposta do assistente.";
    }

    // 🔹 POST genérico
    private async Task<JsonElement> Post(string url, object body)
    {
        var json = JsonSerializer.Serialize(body);

        var res = await _http.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("POST RESPONSE:");
        Console.WriteLine(content);

        return JsonDocument.Parse(content).RootElement;
    }

    // 🔹 GET genérico
    private async Task<JsonElement> Get(string url)
    {
        var res = await _http.GetAsync(url);
        var content = await res.Content.ReadAsStringAsync();

        Console.WriteLine("GET RESPONSE:");
        Console.WriteLine(content);

        return JsonDocument.Parse(content).RootElement;
    }
}
