using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    [Header("Height Map Settings")]
    public FastNoiseLite.NoiseType heightNoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float heightFrequency = 0.05f;
    [Range(0, 1)] public float heightAmplitude = 1f;
    public float heightMultiplier = 1f;

    [Header("Temperature Map Settings")]
    public FastNoiseLite.NoiseType temperatureNoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float temperatureFrequency = 0.03f;
    public float temperatureMultiplier = 1f;

    private FastNoiseLite _heightNoise;
    private FastNoiseLite _temperatureNoise;

    private int _seed;
    public IReadOnlyDictionary<String, float> HeightThresholds = new Dictionary<String, float>(){
        ["flatlands"] = 0f,
        ["hills"] = 0.6f,
        ["mountains"] = 1f,
    };

    public IReadOnlyDictionary<String, float> TemperatureThresholds = new Dictionary<String, float>()
    {
        ["ice"] = -0.6f,
        ["temperate"] = 0.6f,
        ["desert"] = 1f,
    };

    public void Init(int seed)
    {
        _seed = seed;

        _heightNoise = new FastNoiseLite(_seed);
        _heightNoise.SetNoiseType(heightNoiseType);
        _heightNoise.SetFrequency(heightFrequency);

        _temperatureNoise = new FastNoiseLite(_seed + 1);
        _temperatureNoise.SetNoiseType(temperatureNoiseType);
        _temperatureNoise.SetFrequency(temperatureFrequency);
    }

    public Biome GetBiome(Vector2Int chunkCoords)
    {
        Biome biome= new();

        float heightValue = _heightNoise.GetNoise(heightMultiplier * chunkCoords.x, heightMultiplier * chunkCoords.y);
        
        if (heightValue < HeightThresholds["flatlands"]) biome.Height = BiomeType.Flatlands;
        else if (heightValue < HeightThresholds["hills"]) biome.Height = BiomeType.Hills;
        else biome.Height = BiomeType.Mountains;

        float temperatureValue = _temperatureNoise.GetNoise(
            temperatureMultiplier * chunkCoords.x, temperatureMultiplier * chunkCoords.y
            );
        
        if (temperatureValue < TemperatureThresholds["ice"]) biome.Temperature = Temperature.Ice;
        else if(temperatureValue < TemperatureThresholds["temperate"]) biome.Temperature = Temperature.Temperate;
        else biome.Temperature = Temperature.Desert;

        biome.SurfaceBlock = GetSurfaceBlock(biome);

        return biome;
    }

    private BlockType GetSurfaceBlock(Biome biome)
    {
        return biome.Temperature switch
        {
            Temperature.Desert => BlockType.Sand,
            Temperature.Ice => BlockType.Ice,
            _ => biome.Height == BiomeType.Mountains ? BlockType.Stone : BlockType.Grass,
        };
    }
}

public enum BiomeType
{
    Flatlands,
    Hills,
    Mountains,
}

public enum Temperature
{
    Ice,
    Forest,
    Temperate,
    Desert,
}