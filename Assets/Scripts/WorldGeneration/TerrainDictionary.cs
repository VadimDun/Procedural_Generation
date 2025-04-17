using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Player;
using UnityEngine;

public class TerrainDictionary : MonoBehaviour
{
    [Serializable]
    public class TerrainGenerators
    {
        public TerrainGenerator plain;
        public TerrainGenerator mountain;
        public TerrainGenerator hill;
    }

    [SerializeField] private TerrainGenerators _terrainGenerators;

    public Dictionary<BiomeType, TerrainGenerator> GetDictionary()
    {
        Dictionary<BiomeType, TerrainGenerator> biomes = new();
        biomes[BiomeType.Plains] = _terrainGenerators.plain;
        biomes[BiomeType.Mountains] = _terrainGenerators.mountain;
        biomes[BiomeType.Hills] = _terrainGenerators.hill;

        return biomes;
    }

}
