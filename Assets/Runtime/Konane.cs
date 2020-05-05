
public static class Konane
{
    public class BoardGame
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

        public int picked { get; private set; }

        public ICheckable[][] checkers = null;
        public ICheckable[][] checkedMaps = null;
        public Point[] points = null;
        public int x = 0;
        public int y = 0;

        public BoardGame(int x, int y)
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
                    points[id].group = (i & 1) == (j & 1) ? 0 : 1;
                    points[id].x = jOffset;
                    points[id].y = iOffset;
                }
            }
            this.x = x;
            this.y = y;
        }

        public void CancelPickedTarget()
        {
            picked = 0;
        }

        public int[][] Check(int id)
        {
            return ToMove(points[id].x, points[id].y, new int[] { id });
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

        public int GetGroup(int id)
        {
            return points[id].group;
        }

        public int GetGroup(int x, int y)
        {
            return points[checkers[y][x].id].group;
        }

        public void Move(int[] checks, int checkIndex)
        {
            int id = checks[0];
            if (id == picked)
            {
                TakeAway(id);
                for (int i = 1; i < checks.Length; ++i)
                {
                    TakeAway((id + checks[i]) >> 1);
                    id = checks[i];
                    if (i == checkIndex)
                    {
                        break;
                    }
                }
                TakeIn(id);
                picked = id;
            }
        }

        public void Pick(int id)
        {
            picked = id;
        }

        public void Pick(int x, int y)
        {
            picked = checkers[y][x].id;
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

        private int[][] ToJumpBack(int xMap, int yMap, int[] cache)
        {
            ICheckable checker = checkedMaps[yMap][xMap];
            if (TryJump(checker, checker.isBack, cache))
            {
                return ToMoveByJump(xMap, yMap - 1, cache);
            }
            return null;
        }

        private int[][] ToJumpForward(int xMap, int yMap, int[] cache)
        {
            ICheckable checker = checkedMaps[yMap][xMap];
            if (TryJump(checker, checker.isForward, cache))
            {
                return ToMoveByJump(xMap, yMap + 1, cache);
            }
            return null;
        }

        private int[][] ToJumpLeft(int xMap, int yMap, int[] cache)
        {
            ICheckable checker = checkedMaps[yMap][xMap];
            if (TryJump(checker, checker.isLeft, cache))
            {
                return ToMoveByJump(xMap - 1, yMap, cache);
            }
            return null;
        }

        private int[][] ToJumpRight(int xMap, int yMap, int[] cache)
        {
            ICheckable checker = checkedMaps[yMap][xMap];
            if (TryJump(checker, checker.isRight, cache))
            {
                return ToMoveByJump(xMap + 1, yMap, cache);
            }
            return null;
        }

        private int[][] ToMove(int xMap, int yMap, int[] cache)
        {
            int[][][] jumpTree = new int[][][] {
                ToJumpBack(xMap, yMap - 1, cache),
                ToJumpForward(xMap, yMap + 1, cache),
                ToJumpLeft(xMap - 1, yMap, cache),
                ToJumpRight(xMap + 1, yMap, cache)
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
            if (jumpResults.Length == 0 && cache.Length > 1)
            {
                jumpResults = new int[][] { cache };
            }
            return jumpResults;
        }

        private int[][] ToMoveByJump(int xMap, int yMap, int[] cache)
        {
            int[] cacheAppend = new int[cache.Length + 1];
            cacheAppend[cache.Length] = checkedMaps[yMap][xMap].id;
            cache.CopyTo(cacheAppend, 0);
            return ToMove(xMap, yMap, cacheAppend);
        }

        private bool TryJump(ICheckable checker, bool @checked, int[] cache)
        {
            if (checker.zZ)
            {
                return false;
            }
            if (@checked)
            {
                int cached = cache[0];
                for (int i = 1; i < cache.Length; ++i)
                {
                    if (checker.id == ((cached + cache[i]) >> 1))
                    {
                        return false;
                    }
                    cached = cache[i];
                }
                return true;
            }
            return false;
        }
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

        public int group;
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.group = -1;
            this.x = x;
            this.y = y;
        }
    }
}
