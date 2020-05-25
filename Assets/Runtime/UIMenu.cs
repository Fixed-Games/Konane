using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenu : MonoBehaviour, ITriggable
{
    public struct Setting
    {
        public string hash;
        public bool hashReset;
    }

    [SerializeField] private Text appName = null;
    [SerializeField] private Text appVer = null;
    [SerializeField] private UIMenuItem[] gameInits = null;
    [SerializeField] private UIMenuItem[] games = null;

    private Dictionary<int, Setting> gameSettings = null;

    private void Awake()
    {
        appName.text = "Kōnane";
        appVer.text = Application.version;
        string[] hashKeys = new string[] {
            "Classic 6x6",
            "Classic 8x8",
            "Nightmare 9x8"
        };
        gameSettings = new Dictionary<int, Setting>(3 * 2);
        for (int i = 0; i < hashKeys.Length; ++i)
        {
            gameInits[i].context = this;
            gameInits[i].Init(true);
            games[i].context = this;
            games[i].Init(Game.Exists(hashKeys[i]));
            gameSettings[gameInits[i].GetHashCode()] = new Setting() { hash = hashKeys[i], hashReset = true };
            gameSettings[games[i].GetHashCode()] = new Setting() { hash = hashKeys[i], hashReset = false };
        }
    }

    private void OnDestory()
    {
        gameSettings = null;
    }

    void ITriggable.Trigger(int hashCode)
    {
        GameManager.SelectGame(gameSettings[hashCode].hash, gameSettings[hashCode].hashReset);
    }
}
