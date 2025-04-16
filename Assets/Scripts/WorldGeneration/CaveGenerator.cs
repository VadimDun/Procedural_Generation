using UnityEngine;

[CreateAssetMenu(menuName = "Cave Generator")]
public class CaveGenerator : ScriptableObject
{
    [Header("Main Cave Settings")]
    public FastNoiseLite.NoiseType caveNoiseType = FastNoiseLite.NoiseType.Perlin;
    public float caveFrequency = 0.05f;
    public float caveMultiplierXZ = 5f;
    public float caveMultiplierY = 10f;

    [Header("Cave Mask Settings")]
    public FastNoiseLite.NoiseType maskNoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float maskFrequency = 0.1f;
    public float maskMultiplier = 0.3f;

    [Header("Thresholds")]
    public float maskOffset = 0.3f;
    public float caveThreshold = 0.2f;

    private FastNoiseLite _caveNoise;
    private FastNoiseLite _maskNoise;

    public void Init()
    {
        _caveNoise = new FastNoiseLite();
        _caveNoise.SetNoiseType(caveNoiseType);
        _caveNoise.SetFrequency(caveFrequency);

        _maskNoise = new FastNoiseLite();
        _maskNoise.SetNoiseType(maskNoiseType);
        _maskNoise.SetFrequency(maskFrequency);
    }

    public void ApplyCaves(BlockType[,,] terrain, float xOffset, float zOffset)
    {
        for (int x = 0; x < terrain.GetLength(0); x++)
        {
            for (int y = 0; y < terrain.GetLength(1); y++)
            {
                for (int z = 0; z < terrain.GetLength(2); z++)
                {
                    if (terrain[x, y, z] == BlockType.Air)
                        continue;

                    float worldX = x + xOffset;
                    float worldZ = z + zOffset;

                    float caveValue = _caveNoise.GetNoise(
                        worldX * caveMultiplierXZ,
                        y * caveMultiplierY,
                        worldZ * caveMultiplierXZ);

                    float caveMask = _maskNoise.GetNoise(
                        worldX * maskMultiplier,
                        worldZ * maskMultiplier) + maskOffset;

                    if (caveValue > Mathf.Max(caveMask, caveThreshold))
                    {
                        terrain[x, y, z] = BlockType.Air;
                    }
                }
            }
        }
    }
}