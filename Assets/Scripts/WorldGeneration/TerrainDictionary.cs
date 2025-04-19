using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDictionary : MonoBehaviour
{
    [Serializable]
    public class TerrainGenerators
    {
        public TerrainGenerator forest;
        public TerrainGenerator flatlands;
        public TerrainGenerator hill;
        public TerrainGenerator mountain;
        public TerrainGenerator desert;
    }

    [SerializeField] private TerrainGenerators _terrainGenerators;

    public Dictionary<BiomeType, TerrainGenerator> GetDictionary()
    {
        Dictionary<BiomeType, TerrainGenerator> biomes = new()
        {
            [BiomeType.Forest] = _terrainGenerators.forest,
            [BiomeType.Flatlands] = _terrainGenerators.flatlands,
            [BiomeType.Hills] = _terrainGenerators.hill,
            [BiomeType.Mountains] = _terrainGenerators.mountain,
            [BiomeType.Desert] = _terrainGenerators.desert
        };

        return biomes;
    }

}
