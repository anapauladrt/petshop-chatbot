using Microsoft.AspNetCore.Mvc;
using PetAmigoChat.Services;

namespace PetAmigoChat.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly OpenAIAssistantService _assistant;

    public ChatController(OpenAIAssistantService assistant)
    {
        _assistant = assistant;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ChatRequest req)
    {
        var reply = await _assistant.AskAsync(req.Message);
        return Ok(new { reply });
    }
}

public class ChatRequest
{
    public string Message { get; set; }
}
