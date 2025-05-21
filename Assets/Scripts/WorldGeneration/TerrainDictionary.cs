using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using UnityEngine;

// On subObject of GameWorld
public class TerrainDictionary : MonoBehaviour
{
    public class GeneratorsByBiom
    {
        public TerrainGenerator terrainGenerator;
        public TreeGenerator treeGenerator;
    }

    [SerializeField] private GameWorld _gameWorld;
    [SerializeField] private TerrainGenerators _terrainGenerators;
    [SerializeField] private TreeGenerators _treeGenerators;

    public Dictionary<BiomeType, GeneratorsByBiom> Biomes { get; }

    public TerrainGenerator GetTerrainGeneratorByBiom(Biome biome)
    {
        TerrainGenerator terrainGenerator = biome.Height switch
        {
            BiomeType.Flatlands => _terrainGenerators.flatlands,
            BiomeType.FlatlandsHills => _terrainGenerators.flatlands,
            BiomeType.Hills => _terrainGenerators.hill,
            BiomeType.HillsMountains => _terrainGenerators.hill,
            _ => _terrainGenerators.mountain,
        };

        return terrainGenerator;
    }

    public TreeGenerator GetTreeGeneratorByBiom(Biome biome)
    {
        TreeGenerator treeGenerator;
        switch (biome.Temperature)
        {
            case Temperature.Desert:
                treeGenerator = _treeGenerators.desert;
                break;
            case Temperature.Temperate:
                treeGenerator = (biome.Height == BiomeType.Flatlands) ? _treeGenerators.flatlands : _treeGenerators.hill;
                break;
            case Temperature.Forest:
                treeGenerator = _treeGenerators.forest;
                break;
            default:
                treeGenerator = _treeGenerators.mountain;
                break;
        }

        return treeGenerator;
    }

    [Serializable]
    private class TerrainGenerators
    {
        public TerrainGenerator flatlands;
        public TerrainGenerator hill;
        public TerrainGenerator mountain;
    }
    [Serializable]
    private class TreeGenerators
    {
        public TreeGenerator forest;
        public TreeGenerator flatlands;
        public TreeGenerator hill;
        public TreeGenerator mountain;
        public TreeGenerator desert;
    }
}
