namespace EffectiveMobile.Database.Models;

public class Log
{
    public uint Id { get; set; }    
    public DateTime TimeStamp { get; set; }
    public string LevelAsText { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RenderedMessage { get; set; } = string.Empty;
    public Dictionary<string, object>? Properties { get; set; } 
}