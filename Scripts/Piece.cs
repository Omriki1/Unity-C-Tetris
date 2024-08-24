using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public Vector3Int position { get; private set; }
    public TetrominoData data { get; private set; }

    public Vector3Int[] cells { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDealy = 1f;
    public float lockDealy = 0.5f;

    private float stepTime;
    private float lockTime;
    public void Initalize(Board board,Vector3Int position,TetrominoData data)
    { 
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDealy;
        this.lockTime = 0f;

        if (cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }
        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;


        if (Input.GetKeyDown(KeyCode.W))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
        }
        if (Input.GetKeyDown(KeyCode.S))
        { Move(Vector2Int.down);}
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
            Lock();
        }
        if (Time.time >=this.stepTime)
        {
            Step();
        }
        this.board.Set(this);
    }
    private void Step()
    { 
        this.stepTime = Time.time + this.stepDealy;
        Move(Vector2Int.down);
        if (this.lockTime >= this.lockDealy) 
        {
            Lock();
        }
    }
    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }
    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }
    }
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this,newPosition);

        if (valid)
        {

            this.position = newPosition;
            this.lockTime = 0f;

        }
        return valid;

    }
    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }   
    }
    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tertomino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }


    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }


    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        { 
        return min + (input -min) % (max - min);
        }
    }
}
