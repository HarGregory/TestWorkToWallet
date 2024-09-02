using Npgsql;
using System;
using System.Collections.Generic;
using TestWorkToWallet.Models;
using TestWorkToWallet.Helpers;
using TestWorkToWallet.WebSockets;



namespace TestWorkToWallet.DAL
{
    public class MessageRepository
    {
        private readonly WebSocketHandlerImplementation _webSocketHandler;
        private readonly string _connectionString;
        private readonly Helper _helper;
        private readonly ILogger<MessageRepository> _logger;
        public MessageRepository(string connectionString, Helper helper, ILogger<MessageRepository> logger, WebSocketHandlerImplementation webSocketHandler)
        {
            _connectionString = connectionString;
            _helper = helper;
            _logger = logger;
            _webSocketHandler = webSocketHandler;
        }

        public bool AddMessage(Message message)
        {
            try
            {
                if (!_helper.ContainsSqlInjection(message.Content))
                {
                    using var connection = new NpgsqlConnection(_connectionString);
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO messages (content, timestamp, order_number) VALUES (@content, @timestamp, @order_number)", connection);
                    command.Parameters.AddWithValue("content", message.Content);
                    command.Parameters.AddWithValue("timestamp", message.Timestamp);
                    command.Parameters.AddWithValue("order_number", message.OrderNumber);
                    command.ExecuteNonQuery();

                    _logger.LogInformation("AddMessage Success");

                    _webSocketHandler.BroadcastMessageAsync(message.Content, CancellationToken.None).GetAwaiter().GetResult();

                    return true;
                }
                else
                {
                    _logger.LogError("ERROR , did not pass the test to  SqlInjection");
                    return false;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR:  {0}", ex.Message);
                return false;
                throw;
            }

        }

        public async Task<List<Message>> GetMessagesAsync(DateTime from, DateTime to)
        {
            var messages = new List<Message>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var command = new NpgsqlCommand("SELECT * FROM messages WHERE timestamp BETWEEN @from AND @to ORDER BY timestamp", connection);
                command.Parameters.AddWithValue("from", from);
                command.Parameters.AddWithValue("to", to);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    messages.Add(new Message
                    {
                        Content = reader["content"].ToString(),
                        Timestamp = (DateTime)reader["timestamp"],
                        OrderNumber = (int)reader["order_number"]
                    });
                }
                _logger.LogInformation("GetMessagesAsync Success");
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR :  {0}", ex.Message);
            }
           

            return messages;
        }

    }
}
