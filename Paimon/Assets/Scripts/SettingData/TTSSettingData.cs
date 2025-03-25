using System;


public class TTSSettingData
{
    public string HostName { get; set; } = "127.0.0.1";

    public ushort HostPort { get; set; } = 8991;

    public Uri Url => new($"http://{HostName}:{HostPort}/v1/tts");

    public int ChunkLength { get; set; } = 80;
}

