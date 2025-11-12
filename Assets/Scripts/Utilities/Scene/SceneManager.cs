using System;
using UnityEngine;

public struct LoadSceneEvent
{
    public string SceneName;

    public LoadSceneEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}

public struct ReloadCurrentSceneEvent
{
}

public class SceneManager : PersistentSingleton<SceneManager>
{
    public string titleSceneName = "Title";
    public string gameSceneName = "Game";
    public string TutorialSceneName = "Tutorial";
    public string ScoreSceneName = "Score";

    protected override void Awake()
    {
        base.Awake();

        EventBetter.Listen(this, (LoadSceneEvent e) => { LoadScene(e.SceneName); });
        EventBetter.Listen(this, (ReloadCurrentSceneEvent e) => { Reload(); });
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadTitleScene()
    {
        LoadScene(titleSceneName);
    }

    public void LoadGameScene()
    {
        LoadScene(gameSceneName);
    }

    public void LoadTutorialScene()
    {
        LoadScene(TutorialSceneName);   
    }
    public void LoadScoreScene()
    {
        LoadScene(ScoreSceneName);
    }
    private void Update()
    {
        // if (InputManager.Instance.RestartAction.WasPerformedThisFrame())
        // {
        //     Reload();
        // }
    }

    private void Reload()
    {
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}