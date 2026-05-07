using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelection : MonoBehaviour
{
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] WorldCube defaultWorldCube;
    [SerializeField] Cube playerCube;

    [SerializeField] List<string> backgroundMusicIDs = new();
    private AudioPlayer backgroundMusicAudioPlayer;

    SaveAndLoadManager.GameData gameData;

    private void Start()
    {
        // Spawn player cube
        if (SaveAndLoadManager.GeneralGameData != null)
        {
            gameData = SaveAndLoadManager.GeneralGameData;
        }

        WorldCube lastOpenedWorld = null;
        var allWorldCubes = FindObjectsByType<WorldCube>(FindObjectsSortMode.None);
        foreach(var worldCube in allWorldCubes)
        {
            if (worldCube.WorldName == gameData.LastOpenedWorldName)
            {
                lastOpenedWorld = worldCube;
                break;
            }
        }

        if(lastOpenedWorld == null) lastOpenedWorld = defaultWorldCube;

        if (playerCube != null && lastOpenedWorld != null && playerCube.IsPlayer)
        {
            playerCube.Parent = lastOpenedWorld;
            playerCube.RelativeScale = new(1, 1);
            playerCube.RelativePosition = new(0, 1.5f);
            playerCube.Init();

            var targetPosInGrid = lastOpenedWorld.ChildGrid.GetEnterPosition(CubeMovement.GridDirections.Down, playerCube.RelativePosition);
            lastOpenedWorld.ChildGrid.Children[targetPosInGrid.x, targetPosInGrid.y].Cube = playerCube;
            lastOpenedWorld.ExternalCube = playerCube;

            Vector2 targetRScl = new(1.0f / lastOpenedWorld.ChildGrid.Tiling.y, 1.0f / lastOpenedWorld.ChildGrid.Tiling.x);
            Vector2 targetRPos = new(0, 1.0f - targetRScl.y);

            var playerCubeMovement = playerCube.GetComponent<CubeMovement>();
            if (playerCubeMovement != null)
            {
                playerCubeMovement.StartMoving(playerCube.RelativePosition, playerCube.RelativeScale, targetRPos, targetRScl, 0.5f, lastOpenedWorld, true);
            }
        }

        // Play background music
        if (SoundsManager.Instance != null)
        {
            backgroundMusicAudioPlayer = SoundsManager.Instance.GetBackgroundMusicPlayer();
            if (backgroundMusicAudioPlayer != null && !backgroundMusicAudioPlayer.IsPlaying)
            {
                foreach(var audioClipID in backgroundMusicIDs) backgroundMusicAudioPlayer.AddToPlayList(SoundsManager.Instance.GetAudio(audioClipID));
                backgroundMusicAudioPlayer.SetPlayMode(AudioPlayer.PlayModes.Random);
                backgroundMusicAudioPlayer.Play();
            }
        }
    }

    private void Update()
    {
        if (playerCube != null && gameData != null)
        {
            if (playerCube.Parent != null && playerCube.Parent is WorldCube)
            {
                gameData.LastOpenedWorldName = (playerCube.Parent as WorldCube).WorldName;
            }
        }
    }
    public void OnReturnToMainMenu()
    {
        SceneManager.LoadSceneAsync(mainMenuSceneName);
    }
    public void OnQuitGame()
    {
        Application.Quit();
    }
}
