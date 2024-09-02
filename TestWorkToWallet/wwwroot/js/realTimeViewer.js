document.addEventListener("DOMContentLoaded", function () {
    const messagesDiv = document.getElementById("messages");

    const socket = new WebSocket('wss://localhost:44332/ws/messages');

    socket.onmessage = function (event) {
        const message = JSON.parse(event.data);

        messagesDiv.innerHTML = `
            <div>
                <strong>Message:</strong> ${message.Content}
                <strong>Timestamp:</strong> ${new Date(message.Timestamp).toLocaleString()}
                <strong>Order:</strong> ${message.OrderNumber}
            </div>
        `;

    };

    socket.onerror = function (error) {
        console.error('WebSocket Error: ', error);
    };

    socket.onclose = function (event) {
        if (event.wasClean) {
            console.log('WebSocket closed cleanly');
        } else {
            console.error('WebSocket connection error');
        }
    };

    socket.onopen = function () {
        console.log('WebSocket connection established');
    };
});
