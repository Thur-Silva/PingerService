namespace PingerService.Models
{
    public class PingTarget
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int IntervalMinutes { get; set; } = 10;
    }
}