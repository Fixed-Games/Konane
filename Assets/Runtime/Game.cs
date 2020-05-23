using System.Collections.Generic;
using UnityEngine;

public class Game : ScriptableObject, System.IDisposable
{
    public const int OFFSET = Konane.BoardGame.MAP_ID_OFFSET;

    public bool interactable { get; set; }
    public int round { get { return gameRound; } }
    public int x { get { return dimensions[0]; } }
    public int y { get { return dimensions[1]; } }

    public bool classic = false;
    public bool customized = false;
    public int[] customs = null;
    public int[] dimensions = null;

    private Dictionary<int, int[][]> database = null;
    private bool dataChanged = false;
    private string dataHash = null;
    private Konane.BoardGame gamePlay = null;
    private int gameRound = 0;
    private int gameSubRound = 0;
    private int gameVer = 0;
    private int id = 0;
    private int index = 0;

    public static bool Asleep(int value)
    {
        return (value & 1) != 0;
    }

    public static void Delete(string hash)
    {
        int size = PlayerPrefs.GetInt(hash, 0);
        for (int i = 0; i < size; ++i)
        {
            PlayerPrefs.DeleteKey(hash + i);
        }
        PlayerPrefs.DeleteKey(hash);
        PlayerPrefs.Save();
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public static bool Exists(string hash)
    {
        return PlayerPrefs.HasKey(hash);
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
                dataChanged = true;
                gamePlay.TakeAway(id);
                gameRound++;
                gameSubRound = 0;
                Save();
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
                        else if (TryGetMove(picked, out int[][] data))
                        {
                            for (int i = 0; i < data[index].Length; ++i)
                            {
                                if (data[index][i] == id)
                                {
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
                        dataChanged = true;
                        gamePlay.CancelPickedTarget();
                        gameRound++;
                        gameSubRound = 0;
                        Save();
                        break;
                }
                break;
        }
    }

    public void Clear()
    {
        Delete(dataHash);
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
        var checker = gamePlay.GetChecker(id);
        var value = 0;
        if (checker.zZ)
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
                case 1:
                    if (gamePlay.GetColor(id) == gameRound)
                    {
                        if (customized)
                        {
                            if (checker.isBack || checker.isForward || checker.isLeft || checker.isRight)
                            {
                                value = 2;
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
        Init(string.Empty, true);
    }

    public void Init(string hash, bool hashReset)
    {
        int x = dimensions[0];
        int y = dimensions[1];
        database = new Dictionary<int, int[][]>(x * y);
        dataChanged = true;
        dataHash = hash;
        if (!hashReset && PlayerPrefs.HasKey(hash))
        {
            gamePlay = Create();
            gamePlay.Init(x, y, PlayerPrefs.GetString(hash + 0, null));
            gameRound = PlayerPrefs.GetInt(hash + 1, 0);
            gameSubRound = PlayerPrefs.GetInt(hash + 2, 0);
            gameVer = PlayerPrefs.GetInt(hash + 3, 0);
        }
        else
        {
            gamePlay = Create();
            gamePlay.Init(x, y);
            gameRound = 0;
            gameSubRound = 0;
            gameVer = 1;
        }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(dataHash))
        {
            return;
        }
        const int size = 4;
        PlayerPrefs.SetInt(dataHash, size);
        PlayerPrefs.SetString(dataHash + 0, gamePlay.ToFile());
        PlayerPrefs.SetInt(dataHash + 1, gameRound);
        PlayerPrefs.SetInt(dataHash + 2, gameSubRound);
        PlayerPrefs.SetInt(dataHash + 3, gameVer);
        PlayerPrefs.Save();
    }

    public void Select(int id, int index)
    {
        this.id = id;
        this.index = index;
    }

    public bool TryGetMove(int id, out int[][] data)
    {
        return database.TryGetValue(id, out data) && data.Length != 0;
    }

    public bool TryGetPickedID(out int id)
    {
        id = gamePlay.picked;
        return id != Konane.BoardGame.NON_PICKED;
    }

    public bool TryRefresh(out int[] dataTips)
    {
        if (gameRound != 0 && gameRound != 1)
        {
            if (dataChanged)
            {
                dataChanged = false;
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
            dataTips = dataList.ToArray();
            return true;
        }
        dataTips = null;
        return false;
    }

    private void OnDestroy()
    {
        database = null;
        gamePlay = null;
    }
}
