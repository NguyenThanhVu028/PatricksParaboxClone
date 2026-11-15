using System;
using System.IO;
using UnityEngine;

public class SaveAndLoadUtils
{
    private const string DEBUG_PREFIX = "[SaveAndLoadUtils]";
    private static string _saveDirectory;
    private static string _saveFileName = "GameData";

    public static event Action OnSaveGameData;

    private static GameData _gameData = null;

    public static GameData GameData
    {
        get => _gameData;
    }

    public static void SetSaveDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogError($"{DEBUG_PREFIX} Trying to set an invalid save directory!");
            return;
        }
        _saveDirectory = path;
    }

    public static void SetData(GameData newGameData)
    {
        _gameData = newGameData;
    }

    public static void SaveData()
    {
        OnSaveGameData?.Invoke();

        if (_gameData == null)
        {
            Debug.LogError($"{DEBUG_PREFIX} Game data is null! Cannot Save!");
            return;
        }

        string gameDataFullPath = Path.Combine(Application.persistentDataPath, _saveDirectory, _saveFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(gameDataFullPath));
            string DataString = JsonUtility.ToJson(_gameData, true);

            using FileStream stream = new(gameDataFullPath, FileMode.Create);
            using StreamWriter writer = new(stream);
            writer.Write(DataString);
        }
        catch (Exception e)
        {
            Debug.LogError($"{DEBUG_PREFIX} Can't save game data! Error: " + e.Message);
        }
    }

    public static void LoadData()
    {
        string gameDataFullPath = Path.Combine(Application.persistentDataPath, _saveDirectory, _saveFileName);

        if (!File.Exists(gameDataFullPath))
        {
            _gameData = new();
            return;
        }
        else
        {
            // Load game data
            string gameDataString;

            using (FileStream stream = new(gameDataFullPath, FileMode.Open))
            {
                using StreamReader reader = new(stream);
                gameDataString = reader.ReadToEnd();
            }

            _gameData = JsonUtility.FromJson<GameData>(gameDataString);
            if (_gameData == null)
            {
                Debug.LogWarning($"{DEBUG_PREFIX} Failed to load game data!");
                _gameData = new();
            }
        }
    }
}
