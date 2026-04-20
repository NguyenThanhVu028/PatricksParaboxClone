using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Events;

public class SaveAndLoadManager : MonoBehaviour
{
    private static SaveAndLoadManager instance;
    public static SaveAndLoadManager Instance { get => instance; }

    [SerializeField] string gameDataFileName = "GameData";

    private static GameData gameData;
    public static GameData GeneralGameData { get => gameData; }

    private UnityEvent onLoadDataEvent = new();
    private UnityEvent onSaveDataEvent = new();

    public UnityEvent OnLoadDataEvent { get => onLoadDataEvent; }
    public UnityEvent OnSaveDataEvent { get => onSaveDataEvent; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void OnEnable()
    {
        if (gameData == null) LoadData();
    }

    private void OnDisable()
    {
        SaveData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void LoadData()
    {
        string gameDataFullPath = Path.Combine(Application.persistentDataPath, gameDataFileName);

        if (!File.Exists(gameDataFullPath))
        {
            gameData = new();
            return;
        }
        else
        {
            // Load game data
            string gameDataString;

            using (FileStream stream = new FileStream(gameDataFullPath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    gameDataString = reader.ReadToEnd();
                }
            }

            gameData = JsonUtility.FromJson<GameData>(gameDataString);
            if (gameData == null)
            {
                Debug.LogWarning("Load game data failed!");
                gameData = new();
            }
        }

        onLoadDataEvent.Invoke();
    }

    private void SaveData()
    {
        onSaveDataEvent.Invoke();

        string gameDataFullPath = Path.Combine(Application.persistentDataPath, gameDataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(gameDataFullPath));
            string DataString = JsonUtility.ToJson(gameData, true);

            using (FileStream stream = new FileStream(gameDataFullPath, FileMode.Create))
            {
                using (StreamWriter Writer = new StreamWriter(stream))
                {
                    Writer.Write(DataString);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Can't save game data! " + e.Message);
        }
    }

    [Serializable]
    public class GameData
    {
        //[SerializeField] List<string> unlockedWorldIDs = new();
        [SerializeField] List<string> finishedMapIDs = new();
        [SerializeField] string lastOpenedMapName = "";
        [SerializeField] string lastOpenedWorldName = "";
        [SerializeField] string lastUsedFaceAniID = "DefaultFaceIdle";

        // Movement Settings values
        [SerializeField] float moveTime = 100f;
        [SerializeField] float enterTime = 500f;

        // Sound Settings values
        [SerializeField] float musicVolume = 50f;
        [SerializeField] float sfxVolume = 50f;

        private UnityEvent<float> onMusicVolumeChanged = new();
        private UnityEvent<float> onSFXVolumeChanged = new();

        //public List<string> UnLockedWorldIDs { get => unlockedWorldIDs; }
        public List<string> FinishedMapIDs { get => finishedMapIDs; }
        public string LastOpenedMapName { get => lastOpenedMapName; set => lastOpenedMapName = value; }
        public string LastOpenedWorldName { get => lastOpenedWorldName; set => lastOpenedWorldName = value; }
        public string LastUsedFaceAniID { get => lastUsedFaceAniID; set => lastUsedFaceAniID = value; }

        public float MoveTime { get => moveTime; set => moveTime = value; }
        public float EnterTime { get => enterTime; set => enterTime = value; }

        public float MusicVolume 
        { 
            get => musicVolume;
            set
            {
                musicVolume = value;
                onMusicVolumeChanged.Invoke(musicVolume);
            } 
        }
        public float SFXVolume 
        { 
            get => sfxVolume; 
            set
            {
                sfxVolume = value;
                onSFXVolumeChanged.Invoke(sfxVolume);
            } 
        }

        public UnityEvent<float> OnMusicVolumeChanged { get => onMusicVolumeChanged; }
        public UnityEvent<float> OnSFXVolumeChanged { get => onSFXVolumeChanged; }

        public void AddFinishedMap(string id)
        {
            if (finishedMapIDs.Contains(id)) return;
            finishedMapIDs.Add(id);
        }
    }
}
