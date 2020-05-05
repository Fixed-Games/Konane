using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    public static void Quit()
    {
        SceneManager.LoadScene(0);
    }

    public static Game StartGame()
    {
        var game = ScriptableObject.CreateInstance<Game>();
        var gameJson = Resources.Load<TextAsset>("Game Settings")?.text;
        JsonUtility.FromJsonOverwrite(gameJson, game);
        return game;
    }

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return Resources.UnloadUnusedAssets();
        yield return SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }
}
