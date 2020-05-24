
public static class Konane
{
    public abstract class BoardGame
    {
        public interface ICheckable
        {
            int id { get; }
            bool isBack { get; set; }
            bool isForward { get; set; }
            bool isLeft { get; set; }
            bool isRight { get; set; }
            bool zZ { get; set; }
        }

        public const int MAP_ID_OFFSET = 1;
        public const int MAP_OFFSET = 1;
        public const int NON_PICKED = MAP_ID_OFFSET - 1;

        public int picked { get; private set; }

        public ICheckable[][] checkers = null;
        public ICheckable[][] checkedMaps = null;
        public Point[] points = null;
        public int x = 0;
        public int y = 0;

        public int CancelPickedTarget()
        {
            int picekd = this.picked;
            if (picked != NON_PICKED)
            {
                OnPut(picked);
                this.picked = NON_PICKED;
            }
            return picked;
        }

        public int[][] Check(int id)
        {
            return CheckOut(points[id].x, points[id].y, null, out _);
        }

        public int[][] Check(int x, int y)
        {
            return Check(checkers[y][x].id);
        }

        public ICheckable GetChecker(int id)
        {
            return checkedMaps[points[id].y][points[id].x];
        }

        public ICheckable GetChecker(int x, int y)
        {
            return checkers[y][x];
        }

        public int GetColor(int id)
        {
            return points[id].color;
        }

        public int GetColor(int x, int y)
        {
            return GetColor(checkers[y][x].id);
        }

        public void Init(int x, int y, string file = null)
        {
            NilChecker nil = new NilChecker();
            int xMap = x + (MAP_OFFSET << 1);
            int yMap = y + (MAP_OFFSET << 1);
            checkedMaps = new BoardGame.ICheckable[yMap][];
            for (int i = 0; i < yMap; ++i)
            {
                checkedMaps[i] = new BoardGame.ICheckable[xMap];
                for (int j = 0; j < xMap; ++j)
                {
                    checkedMaps[i][j] = nil;
                }
            }
            checkers = new BoardGame.ICheckable[y][];
            points = new Point[MAP_ID_OFFSET + x * y];
            for (int i = 0; i < MAP_ID_OFFSET; ++i)
            {
                points[i] = Point.NaN;
            }
            for (int i = 0, id = MAP_ID_OFFSET; i < y; ++i)
            {
                checkers[i] = new BoardGame.ICheckable[x];
                for (int j = 0; j < x; ++j, ++id)
                {
                    Checker checker = new Checker(id);
                    int iOffset = i + MAP_OFFSET;
                    int jOffset = j + MAP_OFFSET;
                    checkers[i][j] = checker;
                    checkedMaps[iOffset][jOffset] = checker;
                    points[id].color = (i & 1) == (j & 1) ? 0 : 1;
                    points[id].x = jOffset;
                    points[id].y = iOffset;
                }
            }
            try
            {
                if (file != null)
                {
                    string[] fileEach = file.Split(';');
                    string filePicked = fileEach[0];
                    string fileValues = fileEach[1];
                    if (!string.IsNullOrEmpty(fileValues))
                    {
                        foreach (string value in fileValues.Split(','))
                        {
                            TakeAway(int.Parse(value));
                        }
                    }
                    picked = int.Parse(filePicked);
                }
                else
                {
                    picked = NON_PICKED;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                this.x = x;
                this.y = y;
            }
        }

        public int[][] Move(int[] checks, int checkIndex)
        {
            int id = checks[0];
            if (id == picked)
            {
                for (int i = 1; i < checks.Length; ++i)
                {
                    TakeAway((id + checks[i]) >> 1);
                    id = checks[i];
                    if (i == checkIndex)
                    {
                        break;
                    }
                }
                picked = id;
                if (checks.Length > checkIndex + 1)
                {
                    int[] checkNews = new int[checks.Length - checkIndex];
                    for (int i = 0; i < checkNews.Length; ++i)
                    {
                        checkNews[i] = checks[checkIndex + i];
                    }
                    return new int[][] { checkNews };
                }
                return new int[0][];
            }
            return null;
        }

        public void Pick(int id)
        {
            CancelPickedTarget();
            OnPickUp(id);
            picked = id;
        }

        public void Pick(int x, int y)
        {
            Pick(checkers[y][x].id);
        }

        public void TakeAway(int id)
        {
            int x = points[id].x;
            int y = points[id].y;
            checkedMaps[y + 1][x].isBack = true;
            checkedMaps[y - 1][x].isForward = true;
            checkedMaps[y][x + 1].isLeft = true;
            checkedMaps[y][x - 1].isRight = true;
            checkedMaps[y][x].zZ = true;
        }

        public void TakeAway(int x, int y)
        {
            TakeAway(checkers[y][x].id);
        }

        public void TakeIn(int id)
        {
            int x = points[id].x;
            int y = points[id].y;
            checkedMaps[y][x].zZ = false;
            ICheckable checker = null;
            checker = checkedMaps[y - 1][x];
            checker.isForward = false;
            checkedMaps[y][x].isBack = checker.zZ;
            checker = checkedMaps[y + 1][x];
            checker.isBack = false;
            checkedMaps[y][x].isForward = checker.zZ;
            checker = checkedMaps[y][x - 1];
            checker.isRight = false;
            checkedMaps[y][x].isLeft = checker.zZ;
            checker = checkedMaps[y][x + 1];
            checker.isLeft = false;
            checkedMaps[y][x].isRight = checker.zZ;
        }

        public void TakeIn(int x, int y)
        {
            TakeIn(checkers[y][x].id);
        }

        public string ToFile()
        {
            int[] fileNums = new int[points.Length];
            int fileSize = 0;
            for (int i = MAP_OFFSET; i < points.Length; ++i)
            {
                ICheckable checker = checkedMaps[points[i].y][points[i].x];
                if (checker.zZ)
                {
                    fileNums[fileSize++] = checker.id;
                }
            }
            if (fileSize != 0)
            {
                object[] fileVals = new object[fileSize];
                for (int i = 0; i < fileSize; ++i)
                {
                    fileVals[i] = fileNums[i];
                }
                return string.Concat(picked, ";", string.Join(",", fileVals));
            }
            return string.Concat(picked, ";");
        }

        protected int[] CheckIn(int xMap, int yMap, int[] cache)
        {
            int id = checkedMaps[yMap][xMap].id;
            if (cache == null)
            {
                return new int[] { id };
            }
            int[] cacheNew = new int[cache.Length + 1];
            cacheNew[cache.Length] = id;
            cache.CopyTo(cacheNew, 0);
            return cacheNew;
        }

        protected int[][] CheckOut(int xMap, int yMap, int[] cache, out int[] jumpCache)
        {
            jumpCache = CheckIn(xMap, yMap, cache);
            int[][][] jumpTree = new int[][][] {
                ToJumpBack(xMap, yMap - 1, jumpCache),
                ToJumpForward(xMap, yMap + 1, jumpCache),
                ToJumpLeft(xMap - 1, yMap, jumpCache),
                ToJumpRight(xMap + 1, yMap, jumpCache)
            };
            int[][] jumpResults = new int[0][];
            for (int i = 0; i < jumpTree.Length; ++i)
            {
                if (jumpTree[i] != null && jumpTree[i].Length != 0)
                {
                    int[][] jumpNewResults = new int[jumpResults.Length + jumpTree[i].Length][];
                    jumpTree[i].CopyTo(jumpNewResults, jumpResults.Length);
                    jumpResults.CopyTo(jumpNewResults, 0);
                    jumpResults = jumpNewResults;
                }
            }
            return jumpResults;
        }

        protected virtual void OnPickUp(int id)
        {
            TakeAway(id);
        }

        protected virtual void OnPut(int id)
        {
            TakeIn(id);
        }

        protected abstract int[][] ToJumpBack(int xMap, int yMap, int[] cache);
        protected abstract int[][] ToJumpForward(int xMap, int yMap, int[] cache);
        protected abstract int[][] ToJumpLeft(int xMap, int yMap, int[] cache);
        protected abstract int[][] ToJumpRight(int xMap, int yMap, int[] cache);
    }

    public class Checker : BoardGame.ICheckable
    {
        public const int BACK = 2;
        public const int FORWARD = 4;
        public const int LEFT = 8;
        public const int RIGHT = 16;
        public const int SLEEP = 32;

        public int id { get; private set; }

        public bool isBack
        {
            get { return (state & BACK) == BACK; }
            set { state = value ? (state | BACK) : (state & (~BACK)); }
        }

        public bool isForward
        {
            get { return (state & FORWARD) == FORWARD; }
            set { state = value ? (state | FORWARD) : (state & (~FORWARD)); }
        }

        public bool isLeft
        {
            get { return (state & LEFT) == LEFT; }
            set { state = value ? (state | LEFT) : (state & (~LEFT)); }
        }

        public bool isRight
        {
            get { return (state & RIGHT) == RIGHT; }
            set { state = value ? (state | RIGHT) : (state & (~RIGHT)); }
        }

        public bool zZ
        {
            get { return (state & SLEEP) == SLEEP; }
            set { state = value ? SLEEP : 0; }
        }

        public int state = 0;

        public Checker(int id)
        {
            this.id = id;
        }
    }

    public class NilChecker : BoardGame.ICheckable
    {
        public int id { get { return 0; } }
        public bool isBack { get { return false; } set {} }
        public bool isForward { get { return false; } set {} }
        public bool isLeft { get { return false; } set {} }
        public bool isRight { get { return false; } set {} }
        public bool zZ { get { return false; } set {} }
    }

    public struct Point
    {
        public static readonly Point NaN = new Point(-1, -1);

        public int color;
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.color = -1;
            this.x = x;
            this.y = y;
        }
    }
}
