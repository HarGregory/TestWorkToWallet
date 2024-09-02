namespace TestWorkToWallet.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string Status { get; set; } // Status could be "Success", "Error", "Critical"
        public DateTime LogDate { get; set; }
    }
}
