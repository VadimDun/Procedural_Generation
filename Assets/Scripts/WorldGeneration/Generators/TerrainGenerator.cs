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
    private int _seed;



    public void Init(int seed)
    {
        _seed = seed;

        _octaveNoises = new FastNoiseLite[Octaves.Length];
        for (int i = 0; i < Octaves.Length; ++i)
        {
            _octaveNoises[i] = new(_seed);
            _octaveNoises[i].SetNoiseType(Octaves[i].NoiseType);
            _octaveNoises[i].SetFrequency(Octaves[i].Frequency);
        }

        _warpNoise = new(_seed);
        _warpNoise.SetNoiseType(DomainWarp.NoiseType);
        _warpNoise.SetFrequency(DomainWarp.Frequency);
        _warpNoise.SetDomainWarpAmp(DomainWarp.Amplitude);
    }

    public BlockType[,,] GenerateTerrain(float xOffset, float zOffset, BlockType SurfaceBlockType){
        
        var result = new BlockType[ChunkRenderer.CHUNK_WIDTH, ChunkRenderer.CHUNK_HEIGHT, ChunkRenderer.CHUNK_WIDTH];

        for (int x = 0; x < ChunkRenderer.CHUNK_WIDTH; ++x){
            float xWorld = xOffset + x;
            
            for (int z = 0; z < ChunkRenderer.CHUNK_WIDTH; ++z) {
                float zWorld = zOffset + z;
                float height = GetHeight(xWorld, zWorld);

                float surfaceHeight = 2;
                for (int y = 0; y < height; ++y){
                    if (height - y < surfaceHeight)
                    {
                        result[x, y, z] = SurfaceBlockType;
                    }
                    else{
                        result[x, y, z] = BlockType.Stone;
                    }
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