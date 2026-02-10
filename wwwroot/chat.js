function toggleChat() {
    document.getElementById("chat-box").classList.toggle("open");
}

async function sendMessage() {
    const input = document.getElementById("input");
    const messages = document.getElementById("messages");
    const text = input.value.trim();
    if (!text) return;

    messages.innerHTML += `<div class="message user">${text}</div>`;
    input.value = "";

    const typing = document.createElement("div");
    typing.className = "message bot typing";
    typing.innerText = "Digitando...";
    messages.appendChild(typing);

    try {
        const res = await fetch("/api/chat", {
            method: "POST",
            headers: {"Content-Type":"application/json"},
            body: JSON.stringify({ message: text })
        });

        const data = await res.json();
        typing.remove();
        messages.innerHTML += `<div class="message bot">${data.reply}</div>`;
    } catch {
        typing.remove();
        messages.innerHTML += `<div class="message bot">Erro ❌</div>`;
    }

    messages.scrollTop = messages.scrollHeight;
}

function handleEnter(e) {
    if (e.key === "Enter") sendMessage();
}
