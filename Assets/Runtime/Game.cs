using System.Collections.Generic;
using UnityEngine;

public class Game : ScriptableObject, System.IDisposable
{
    public const int NIL = 0;
    public const int NIL_OFFSET = Konane.BoardGame.MAP_ID_OFFSET;

    public int round { get { return gameRound; } }
    public int x { get { return gamePlay.x; } }
    public int y { get { return gamePlay.y; } }

    public bool customized = false;
    public int[] customs = null;
    public int[] dimensions = null;

    private bool changed = false;
    private Konane.BoardGame gamePlay = null;
    private int gameRound = 0;
    private int gameRoundStep = 0;
    private Dictionary<int, int[][]> gameSearch = null;
    private int id = NIL;
    private int selected = 0;

    public static bool IsActived(int value)
    {
        return (value & 1) != 0;
    }

    public static bool IsPicked(int value)
    {
        return (value & 2) != 0;
    }

    public void A()
    {
        switch (gameRound)
        {
            case 0:
            case 1:
                changed = true;
                gamePlay.TakeAway(id);
                gameRound++;
                break;
            default:
                switch (gameRoundStep)
                {
                    case 0:
                        gamePlay.Pick(id);
                        gameRoundStep = 1;
                        break;
                    case 1:
                    case 2:
                        int picked = gamePlay.picked;
                        if (picked == id)
                        {
                            gamePlay.CancelPickedTarget();
                            gameRoundStep = 0;
                        }
                        else if (TryGetSearch(picked, out int[][] results))
                        {
                            int[] result = results[selected];
                            for (int i = 0; i < result.Length; ++i)
                            {
                                if (result[i] == id)
                                {
                                    changed = true;
                                    gamePlay.Move(result, i);
                                    gameRoundStep = 2;
                                    gameSearch[id] = gamePlay.Check(id);
                                    break;
                                }
                            }
                        }
                        break;
                }
                break;
        }
        id = NIL;
    }

    public void B()
    {
        switch (gameRound)
        {
            case 0:
            case 1:
                break;
            default:
                switch (gameRoundStep)
                {
                    case 0:
                        break;
                    case 1:
                        gamePlay.CancelPickedTarget();
                        gameRoundStep = 0;
                        break;
                    case 2:
                        changed = true;
                        gamePlay.CancelPickedTarget();
                        gameRound++;
                        gameRoundStep = 0;
                        break;
                }
                break;
        }
    }

    public void Dispose()
    {
        Object.Destroy(this);
    }

    public int GetCheckedValue(int id, out bool asleep)
    {
        asleep = gamePlay.GetChecker(id).zZ;
        int value = 0;
        switch (gameRound)
        {
            case 0:
            case 1:
                if (asleep == false && gamePlay.GetGroup(id) == gameRound)
                {
                    if (customized)
                    {
                        for (int i = 0; i < customs.Length; ++i)
                        {
                            if (id == customs[i])
                            {
                                value = 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        value = 1;
                    }
                }
                break;
            default:
                switch (gameRoundStep)
                {
                    case 0:
                        if (asleep == false && gamePlay.GetGroup(id) == GetRoundGroup())
                        {
                            value = 1;
                        }
                        break;
                    case 1:
                        if (asleep)
                        {
                            value = 1;
                        }
                        else if (gamePlay.picked == id)
                        {
                            value = 1 | 2;
                        }
                        break;
                    case 2:
                        if (asleep)
                        {
                            value = 1;
                        }
                        else if (gamePlay.picked == id)
                        {
                            value = 2;
                        }
                        break;
                }
                break;
        }
        return value;
    }

    public int GetGroup(int x, int y)
    {
        return gamePlay.GetGroup(x, y);
    }

    public int GetID(int x, int y)
    {
        return gamePlay.GetChecker(x, y).id;
    }

    public int GetPickedID()
    {
        return gamePlay.picked;
    }

    public int GetRoundGroup()
    {
        return gameRound & 1;
    }

    public void Init()
    {
        gamePlay = new Konane.BoardGame(dimensions[0], dimensions[1]);
        gameRound = 0;
        gameRoundStep = 0;
        gameSearch = new Dictionary<int, int[][]>(gamePlay.x * gamePlay.y);
    }

    public int[] Refresh()
    {
        if (gameRound != 0 && gameRound != 1)
        {
            if (changed)
            {
                changed = false;
                for (int i = 0; i < gamePlay.y; ++i)
                {
                    for (int j = 0; j < gamePlay.x; ++j)
                    {
                        var checker = gamePlay.GetChecker(j, i);
                        if (checker.zZ)
                        {
                            gameSearch.Remove(checker.id);
                        }
                        else
                        {
                            gameSearch[checker.id] = gamePlay.Check(j, i);
                        }
                    }
                }
            }
            List<int> dataList = new List<int>();
            foreach (var data in gameSearch)
            {
                if (gamePlay.GetGroup(data.Key) == GetRoundGroup())
                {
                    if (data.Value.Length != 0)
                    {
                        dataList.Add(data.Key);
                    }
                }
            }
            return dataList.ToArray();
        }
        return null;
    }

    public void Select(int id, int selected)
    {
        this.id = id;
        this.selected = selected;
    }

    public bool TryGetSearch(int id, out int[][] results)
    {
        return gameSearch.TryGetValue(id, out results) && results.Length != 0;
    }

    public int[][] UpdateSearch(int id)
    {
        gameSearch[id] = gamePlay.Check(id);
        return gameSearch[id];
    }

    private void OnDestroy()
    {
        gamePlay = null;
        gameSearch = null;
    }
}
