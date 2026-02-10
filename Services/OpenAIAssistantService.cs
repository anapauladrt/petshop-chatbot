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
        _assistantId = config["OpenAI:AssistantId"];

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config["OpenAI:ApiKey"]);

        _http.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }

    public async Task<string> AskAsync(string message)
    {
        var thread = await Post("https://api.openai.com/v1/threads", new { });
        var threadId = thread.GetProperty("id").GetString();

        await Post(
            $"https://api.openai.com/v1/threads/{threadId}/messages",
            new { role = "user", content = message }
        );

        var run = await Post(
            $"https://api.openai.com/v1/threads/{threadId}/runs",
            new { assistant_id = _assistantId }
        );

        var runId = run.GetProperty("id").GetString();

        while (true)
        {
            await Task.Delay(1000);
            var status = await Get(
                $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}"
            );

            if (status.GetProperty("status").GetString() == "completed")
                break;
        }

        var messages = await Get(
            $"https://api.openai.com/v1/threads/{threadId}/messages"
        );

        return messages
            .GetProperty("data")[0]
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetProperty("value")
            .GetString();
    }

    private async Task<JsonElement> Post(string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var res = await _http.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var content = await res.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content).RootElement;
    }

    private async Task<JsonElement> Get(string url)
    {
        var res = await _http.GetAsync(url);
        var content = await res.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content).RootElement;
    }
}
