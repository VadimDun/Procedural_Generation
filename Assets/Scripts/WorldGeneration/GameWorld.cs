using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// On GameWorld
[RequireComponent(typeof(BiomeGenerator))]
public class GameWorld : MonoBehaviour
{
    public Vector2Int CurrentPlayerChunk{ get; private set; }
    public Biome CurrentBiome { get; private set; }

    [Header("World settings")]
    [SerializeField] private int _seed = 1337;
    [SerializeField] private int VIEW_RADIUS = 5;

    [Header("Prefabs and Generators")]
    [SerializeField] private ChunkRenderer ChunkPrefab;
    [SerializeField] private CaveGenerator _caveGenerator;
    [SerializeField] private BiomeGenerator _biomeGenerator;

    public Dictionary<Vector2Int, ChunkData> ChunkDatas { get; private set; }
    [SerializeField] private Dictionary<BiomeType, TerrainDictionary.GeneratorsByBiom> _biomeGenerators;
    [SerializeField] private TerrainDictionary _terrainDictionary;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        ChunkDatas = new();

        _caveGenerator.Init();
        _biomeGenerator.Init(_seed);

        CurrentBiome = _biomeGenerator.GetBiome(CurrentPlayerChunk);

        StartCoroutine(Generate(false));
    }

    private IEnumerator Generate(bool wait)
    {
        for (int x = CurrentPlayerChunk.x - VIEW_RADIUS; x <= CurrentPlayerChunk.x + VIEW_RADIUS; ++x)
        {
            for (int y = CurrentPlayerChunk.y - VIEW_RADIUS; y <= CurrentPlayerChunk.y + VIEW_RADIUS; ++y)
            {
                var chunkPosition = new Vector2Int(x, y);
                if (ChunkDatas.ContainsKey(chunkPosition)) continue;

                LoadChunkAt(chunkPosition);
                RebuildNeighborChunks(chunkPosition);

                if (wait) yield return new WaitForSecondsRealtime(.1f);
            }
        }

        RemoveDistantChunks();
    }

    private void RebuildNeighborChunks(Vector2Int chunkPosition)
    {
        Vector2Int[] neighborDirections = new Vector2Int[]
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        foreach (var dir in neighborDirections)
        {
            Vector2Int neighborPos = chunkPosition + dir;
            if (ChunkDatas.TryGetValue(neighborPos, out ChunkData neighborChunk))
            {
                if (neighborChunk.Renderer != null)
                {
                    neighborChunk.Renderer.RebuildMesh();
                }
            }
        }
    }

    private void RemoveDistantChunks()
    {
        List<Vector2Int> chunksToRemove = new();

        foreach (var chunkData in ChunkDatas)
        {
            var dist = CurrentPlayerChunk - chunkData.Key;

            if (Math.Abs(dist.x) > VIEW_RADIUS || Math.Abs(dist.y) > VIEW_RADIUS)
            {
                chunksToRemove.Add(chunkData.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            if (ChunkDatas.TryGetValue(chunkPos, out ChunkData chunkData))
            {
                if (chunkData.Renderer != null)
                {
                    Destroy(chunkData.Renderer.gameObject);
                }
                ChunkDatas.Remove(chunkPos);
            }
        }
    }

    private void LoadChunkAt(Vector2Int chunkCoords)
    {
        Biome biome = _biomeGenerator.GetBiome(chunkCoords);
        BlockType surfaceBlocktype = biome.SurfaceBlock;
        TerrainGenerator terrainGenerator = _terrainDictionary.GetTerrainGeneratorByBiom(biome);
        TreeGenerator treeGenerator = _terrainDictionary.GetTreeGeneratorByBiom(biome);

        terrainGenerator.Init();
        treeGenerator.SetSeed(_seed);
        treeGenerator.Init();

        float xPosWorld = chunkCoords.x * ChunkRenderer.CHUNK_WIDTH;
        float zPosWorld = chunkCoords.y * ChunkRenderer.CHUNK_WIDTH;

        BlockType[,,] blocks = terrainGenerator.GenerateTerrain(xPosWorld, zPosWorld, surfaceBlocktype);
        _caveGenerator.ApplyCaves(blocks, xPosWorld, zPosWorld, terrainGenerator.BaseHeightLevel);
        treeGenerator.GenerateTrees(blocks, xPosWorld, zPosWorld);

        ChunkData chunkData = new()
        {
            ChunkPosition = chunkCoords,
            Blocks = blocks
        };
        ChunkDatas.Add(chunkCoords, chunkData);

        var chunk = Instantiate(ChunkPrefab, new Vector3(xPosWorld, 0, zPosWorld), Quaternion.identity, transform);
        chunk.ChunkData = chunkData;
        chunk.World = this;

        chunkData.Renderer = chunk;
    }

    [ContextMenu("Regenerate world")]
    public void Regenerate()
    {
        _caveGenerator.Init();

        foreach (var chunkData in ChunkDatas)
            Destroy(chunkData.Value.Renderer.gameObject);

        ChunkDatas.Clear();

        StartCoroutine(Generate(false));
    }

    private void Update()
    {
        Vector3Int playerWorldPos = Vector3Int.FloorToInt(_mainCamera.transform.position);
        Vector2Int playerChunk = GetChunkAt(playerWorldPos);
        if (playerChunk != CurrentPlayerChunk)
        {
            CurrentPlayerChunk = playerChunk;
            CurrentBiome = _biomeGenerator.GetBiome(CurrentPlayerChunk);
            StartCoroutine(Generate(true));
        }
    }

    public Vector2Int GetChunkAt(Vector3Int blockWorldPos)
    {
        Vector2Int chunkPosition = new(blockWorldPos.x / ChunkRenderer.CHUNK_WIDTH, blockWorldPos.z / ChunkRenderer.CHUNK_WIDTH);

        if (blockWorldPos.x < 0) chunkPosition.x--;
        if (blockWorldPos.z < 0) chunkPosition.y--;
        return chunkPosition;
    }
}