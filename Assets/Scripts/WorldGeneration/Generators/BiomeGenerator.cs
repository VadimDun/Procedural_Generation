using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public FastNoiseLite.NoiseType biomeNoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float biomeFrequency = 0.05f;
    public float Multiplier = 1f;
    private int _seed;

    public IReadOnlyDictionary<String, float> Thresholds = new Dictionary<String, float>(){
        ["desert"] = -0.7f,
        ["flatlands"] = 0f,
        ["forest"] = 0.3f,
        ["hills"] = 0.8f,
        ["mountains"] = 1f,
    };

    private FastNoiseLite _biomeNoise;
    private FastNoiseLite _detailNoise;

    public void Init()
    {
        _biomeNoise = new FastNoiseLite(_seed);
        _biomeNoise.SetNoiseType(biomeNoiseType);
        _biomeNoise.SetFrequency(biomeFrequency);

        _detailNoise = new FastNoiseLite(_seed + 1);
        _detailNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _detailNoise.SetFrequency(0.1f);
    }
    public void SetSeed(int seed) { _seed = seed; }

    public BiomeType GetBiome(Vector2Int chunkCoords)
    {
        float biomeValue = _biomeNoise.GetNoise(Multiplier * chunkCoords.x, Multiplier * chunkCoords.y);

        if (biomeValue < Thresholds["desert"]) return BiomeType.Desert;
        if (biomeValue < Thresholds["flatlands"]) return BiomeType.Flatlands;
        if (biomeValue < Thresholds["forest"]) return BiomeType.Forest;
        if (biomeValue < Thresholds["hills"]) return BiomeType.Hills;
        return BiomeType.Mountains;
    }

    public BlockType GetSurfaceBlock(BiomeType biome)
    {
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