using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// On GameWorld
public class GameWorld : MonoBehaviour
{
    [SerializeField] private int VIEW_RADIUS = 5;
    [SerializeField] private int DELETING_RADIUS = 5;
    public Dictionary<Vector2Int, ChunkData> ChunkDatas = new();
    public ChunkRenderer ChunkPrefab;
    public TerrainGenerator TerGenerator;
    public CaveGenerator caveGenerator;

    private Camera _mainCamera;
    [SerializeField] private Vector2Int _currentPlayerChunk;

    void Start()
    {
        _mainCamera = Camera.main;
        TerGenerator.Init();
        caveGenerator.Init();
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
                RebuildNeighborChunks(chunkPos);
            }
        }
    }

    private void LoadChunkAt(Vector2Int chunkCoords)
    {
        float xPosWorld = chunkCoords.x * ChunkRenderer.CHUNK_WIDTH;
        float zPosWorld = chunkCoords.y * ChunkRenderer.CHUNK_WIDTH;

        var blocks = TerGenerator.GenerateTerrain(xPosWorld, zPosWorld);
        caveGenerator.ApplyCaves(blocks, xPosWorld, zPosWorld, TerGenerator.BaseHeightLevel);

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
        TerGenerator.Init();
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