using System;
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
        public GeneratorsByBiom(TerrainGenerator terGen, TreeGenerator treeGen)
        {
            terrainGenerator = terGen;
            treeGenerator = treeGen;
        }
    }

    [SerializeField] private GameWorld _gameWorld;
    [SerializeField] private TerrainGenerators _terrainGenerators;
    [SerializeField] private TreeGenerators _treeGenerators;

    public Dictionary<BiomeType, GeneratorsByBiom> Biomes { get; }
    void Awake()
    {
        Dictionary<BiomeType, GeneratorsByBiom> biomes = new()
        {
            [BiomeType.Forest] = new(_terrainGenerators.forest, _treeGenerators.forest),
            [BiomeType.Flatlands] = new(_terrainGenerators.flatlands, _treeGenerators.flatlands),
            [BiomeType.Hills] = new(_terrainGenerators.hill, _treeGenerators.hill),
            [BiomeType.Mountains] = new(_terrainGenerators.mountain, _treeGenerators.mountain),
            [BiomeType.Desert] = new(_terrainGenerators.desert, _treeGenerators.desert)
        };

        _gameWorld.SetBiomeGenerators(biomes);
    }

    [Serializable]
    private class TerrainGenerators
    {
        public TerrainGenerator forest;
        public TerrainGenerator flatlands;
        public TerrainGenerator hill;
        public TerrainGenerator mountain;
        public TerrainGenerator desert;
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
