namespace Frends.Radon
{
    public class RadonResult
    {
        public string UserResultMessage { get; set; }
        public bool Success { get; set; }
        public bool ActionSkipped { get; set; }
        public int UnreportedEventsCount { get; set; }
    }
}
