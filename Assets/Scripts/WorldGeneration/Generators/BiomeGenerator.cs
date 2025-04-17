using System;
using System.Collections.Generic;
using UnityEngine;

// On GameWorld
public class BiomeGenerator : MonoBehaviour
{
    public FastNoiseLite.NoiseType biomeNoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float biomeFrequency = 0.05f;
    public int seed = 1000;

    public IReadOnlyDictionary<String, float> Thresholds = new Dictionary<String, float>(){
        ["plains"] = 0.3f,
        ["forest"] = 0.5f,
        ["mountains"] = 0.8f,
        
    };

    private FastNoiseLite _biomeNoise;
    private FastNoiseLite _detailNoise;

    public void Init()
    {
        _biomeNoise = new FastNoiseLite(seed);
        _biomeNoise.SetNoiseType(biomeNoiseType);
        _biomeNoise.SetFrequency(biomeFrequency);

        _detailNoise = new FastNoiseLite(seed + 1);
        _detailNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _detailNoise.SetFrequency(0.1f);
    }

    public BiomeType GetBiome(float worldX, float worldZ)
    {
        float biomeValue = _biomeNoise.GetNoise(worldX, worldZ);
        biomeValue = (biomeValue + 1) / 2f;

        if (biomeValue < Thresholds["plains"]) return BiomeType.Plains;
        if (biomeValue < Thresholds["forest"]) return BiomeType.Hills; //////////////////////////////
        if (biomeValue < Thresholds["mountains"]) return BiomeType.Mountains;
        return BiomeType.Hills; //////////////////////////////////////////////////////
    }

    public BlockType GetSurfaceBlock(float worldX, float worldZ)
    {
        BiomeType biome = GetBiome(worldX, worldZ);

        switch (biome)
        {
            case BiomeType.Plains:
            case BiomeType.Forest:
                return BlockType.Grass;
            case BiomeType.Mountains:
                return BlockType.Stone;
            case BiomeType.Desert:
                return BlockType.Sand;
            default:
                return BlockType.Grass;
        }
    }
}

public enum BiomeType
{
    Plains,
    Forest,
    Hills,
    Mountains,
    Desert,
}