using System;
using System.Collections.Generic;
using UnityEngine;

// On subObject of GameWorld
public class BiomeGenerator : MonoBehaviour
{
    public FastNoiseLite.NoiseType biomeNoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float biomeFrequency = 0.05f;
    public int seed = 1000;
    public float Multiplier = 1f;

    public IReadOnlyDictionary<String, float> Thresholds = new Dictionary<String, float>(){
        ["desert"] = -0.7f,
        ["flatlands"] = 0f,
        ["forest"] = 0.2f,
        ["hills"] = 0.7f,
        ["mountains"] = 1f,
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
        float biomeValue = _biomeNoise.GetNoise(Multiplier * worldX, Multiplier * worldZ);

        if (biomeValue < Thresholds["desert"]) return BiomeType.Desert;
        if (biomeValue < Thresholds["flatlands"]) return BiomeType.Flatlands;
        if (biomeValue < Thresholds["forest"]) return BiomeType.Forest;
        if (biomeValue < Thresholds["hills"]) return BiomeType.Hills;
        return BiomeType.Mountains;
    }

    public BlockType GetSurfaceBlock(float worldX, float worldZ)
    {
        BiomeType biome = GetBiome(worldX, worldZ);

        return biome switch
        {
            BiomeType.Mountains => BlockType.Stone,
            BiomeType.Desert => BlockType.Sand,
            _ => BlockType.Grass,
        };
    }
}

public enum BiomeType
{
    Flatlands,
    Forest,
    Hills,
    Mountains,
    Desert,
}