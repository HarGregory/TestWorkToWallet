document.addEventListener("DOMContentLoaded", function () {
    const sendMessageButton = document.getElementById("sendMessageButton");
    const messageInput = document.getElementById("messageInput");
    const numberInput = document.getElementById("numberInput");
    const socket = new WebSocket('wss://localhost:44332/ws/messages');

    sendMessageButton.addEventListener("click", async function () {
        const message = messageInput.value.trim();
        const number = numberInput.value;

        if (message === "" || message.length > 128) {
            alert("Message cannot be empty and must be less than 128 characters!");
            return;
        }

        if (!/^\d+$/.test(number)) {
            alert("Number must be a valid integer!");
            return;
        }

        const messageData = {
            Content: message,
            OrderNumber: parseInt(number),
            Timestamp: new Date().toISOString()
        };

        const response = await fetch('/api/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(messageData)
        });

        if (!response.ok) {
            showNotification('Message failed sent to the server. Please check your connection and ensure the message does not contain SQL injection attempts.', 'error');
            throw new Error('Message failed sent to the server');
        }

        showNotification('Message successfully sent to the server', 'success');

        if (socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify(messageData));
            messageInput.value = "";
            numberInput.value = "";
        } else {
            showNotification('WebSocket is not open', 'warning');
            console.error('WebSocket is not open. Current state:', socket.readyState);
        }
    });

    socket.onopen = function () {
        console.log('WebSocket connection established');
    };

    socket.onclose = function (event) {
        console.log(event.wasClean ? 'WebSocket closed cleanly' : 'WebSocket connection error');
    };

    socket.onerror = function (error) {
        console.error('WebSocket Error: ', error);
    };

    socket.onmessage = function (event) {
        console.log("Message received:", event.data);
    };

    function showNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.textContent = message;
        document.getElementById('notifications').appendChild(notification);

        setTimeout(() => {
            notification.remove();
        }, 5000);
    }
});
