using System.Text.Json.Serialization;

namespace Bank.Notification.Api.Domain.Entities.Notification
{
    public class NotificationEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; }
        [JsonPropertyName("notificationDate")]
        public DateTime NotificationDate { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("transactionState")]
        public bool TransactionState { get; set; }
        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }
    }
}
