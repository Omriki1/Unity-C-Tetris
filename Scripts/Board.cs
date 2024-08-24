using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece currntPiece { get; private set; }
    public TetrominoData[] tertominos;
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }


    private void Awake()
    {

        this.tilemap = GetComponentInChildren<Tilemap>();
        this.currntPiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tertominos.Length; i++)
        {
            this.tertominos[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {

        int random = Random.Range(0, this.tertominos.Length);
        TetrominoData data = this.tertominos[random];
        this.currntPiece.Initalize(this, this.spawnPosition, data);

        if (IsValidPosition(this.currntPiece,this.spawnPosition))
        {
            Set(this.currntPiece);
        }
        else
        {
            GameOver();
        }
    }
    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
        //to do
    }
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePostion = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePostion, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePostion = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePostion, null);
        }
    }
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }
    public void ClearLines()
    {
        RectInt bounds =this.Bounds;
        int row = bounds.yMin;
        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            row++;
        }
    }
    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        { 
            Vector3Int position = new Vector3Int(col, row,0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
