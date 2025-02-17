using Newtonsoft.Json;
using System.IO;
using Unity.Logging;
using UnityEngine;


public class UserSettingData
{
    public ASRSettingData ASRSettingData { get; set; } = new();
    public LLMSettingData LLMSettingData { get; set; } = new();
    public TTSSettingData TTSSettingData { get; set;} = new();

    /// <summary>
    /// 保存
    /// </summary>
    public void Save()
    {
        var userSettingDataPath = Path.Join(Application.persistentDataPath, "UserSettingData.json");

        // 序列化
        var json = JsonConvert.SerializeObject(this);
        Log.Info($"保存配置{userSettingDataPath}:{json}");
        File.WriteAllText(userSettingDataPath, json);
    }

    private static UserSettingData instance;
    public static UserSettingData Instance => instance ??= Load();

    private static UserSettingData Load()
    {
        var userSettingDataPath = Path.Join(Application.persistentDataPath, "UserSettingData.json");
        UserSettingData userSettingData;
        if (File.Exists(userSettingDataPath))
        {
            string json = File.ReadAllText(userSettingDataPath);
            Log.Info($"加载配置{userSettingDataPath}:{json}");
            userSettingData = JsonConvert.DeserializeObject<UserSettingData>(json);
        }
        else
        {
            userSettingData = new UserSettingData();
            userSettingData.Save();
        }
        return userSettingData;
    }
}

