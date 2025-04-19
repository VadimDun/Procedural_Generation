using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// On GameWorld
[RequireComponent(typeof(BiomeGenerator))]
public class GameWorld : MonoBehaviour
{
    [SerializeField] private Vector2Int _currentPlayerChunk;

    [Header("Radiuses")]
    [SerializeField] private int VIEW_RADIUS = 5;
    [SerializeField] private int DELETING_RADIUS = 5;

    [Header("Prefabs and Generators")]
    public ChunkRenderer ChunkPrefab;
    public Dictionary<Vector2Int, ChunkData> ChunkDatas = new();
    public Dictionary<BiomeType, TerrainGenerator> BiomeGenerators;
    public CaveGenerator caveGenerator;
    public BiomeGenerator biomeGenerator;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        caveGenerator.Init();
        biomeGenerator.Init();
        StartCoroutine(Generate(false));
    }

    private IEnumerator Generate(bool wait)
    {
        for (int x = _currentPlayerChunk.x - VIEW_RADIUS; x <= _currentPlayerChunk.x + VIEW_RADIUS; ++x)
        {
            for (int y = _currentPlayerChunk.y - VIEW_RADIUS; y <= _currentPlayerChunk.y + VIEW_RADIUS; ++y)
            {
                var chunkPosition = new Vector2Int(x, y);
                if (ChunkDatas.ContainsKey(chunkPosition)) continue;

                LoadChunkAt(chunkPosition);

                if (wait) yield return new WaitForSecondsRealtime(.1f);
            }
        }

        RemoveDistantChunks();
    }

    private void RemoveDistantChunks()
    {
        List<Vector2Int> chunksToRemove = new();

        foreach (var chunkData in ChunkDatas)
        {
            var dist = _currentPlayerChunk - chunkData.Key;

            if (Math.Abs(dist.x) > DELETING_RADIUS || Math.Abs(dist.y) > DELETING_RADIUS)
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
        BiomeType biome = biomeGenerator.GetBiome(chunkCoords);
        BlockType surfaceBlocktype = biomeGenerator.GetSurfaceBlock(biome);
        TerrainGenerator terrainGenerator = BiomeGenerators[biome];
        terrainGenerator.Init();

        float xPosWorld = chunkCoords.x * ChunkRenderer.CHUNK_WIDTH;
        float zPosWorld = chunkCoords.y * ChunkRenderer.CHUNK_WIDTH;

        BlockType[,,] blocks = terrainGenerator.GenerateTerrain(xPosWorld, zPosWorld, surfaceBlocktype);
        caveGenerator.ApplyCaves(blocks, xPosWorld, zPosWorld, terrainGenerator.BaseHeightLevel);

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
        caveGenerator.Init();

        foreach (var chunkData in ChunkDatas)
            Destroy(chunkData.Value.Renderer.gameObject);

        ChunkDatas.Clear();

        StartCoroutine(Generate(false));
    }

    private void Update()
    {
        Vector3Int playerWorldPos = Vector3Int.FloorToInt(_mainCamera.transform.position);
        Vector2Int playerChunk = GetChunkAt(playerWorldPos);
        if (playerChunk != _currentPlayerChunk)
        {
            _currentPlayerChunk = playerChunk;
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