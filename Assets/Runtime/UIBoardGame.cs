using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBoardGame : MonoBehaviour, ITriggable
{
    public const float FREQUENCY = 1f;
    public const float T = 1f / FREQUENCY;

    [SerializeField] private Button board = null;
    [SerializeField] private Button boardSelect = null;
    [SerializeField] private CanvasGroup boardView = null;
    [SerializeField] private AspectRatioFitter gameRatioFitter = null;
    [SerializeField] private RectTransform originalRoot = null;
    [SerializeField] private GameObject originalUI = null;

    private Game game = null;
    private Dictionary<int, int> gameUI = null;
    private int selected = 0;
    private int[] tips = null;
    private UIChecker[] uiCheckers = null;
    private float uiDeltaTime = 0f;
    private int uiRound = 0;
    private float uiRoundTime = 0f;

    private void Awake()
    {
        board.onClick.AddListener(OnBoard);
        boardSelect.onClick.AddListener(OnBoardSelect);
        boardView.blocksRaycasts = false;
    }

    private void OnBoard()
    {
        game.B();
        for (int i = Game.OFFSET; i < uiCheckers.Length; ++i)
        {
            uiCheckers[i].Clear();
        }
        uiDeltaTime = T;
    }

    private void OnBoardChecker(int hashCode)
    {
        int id = gameUI[hashCode];
        game.Select(id, selected);
        game.A();
        for (int i = Game.OFFSET; i < uiCheckers.Length; ++i)
        {
            uiCheckers[i].Clear();
        }
        uiDeltaTime = T * 0.8f;
    }

    private void OnBoardSelect()
    {
        if (game.TryGetPickedID(out int id))
        {
            for (int i = Game.OFFSET; i < uiCheckers.Length; ++i)
            {
                uiCheckers[i].Clear();
            }
            OnBoardSelect(id, selected + 1);
        }
        else
        {
            if (tips != null)
            {
                if (tips.Length != 0)
                {
                    for (int i = 0; i < tips.Length; ++i)
                    {
                        uiCheckers[tips[i]].SetInfoWarning();
                    }
                }
                else
                {
                    GameManager.Quit();
                }
            }
        }
    }

    private void OnBoardSelect(int id, int index)
    {
        if (game.TryGetData(id, out int[][] data))
        {
            selected = index < data.Length ? index : 0;
            for (int i = 0; i < data[selected].Length; ++i)
            {
                int point = data[selected][i];
                uiCheckers[point].SetInfo(Color.yellow);
                uiCheckers[point].Repaint();
            }
        }
        else
        {
            game.B();
            uiCheckers[id].SetInfoError();
            uiDeltaTime = T;
        }
    }

    private void OnDestroy()
    {
        game?.Dispose();
        game = null;
        gameUI = null;
    }

    private void OnGame()
    {
        for (int i = Game.OFFSET; i < uiCheckers.Length; ++i)
        {
            int value = game.GetCheckedValue(i);
            if (Game.Asleep(value))
            {
                if (Game.IsActived(value))
                {
                    if (Game.IsPicked(value))
                    {
                        uiCheckers[i].Check();
                    }
                    else
                    {
                        uiCheckers[i].SleepWithCheck();
                    }
                }
                else
                {
                    if (Game.IsPicked(value))
                    {
                        uiCheckers[i].CheckOff();
                    }
                    else
                    {
                        uiCheckers[i].Sleep();
                    }
                }
            }
            else
            {
                if (Game.IsActived(value))
                {
                    uiCheckers[i].Check();
                }
                else
                {
                    uiCheckers[i].CheckOff();
                }
            }
            uiCheckers[i].Repaint();
        }
    }

    private void OnGameOver()
    {
        Debug.LogWarningFormat("PLAYER_{0} FAILED", game.GetRoundColor());
    }

    private void OnGameUpdate(float deltaTime)
    {
        for (int i = Game.OFFSET; i < uiCheckers.Length; ++i)
        {
            uiCheckers[i].Clear(deltaTime);
        }
    }

    private void OnRound()
    {
        if (game.TryRefresh(out tips))
        {
            if (tips.Length == 0)
            {
                boardView.blocksRaycasts = false;
                boardView.interactable = false;
                game.interactable = false;
                for (int i = Game.OFFSET; i < uiCheckers.Length; ++i)
                {
                    int value = game.GetCheckedValue(i);
                    if (Game.IsActived(value))
                    {
                        uiCheckers[i].SetInfo(Color.red);
                    }
                }
                OnGameOver();
            }
        }
    }

    private void Start()
    {
        game = GameManager.StartGame();
        game.Init();
        game.interactable = true;
        gameRatioFitter.aspectRatio = (float)game.x / game.y;
        gameRatioFitter.enabled = true;
        gameUI = new Dictionary<int, int>(game.x * game.y);
        float[] xMaps = new float[game.x + 1];
        float[] yMaps = new float[game.y + 1];
        for (int i = 1; i < xMaps.Length; ++i)
        {
            xMaps[i] = (float)i / game.x;
        }
        for (int i = 0; i < yMaps.Length; ++i)
        {
            yMaps[i] = 1f - (float)i / game.y;
        }
        uiCheckers = new UIChecker[Game.OFFSET + game.x * game.y];
        for (int i = 0; i < game.y; ++i)
        {
            for (int j = 0; j < game.x; ++j)
            {
                var color = game.GetColor(j, i);
                var id = game.GetID(j, i);
                var target = Object.Instantiate<GameObject>(originalUI, originalRoot, false);
                var targetRect = target.GetComponent<RectTransform>();
                targetRect.anchorMin = new Vector2(xMaps[j], yMaps[i + 1]);
                targetRect.anchorMax = new Vector2(xMaps[j + 1], yMaps[i]);
                uiCheckers[id] = target.GetComponent<UIChecker>();
                uiCheckers[id].context = this;
                uiCheckers[id].Init(color == 0 ? Color.black : Color.white);
                gameUI[uiCheckers[id].GetHashCode()] = id;
            }
        }
        uiRound = game.round;
        uiRoundTime = 0f;
        OnRound();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        uiDeltaTime += deltaTime;
        uiRoundTime += deltaTime;
        if (uiRound != game.round)
        {
            uiRound = game.round;
            uiRoundTime = 0f;
            OnRound();
        }
        if (uiDeltaTime > T)
        {
            uiDeltaTime = 0f;
            OnGame();
            if (game.interactable)
            {
                if (boardView.blocksRaycasts == false)
                {
                    boardView.blocksRaycasts = true;
                    boardView.interactable = true;
                    if (game.TryGetPickedID(out int id))
                    {
                        OnBoardSelect(id, 0);
                    }
                }
            }
        }
        else
        {
            OnGameUpdate(deltaTime);
        }
    }

    void ITriggable.Trigger(int hashCode)
    {
        boardView.blocksRaycasts = false;
        boardView.interactable = false;
        OnBoardChecker(hashCode);
    }
}
