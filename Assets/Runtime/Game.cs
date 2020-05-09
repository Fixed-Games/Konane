using System.Collections.Generic;
using UnityEngine;

public class Game : ScriptableObject, System.IDisposable
{
    public const int OFFSET = Konane.BoardGame.MAP_ID_OFFSET;

    public bool interactable { get { return true; } }
    public int round { get { return gameRound; } }
    public int x { get { return dimensions[0]; } }
    public int y { get { return dimensions[1]; } }

    public bool classic = false;
    public bool customized = false;
    public int[] customs = null;
    public int[] dimensions = null;

    private bool changed = false;
    private Dictionary<int, int[][]> database = null;
    private Konane.BoardGame gamePlay = null;
    private int gameRound = 0;
    private int gameSubRound = 0;
    private int id = 0;
    private int index = 0;

    public static bool Asleep(int value)
    {
        return (value & 1) != 0;
    }

    public static bool IsActived(int value)
    {
        return (value & 2) != 0;
    }

    public static bool IsPicked(int value)
    {
        return (value & 4) != 0;
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
                switch (gameSubRound)
                {
                    case 0:
                        gamePlay.Pick(id);
                        gameSubRound = 1;
                        database[id] = gamePlay.Check(id);
                        break;
                    case 1:
                    case 2:
                        int picked = gamePlay.picked;
                        if (picked == id)
                        {
                            gamePlay.CancelPickedTarget();
                            gameSubRound = 0;
                        }
                        else if (TryGetData(picked, out int[][] data))
                        {
                            for (int i = 0; i < data[index].Length; ++i)
                            {
                                if (data[index][i] == id)
                                {
                                    changed = true;
                                    database[id] = gamePlay.Move(data[index], i);
                                    gameSubRound = 2;
                                    break;
                                }
                            }
                        }
                        break;
                }
                break;
        }
        id = 0;
    }

    public void B()
    {
        switch (gameRound)
        {
            case 0:
            case 1:
                break;
            default:
                switch (gameSubRound)
                {
                    case 0:
                        break;
                    case 1:
                        gamePlay.CancelPickedTarget();
                        gameSubRound = 0;
                        break;
                    case 2:
                        changed = true;
                        gamePlay.CancelPickedTarget();
                        gameRound++;
                        gameSubRound = 0;
                        break;
                }
                break;
        }
    }

    public Konane.BoardGame Create()
    {
        if (classic)
        {
            return new KonaneClassic();
        }
        return new KonaneNightmare();
    }

    public void Dispose()
    {
        Object.Destroy(this);
    }

    public int GetCheckedValue(int id)
    {
        int value = 0;
        if (gamePlay.GetChecker(id).zZ)
        {
            switch (gameRound)
            {
                case 0:
                case 1:
                    value = 1;
                    break;
                default:
                    switch (gameSubRound)
                    {
                        case 0:
                            value = 1;
                            break;
                        case 1:
                            if (gamePlay.picked == id)
                            {
                                value = 1 | 2 | 4;
                            }
                            else
                            {
                                value = 1 | 2;
                            }
                            break;
                        case 2:
                            if (gamePlay.picked == id)
                            {
                                value = 1 | 4;
                            }
                            else
                            {
                                value = 1 | 2;
                            }
                            break;
                    }
                    break;
            }
        }
        else
        {
            switch (gameRound)
            {
                case 0:
                case 1:
                    if (gamePlay.GetColor(id) == gameRound)
                    {
                        if (customized)
                        {
                            for (int i = 0; i < customs.Length; ++i)
                            {
                                if (id == customs[i])
                                {
                                    value = 2;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            value = 2;
                        }
                    }
                    break;
                default:
                    switch (gameSubRound)
                    {
                        case 0:
                            if (gamePlay.GetColor(id) == GetRoundColor())
                            {
                                value = 2;
                            }
                            break;
                        case 1:
                        case 2:
                            break;
                    }
                    break;
            }
        }
        return value;
    }

    public int GetColor(int x, int y)
    {
        return gamePlay.GetColor(x, y);
    }

    public int GetID(int x, int y)
    {
        return gamePlay.GetChecker(x, y).id;
    }

    public int GetRoundColor()
    {
        return gameRound & 1;
    }

    public void Init()
    {
        int x = dimensions[0];
        int y = dimensions[1];
        database = new Dictionary<int, int[][]>(x * y);
        gamePlay = Create();
        gamePlay.Init(x, y);
        gameRound = 0;
        gameSubRound = 0;
    }

    public bool TryRefresh(out int[] tips)
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
                            database.Remove(checker.id);
                        }
                        else
                        {
                            database[checker.id] = gamePlay.Check(j, i);
                        }
                    }
                }
            }
            List<int> dataList = new List<int>();
            foreach (var data in database)
            {
                if (gamePlay.GetColor(data.Key) == GetRoundColor())
                {
                    if (data.Value.Length != 0)
                    {
                        dataList.Add(data.Key);
                    }
                }
            }
            tips = dataList.ToArray();
            return true;
        }
        tips = null;
        return false;
    }

    public void Select(int id, int index)
    {
        this.id = id;
        this.index = index;
    }

    public bool TryGetData(int id, out int[][] data)
    {
        return database.TryGetValue(id, out data) && data.Length != 0;
    }

    public bool TryGetPickedID(out int id)
    {
        id = gamePlay.picked;
        return id != 0;
    }

    private void OnDestroy()
    {
        database = null;
        gamePlay = null;
    }
}
