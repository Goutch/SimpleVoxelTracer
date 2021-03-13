public class VoxelData
{
    public const int SIZE_X = 128, SIZE_Y = 128, SIZE_Z = 128;
    private int[] data = new int[SIZE_X * SIZE_Y * SIZE_Z];
    
    public int this[int x, int z, int y]
    {
        get => data[(x * SIZE_Y * SIZE_Z) + (y * SIZE_Z) + z];
        set => data[(x * SIZE_Y * SIZE_Z) + (y * SIZE_Z) + z] = value;
    }
}