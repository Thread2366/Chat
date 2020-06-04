function sendForm() {
    var form = document.getElementById('ajax-form');
    var formData = new FormData(form);
    var href = form.getAttribute('action');
    fetch(href, { // файл-обработчик 
        method: 'POST',
        credentials: "same-origin",
        body: formData
    })
        .then(response => console.log('Сообщение отправлено методом fetch'))
        .catch(error => console.error(error));
    document.getElementById("message-box").value = "";
}

var start = new Date();
start.setFullYear(start.getFullYear() - 1);
var finish;

function updateChat() {
    finish = new Date();
    fetch("/Home/GetMessage", { // файл-обработчик 
        method: 'POST',
        credentials: "same-origin",
        headers: {
            'Content-Type': 'application/json', // отправляемые данные 
        },
        body: JSON.stringify({
            start: start,
            finish: finish
        })
    })
        .then(response => {
            response.json().then(messages => {
                var chatDiv = document.getElementById("chat-div");
                for (var i = 0; i < messages.length; i++) {
                    var senderDiv = document.createElement("div");
                    senderDiv.classList.add("sender-div");
                    senderDiv.innerHTML = messages[i].Sender;
                    var messageDiv = document.createElement("div");
                    messageDiv.classList.add("message-div");
                    messageDiv.innerHTML = messages[i].Message;
                    var timeDiv = document.createElement("div");
                    timeDiv.classList.add("time-div");
                    timeDiv.innerHTML = messages[i].Timestamp;
                    chatDiv.appendChild(senderDiv);
                    chatDiv.appendChild(messageDiv);
                    chatDiv.appendChild(timeDiv);
                }
                chatDiv.scrollTop = chatDiv.scrollHeight;
            });
        })
        .catch(error => console.error(error));
    start = finish;
    setTimeout(updateChat, 1000);
}

setTimeout(updateChat, 1000);