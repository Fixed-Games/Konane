using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBoardGame : MonoBehaviour, ITriggable
{
    [SerializeField] private Button board = null;
    [SerializeField] private CanvasGroup boardGroup = null;
    [SerializeField] private AspectRatioFitter gameRatioFitter = null;
    [SerializeField] private RectTransform originalRoot = null;
    [SerializeField] private GameObject originalUI = null;
    [SerializeField] private Button select = null;

    private int[] checkedTips = null;
    private Game game = null;
    private Dictionary<int, int> gameUI = null;
    private int selected = 0;
    private UIChecker[] uiCheckers = null;
    private float uiDeltaTime = 0f;
    private int uiRound = 0;
    private float uiRoundTime = 0f;

    private void Awake()
    {
        board.onClick.AddListener(OnBoard);
        boardGroup.interactable = false;
        game = GameManager.StartGame();
        game.Init();
        gameUI = new Dictionary<int, int>(game.x * game.y);
        select.onClick.AddListener(OnSelect);
    }

    private void OnBoard()
    {
        boardGroup.interactable = false;
        game.B();
        for (int i = Game.NIL_OFFSET; i < uiCheckers.Length; ++i)
        {
            uiCheckers[i].Clear();
        }
        uiDeltaTime = 0.5f;
    }

    private void OnBoardChecker(int id)
    {
        boardGroup.interactable = false;
        game.Select(id, selected);
        game.A();
        for (int i = Game.NIL_OFFSET; i < uiCheckers.Length; ++i)
        {
            uiCheckers[i].Clear();
        }
        uiDeltaTime = 0.5f;
    }

    private void OnBoardUpdate()
    {
        for (int i = Game.NIL_OFFSET; i < uiCheckers.Length; ++i)
        {
            int value = game.GetCheckedValue(i, out bool asleep);
            if (Game.IsActived(value))
            {
                if (Game.IsPicked(value))
                {
                    OnColor(i, Color.yellow);
                }
                if (asleep)
                {
                    uiCheckers[i].SleepWithCheck();
                }
                else
                {
                    uiCheckers[i].Check();
                }
            }
            else
            {
                if (Game.IsPicked(value))
                {
                    OnColor(i, Color.white);
                }
                if (asleep)
                {
                    uiCheckers[i].Sleep();
                }
                else
                {
                    uiCheckers[i].CheckOff();
                }
            }
        }
        boardGroup.interactable = true;
    }

    private void OnColor(int id, Color color)
    {
        uiCheckers[id].SetInfo(color);
    }

    private void OnDestroy()
    {
        game?.Dispose();
        game = null;
        gameUI = null;
    }

    private void OnGameOver()
    {
        Debug.LogWarningFormat("PLAYER_{0} FAILED", game.GetRoundGroup());
    }

    private void OnSelect()
    {
        int picked = game.GetPickedID();
        if (picked != Game.NIL)
        {
            if (game.TryGetSearch(picked, out int[][] results))
            {
                for (int i = Game.NIL_OFFSET; i < uiCheckers.Length; ++i)
                {
                    uiCheckers[i].Clear();
                }
                selected = (selected + 1) % results.Length;
                for (int i = 2; i < results[selected].Length; ++i)
                {
                    OnColor(results[selected][i], Color.white);
                }
                OnColor(results[selected][1], Color.yellow);
                if (results[selected].Length > 2)
                    OnColor(results[selected][2], Color.yellow);
                uiDeltaTime = 1f;
            }
        }
        else
        {
            if (checkedTips.Length != 0)
            {
                for (int i = 0; i < checkedTips.Length; ++i)
                {
                    uiCheckers[checkedTips[i]].SetInfoWarning();
                }
            }
            else
            {
                GameManager.Quit();
            }
        }
    }

    private void OnRound()
    {
        checkedTips = game.Refresh();
        if (checkedTips == null)
        {
            checkedTips = new int[0];
        }
        else
        {
            if (checkedTips.Length == 0)
            {
                OnGameOver();
            }
        }
    }

    private void Start()
    {
        gameRatioFitter.aspectRatio = (float)game.x / game.y;
        gameRatioFitter.enabled = true;
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
        uiCheckers = new UIChecker[Game.NIL_OFFSET + game.x * game.y];
        for (int i = 0; i < game.y; ++i)
        {
            for (int j = 0; j < game.x; ++j)
            {
                var group = game.GetGroup(j, i);
                var id = game.GetID(j, i);
                var target = Object.Instantiate<GameObject>(originalUI, originalRoot, false);
                var targetRect = target.GetComponent<RectTransform>();
                targetRect.anchorMin = new Vector2(xMaps[j], yMaps[i + 1]);
                targetRect.anchorMax = new Vector2(xMaps[j + 1], yMaps[i]);
                uiCheckers[id] = target.GetComponent<UIChecker>();
                uiCheckers[id].context = this;
                uiCheckers[id].Init(group == 0 ? Color.black : Color.white);
                gameUI[uiCheckers[id].GetHashCode()] = id;
            }
        }
        OnRound();
    }

    private void Update()
    {
        const float frequency = 1f;
        float deltaTime = Time.deltaTime;
        uiDeltaTime += deltaTime;
        uiRoundTime += deltaTime;
        if (uiRound != game.round)
        {
            uiRound = game.round;
            uiRoundTime = 0f;
            OnRound();
        }
        if (uiDeltaTime > frequency)
        {
            uiDeltaTime = 0f;
            OnBoardUpdate();
            for (int i = Game.NIL_OFFSET; i < uiCheckers.Length; ++i)
            {
                uiCheckers[i].Repaint();
            }
        }
        else
        {
            for (int i = Game.NIL_OFFSET; i < uiCheckers.Length; ++i)
            {
                uiCheckers[i].Repaint(deltaTime);
            }
        }
    }

    void ITriggable.Trigger(int hashCode)
    {
        int id = gameUI[hashCode];
        OnBoardChecker(id);
        int picked = game.GetPickedID();
        if (picked != Game.NIL)
        {
            if (game.TryGetSearch(picked, out int[][] results))
            {
                for (int i = 2; i < results[0].Length; ++i)
                {
                    OnColor(results[0][i], Color.white);
                }
                OnColor(results[0][1], Color.yellow);
                if (results[0].Length > 2)
                    OnColor(results[0][2], Color.yellow);
                selected = 0;
            }
            else
            {
                game.B();
                uiCheckers[picked].SetInfoError();
            }
        }
    }
}
