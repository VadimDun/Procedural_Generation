using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Cave Generator")]
public class CaveGenerator : ScriptableObject
{
    [Range(0f, 1f)] public float caveHeightLimit = 0.7f;
    
    [Header("Main Cave Settings")]
    public FastNoiseLite.NoiseType caveNoiseType = FastNoiseLite.NoiseType.Perlin;
    public float caveFrequency = 0.025f;
    public float caveMultiplierXZ = 3f;
    public float caveMultiplierY = 5f;

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
        int height = (int)(TerrainGenerator.BaseHeightLevel * caveHeightLimit);
        for (int x = 0; x < ChunkRenderer.CHUNK_WIDTH; ++x)
        {
            float worldX = x + xOffset;

            for (int z = 0; z < ChunkRenderer.CHUNK_WIDTH; ++z)
            {
                float worldZ = z + zOffset;

                float caveMask = _maskNoise.GetNoise(
                    worldX * maskMultiplier,
                    worldZ * maskMultiplier) + maskOffset;

                for (int y = 0; y < height; ++y)
                {
                    if (terrain[x, y, z] == BlockType.Air)
                        continue;

                    float caveValue = _caveNoise.GetNoise(
                        worldX * caveMultiplierXZ,
                        y * caveMultiplierY,
                        worldZ * caveMultiplierXZ);

                    //if (caveValue > caveThreshold)
                    //if (caveValue > caveMask)
                    if (caveValue > Mathf.Max(caveMask, caveThreshold))
                    {
                        terrain[x, y, z] = BlockType.Air;
                    }
                }
            }
        }
    }
}