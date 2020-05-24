
public class KonaneNightmare : Konane.BoardGame
{
    protected override int[][] ToJumpBack(int xMap, int yMap, int[] cache)
    {
        if (IsValid(xMap, yMap, cache, out ICheckable checker) && checker.isBack)
        {
            return JumpNext(xMap, yMap - 1, cache);
        }
        return null;
    }

    protected override int[][] ToJumpForward(int xMap, int yMap, int[] cache)
    {
        if (IsValid(xMap, yMap, cache, out ICheckable checker) && checker.isForward)
        {
            return JumpNext(xMap, yMap + 1, cache);
        }
        return null;
    }

    protected override int[][] ToJumpLeft(int xMap, int yMap, int[] cache)
    {
        if (IsValid(xMap, yMap, cache, out ICheckable checker) && checker.isLeft)
        {
            return JumpNext(xMap - 1, yMap, cache);
        }
        return null;
    }

    protected override int[][] ToJumpRight(int xMap, int yMap, int[] cache)
    {
        if (IsValid(xMap, yMap, cache, out ICheckable checker) && checker.isRight)
        {
            return JumpNext(xMap + 1, yMap, cache);
        }
        return null;
    }

    private bool IsValid(int xMap, int yMap, int[] cache, out ICheckable checker)
    {
        checker = checkedMaps[yMap][xMap];
        if (checker.zZ)
        {
            return false;
        }
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

    private int[][] JumpNext(int xMap, int yMap, int[] cache)
    {
        int[][] jumpResults = CheckOut(xMap, yMap, cache, out int[] jumpCache);
        if (jumpResults.Length != 0)
        {
            return jumpResults;
        }
        return new int[][] { jumpCache };
    }
}
