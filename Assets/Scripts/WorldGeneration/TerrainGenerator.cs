using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain generator")]
public class TerrainGenerator : ScriptableObject
{
    [Range(0f, 1f)] public float BaseHeightLevel = 0.5f;

    [Serializable]
    public class NoiseOctaveSettings
    {
        public FastNoiseLite.NoiseType NoiseType;
        public float Frequency = 0.2f;
        public float Amplitude = 1;
    }
    public NoiseOctaveSettings[] Octaves;
    public NoiseOctaveSettings DomainWarp;

    
    private FastNoiseLite[] _octaveNoises;
    private FastNoiseLite _warpNoise;


    public void Init()
    {
        _octaveNoises = new FastNoiseLite[Octaves.Length];
        for (int i = 0; i < Octaves.Length; ++i){
            _octaveNoises[i] = new();
            _octaveNoises[i].SetNoiseType(Octaves[i].NoiseType);
            _octaveNoises[i].SetFrequency(Octaves[i].Frequency);
        }

        _warpNoise = new();
        _warpNoise.SetNoiseType(DomainWarp.NoiseType);
        _warpNoise.SetFrequency(DomainWarp.Frequency);
        _warpNoise.SetDomainWarpAmp(DomainWarp.Amplitude);
    }

    public BlockType[,,] GenerateTerrain(float xOffset, float zOffset){
        
        var result = new BlockType[ChunkRenderer.CHUNK_WIDTH, ChunkRenderer.CHUNK_HEIGHT, ChunkRenderer.CHUNK_WIDTH];

        for (int x = 0; x < ChunkRenderer.CHUNK_WIDTH; ++x){
            for (int z = 0; z < ChunkRenderer.CHUNK_WIDTH; ++z) {
                float xWorld = xOffset + x;
                float zWorld = zOffset + z;
                float height = GetHeight(xWorld, zWorld);

                float grassHeight = 2;
                for (int y = 0; y < height; ++y){
                    if (height - y < grassHeight){
                        result[x, y, z] = BlockType.Grass;
                    }
                    else{
                        result[x, y, z] = BlockType.Stone;
                    }

                    // FastNoise noise = new();
                    // float caveNoise1 = noise.GetPerlinFractal(xWorld * 5f, y * 10f, zWorld * 5f);
                    // float caveMask = noise.GetSimplex(xWorld * .3f, zWorld * .3f) + .3f;
                    // if (caveNoise1 > Mathf.Max(caveMask, .2f))
                    //     result[x, y, z] = BlockType.Air;
                }
            }
        }
       
        return result;
    }

    private float GetHeight(float x, float y){
        _warpNoise.DomainWarp(ref x, ref y);
        float result = ChunkRenderer.CHUNK_HEIGHT * BaseHeightLevel;

        for (int i = 0; i < Octaves.Length; i++)
        {
            float noise = _octaveNoises[i].GetNoise(x, y);
            result += noise * Octaves[i].Amplitude / 2;
        }

        return result;
    }
}