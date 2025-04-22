using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tree generator")]
public class TreeGenerator : ScriptableObject
{
    [Header("Tree Settings")]
    public int minHeight = 4;
    public int maxHeight = 7;
    public int minLeavesRadius = 2;
    public int maxLeavesRadius = 4;
    [SerializeField] [Range(0, 1)] private float _chanceOfAppearance = 0.1f;
    private readonly BlockType trunkType = BlockType.Tree;
    private readonly BlockType leavesType = BlockType.Leaf;
    private readonly BlockType cactusType = BlockType.Cactus;

    private int _seed;
    
    [Header("Biome Settings")]
    public int treeCheckStep = 3;

    private FastNoiseLite _placementNoise;
    private System.Random _random;

    public void Init()
    {
        _placementNoise = new FastNoiseLite(_seed);
        _placementNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _placementNoise.SetFrequency(0.1f);
        _random = new System.Random(_seed);
    }

    public void SetSeed(int seed){ _seed = seed; }

    public void GenerateTrees(BlockType[,,] terrain, float xOffset, float zOffset)
    {
        for (int x = 1; x < ChunkRenderer.CHUNK_WIDTH - 1; x += treeCheckStep)
        {
            float worldX = xOffset + x;
            for (int z = 1; z < ChunkRenderer.CHUNK_WIDTH - 1; z += treeCheckStep)
            {
                float worldZ = zOffset + z;

                if (ShouldPlaceTree(worldX, worldZ))
                {
                    TryPlaceTree(terrain, x, z);
                }
            }
        }
    }

    private bool ShouldPlaceTree(float worldX, float worldZ)
    {
        float noiseValue = _placementNoise.GetNoise(worldX, worldZ);

        float placementChance = 0.5f + noiseValue * 0.5f;
        return placementChance < _chanceOfAppearance;
    }

    private bool TryPlaceTree(BlockType[,,] terrain, int x, int z)
    {
        int surfaceY = FindSurface(terrain, x, z);
        if (surfaceY < 0) return false;

        int treeHeight = minHeight + _random.Next(maxHeight - minHeight);
        int leavesRadius = minLeavesRadius + _random.Next(maxLeavesRadius - minLeavesRadius);
        if (!HasEnoughSpace(terrain, x, surfaceY, z, treeHeight, leavesRadius))
            return false;

        GenerateTree(terrain, x, surfaceY, z, treeHeight, leavesRadius);
        return true;
    }

    private int FindSurface(BlockType[,,] terrain, int x, int z)
    {
        for (int y = ChunkRenderer.CHUNK_HEIGHT - maxHeight - 1; y >= 0; --y)
        {
            if (terrain[x, y, z] != BlockType.Air)
            {
                if (IsSuitableGround(terrain[x, y, z]) && terrain[x, y + 1, z] == BlockType.Air)
                {
                    return y + 1;
                }
                break;
            }
        }
        return -1;
    }

    private bool IsSuitableGround(BlockType groundBlock)
    {
        return groundBlock == BlockType.Grass || groundBlock == BlockType.Sand;
    }

    private bool HasEnoughSpace(BlockType[,,] terrain, int x, int y, int z, int treeHeight, int leavesRadius)
    {
        for (int dy = treeHeight - 2; dy <= treeHeight + 1; ++dy)
        {
            int checkY = y + dy;
            
            for (int dx = -leavesRadius; dx <= leavesRadius; ++dx)
            {
                int checkX = x + dx;
                
                for (int dz = -leavesRadius; dz <= leavesRadius; ++dz)
                {
                    if (dx == 0 && dz == 0) continue; // Пропускаем ствол

                    int checkZ = z + dz;

                    if (checkX >= 0 && checkX < ChunkRenderer.CHUNK_WIDTH &&
                        checkY >= 0 && checkY < ChunkRenderer.CHUNK_HEIGHT &&
                        checkZ >= 0 && checkZ < ChunkRenderer.CHUNK_WIDTH)
                    {
                        if (terrain[checkX, checkY, checkZ] != BlockType.Air)
                            return false;
                    }
                }
            }
        }

        return true;
    }

    private void GenerateTree(BlockType[,,] terrain, int x, int y, int z, int treeHeight, int leavesRadius)
    {
        BlockType trunk;
        if (terrain[x, y - 1, z] == BlockType.Sand)
        {
            trunk = cactusType;
        }
        else 
        {
            trunk = trunkType;
            GenerateLeavesLayer(terrain, x, y + treeHeight, z, leavesRadius - 1);
            GenerateLeavesLayer(terrain, x, y + treeHeight - 1, z, leavesRadius);
            GenerateLeavesLayer(terrain, x, y + treeHeight - 2, z, leavesRadius);

            if (treeHeight > 5)
                GenerateLeavesLayer(terrain, x, y + treeHeight - 3, z, leavesRadius - 1);
        }

        for (int dy = 0; dy < treeHeight; ++dy)
            terrain[x, y + dy, z] = trunk;
    }

    private void GenerateLeavesLayer(BlockType[,,] terrain, int centerX, int centerY, int centerZ, int radius)
    {
        for (int dx = -radius; dx <= radius; ++dx)
        {
            for (int dz = -radius; dz <= radius; ++dz)
            {
                // Пропуск углов
                if (Mathf.Abs(dx) == radius && Mathf.Abs(dz) == radius && _random.NextDouble() > 0.3f)
                    continue;

                int x = centerX + dx;
                int z = centerZ + dz;

                if (x >= 0 && x < ChunkRenderer.CHUNK_WIDTH &&
                    centerY >= 0 && centerY < ChunkRenderer.CHUNK_HEIGHT &&
                    z >= 0 && z < ChunkRenderer.CHUNK_WIDTH &&
                    terrain[x, centerY, z] == BlockType.Air)
                {
                    terrain[x, centerY, z] = leavesType;
                }
            }
        }
    }

}