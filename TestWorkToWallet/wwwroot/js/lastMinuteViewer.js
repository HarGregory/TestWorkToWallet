document.addEventListener("DOMContentLoaded", function () {
    const loadMessagesButton = document.getElementById("loadMessagesButton");
    const messagesDiv = document.getElementById("messages");
    const interval = document.querySelector('.lastMinuteViewer').getAttribute('data-interval');

    loadMessagesButton.addEventListener("click", function () {
        fetch('/api/lastminute')
            .then(response => response.json())
            .then(data => {
                if (data.length === 0) {
                    messagesDiv.innerHTML = '<div>No messages found in the last ' + interval +' minutes.</div>';
                } else {
                    messagesDiv.innerHTML = data.map(message => `
                        <div>
                            <strong>Message:</strong> ${message.content}
                            <strong>Timestamp:</strong> ${new Date(message.Timestamp).toLocaleString()}
                            <strong>Order:</strong> ${message.orderNumber}
                        </div>
                    `).join('');
                }
            })
            .catch(error => {
                console.error('Error fetching messages:', error);
                messagesDiv.innerHTML = '<div>Error loading messages.</div>';
            });
    });
});
