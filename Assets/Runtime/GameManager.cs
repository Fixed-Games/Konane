using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    public static void Home()
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.LoadAsync(1));
        }
    }

    public static void SelectGame(string hash, bool hashReset)
    {
        if (Instance != null)
        {
            Instance.hash = hash;
            Instance.hashReset = hashReset;
            Instance.json = Resources.Load<TextAsset>(hash)?.text;
            Instance.StartCoroutine(Instance.LoadAsync(2));
        }
    }

    public static Game StartGame()
    {
        Game game = ScriptableObject.CreateInstance<Game>();
        if (Instance != null)
        {
            JsonUtility.FromJsonOverwrite(Instance.json, game);
            game.Init(Instance.hash ??  string.Empty, Instance.hashReset);
        }
        else
        {
            JsonUtility.FromJsonOverwrite(
                @"{""classic"":true,""customized"":true,""customs"":[1,4,6,7,10,11,13,16],""dimensions"":[4,4]}",
                game);
            game.Init();
        }
        return game;
    }

    private int buildIndex = 0;
    private string hash = null;
    private bool hashReset = false;
    private string json = null;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator LoadAsync(int buildIndex)
    {
        if (buildIndex != 0)
        {
            if (this.buildIndex != 0)
            {
                yield return SceneManager.UnloadSceneAsync(this.buildIndex);
            }
            yield return Resources.UnloadUnusedAssets();
            yield return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByBuildIndex(buildIndex);
            if (SceneManager.SetActiveScene(scene))
            {
                this.buildIndex = buildIndex;
                yield break;
            }
        }
        SceneManager.LoadScene(0);
    }

    private IEnumerator Start()
    {
        yield return LoadAsync(1);
    }
}
