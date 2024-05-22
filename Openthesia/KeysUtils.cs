namespace Openthesia;

public class KeysUtils
{
    public static bool HasBlack(int key)
    {
        return (!((key - 1) % 7 == 0 || (key - 1) % 7 == 3) && key != 51);
    }
}
