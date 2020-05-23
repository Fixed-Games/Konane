

public class KonaneClassic : Konane.BoardGame
{
    protected override bool IsCustomStarter(int x, int y)
    {
        if (x == y)
        {
            if (x == 0 || x == this.x - 1 || x == this.x >> 1 || x == (this.x - 1) >> 1)
            {
                return true;
            }
        }
        return false;
    }

    protected override int[][] ToJumpBack(int xMap, int yMap, int[] cache)
    {
        return JumpBack(xMap, yMap, cache);
    }

    protected override int[][] ToJumpForward(int xMap, int yMap, int[] cache)
    {
        return JumpForward(xMap, yMap, cache);
    }

    protected override int[][] ToJumpLeft(int xMap, int yMap, int[] cache)
    {
        return JumpLeft(xMap, yMap, cache);
    }

    protected override int[][] ToJumpRight(int xMap, int yMap, int[] cache)
    {
        return JumpRight(xMap, yMap, cache);
    }

    private int[][] JumpBack(int xMap, int yMap, int[] cache)
    {
        ICheckable checker = checkedMaps[yMap][xMap];
        if (checker.zZ == false && checker.isBack)
        {
            int[] jumpCache = CheckIn(xMap, yMap - 1, cache);
            int[][] jumpResults = JumpBack(xMap, yMap - 2, jumpCache);
            if (jumpResults == null)
            {
                jumpResults = new int[][] { jumpCache };
            }
            return jumpResults;
        }
        return null;
    }

    private int[][] JumpForward(int xMap, int yMap, int[] cache)
    {
        ICheckable checker = checkedMaps[yMap][xMap];
        if (checker.zZ == false && checker.isForward)
        {
            int[] jumpCache = CheckIn(xMap, yMap + 1, cache);
            int[][] jumpResults = JumpForward(xMap, yMap + 2, jumpCache);
            if (jumpResults == null)
            {
                jumpResults = new int[][] { jumpCache };
            }
            return jumpResults;
        }
        return null;
    }

    private int[][] JumpLeft(int xMap, int yMap, int[] cache)
    {
        ICheckable checker = checkedMaps[yMap][xMap];
        if (checker.zZ == false && checker.isLeft)
        {
            int[] jumpCache = CheckIn(xMap - 1, yMap, cache);
            int[][] jumpResults = JumpLeft(xMap - 2, yMap, jumpCache);
            if (jumpResults == null)
            {
                jumpResults = new int[][] { jumpCache };
            }
            return jumpResults;
        }
        return null;
    }

    private int[][] JumpRight(int xMap, int yMap, int[] cache)
    {
        ICheckable checker = checkedMaps[yMap][xMap];
        if (checker.zZ == false && checker.isRight)
        {
            int[] jumpCache = CheckIn(xMap + 1, yMap, cache);
            int[][] jumpResults = JumpRight(xMap + 2, yMap, jumpCache);
            if (jumpResults == null)
            {
                jumpResults = new int[][] { jumpCache };
            }
            return jumpResults;
        }
        return null;
    }
}
