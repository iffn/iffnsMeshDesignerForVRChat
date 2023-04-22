
using iffnsStuff.iffnsVRCStuff.MeshDesigner;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class VertexGridOrganizer : UdonSharpBehaviour
{
    /*
        Array design:
            [x][y][z][number of vertices, Index of vertices]
    */

    //Properties:
    MeshController linkedMeshController;

    int[][][][] GridArray;

    int xGridSize = 5;
    int yGridSize = 5;
    int zGridSize = 5;

    float xCellSize;
    float yCellSize;
    float zCellSize;

    float xCellSizeInverted;
    float yCellSizeInverted;
    float zCellSizeInverted;

    int defaultVerticesPerCell = 10;

    Vector3[] VertexPositions
    {
        get
        {
            return linkedMeshController.Vertices;
        }
    }

    //Maintenance functions:
    public void Setup(MeshController linkedMeshController)
    {
        this.linkedMeshController = linkedMeshController;

        GenerateGridArray();

        SetInvertedCellSizes();
    }

    #region Public functions:
    //Modifying functions:
    public void AddVertex(int index, Vector3 position)
    {
        Vector3Int cell = GetCellFromLocalPosition(position);

        AddVertexToGrid(cell.x, cell.y, cell.z, index);
    }

    public void RemoveVertex(int index, Vector3 position)
    {
        Vector3Int cell = GetCellFromLocalPosition(position);

        RemoveVertexFromGrid(cell.x, cell.y, cell.z, index);
    }

    public void MoveVertexToNewPositoin(int index, Vector3 oldPosition, Vector3 newPosition)
    {
        Vector3Int oldCell = GetCellFromLocalPosition(oldPosition);
        Vector3Int newCell = GetCellFromLocalPosition(newPosition);

        if (oldCell.Equals(newCell)) return; //ToDo: Check if equals works

        RemoveVertexFromGrid(oldCell.x, oldCell.y, oldCell.z, index);
        AddVertexToGrid(newCell.x, newCell.y, newCell.z, index);
    }

    //Non-modifying functions:
    public int GetClosestVertexFromPosition(Vector3 position)
    {
        //Main cell
        Vector3Int cell = GetCellFromLocalPosition(position);

        int[] vertices = GetVerticesInCell(cell.x, cell.y, cell.z);

        Vector3[] vertexPositions = VertexPositions;

        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;

        for(int i = 0; i<vertices.Length; i++)
        {
            float distance = (vertexPositions[vertices[i]] - position).magnitude;

            if (closestDistance < distance) continue;

            closestDistance = distance;
            closestIndex = i;
        }

        //ToDo: Also check neighbor cells if position to cell wall < closestDistance

        return closestIndex;
    }

    public void GetClosestVerticesFromPosition(Vector3 position, int[] returnArrayWithCorrectSize)
    {
        //ToDo Fill return array

        float[] distances = new float[returnArrayWithCorrectSize.Length];

        for(int i = 0; i<distances.Length; i++)
        {
            distances[i] = Mathf.Infinity;
        }

        Vector3Int cell = GetCellFromLocalPosition(position);
    }

    #endregion

    #region Private functions:

    //Modifying functions:
    void GenerateGridArray()
    {
        GridArray = new int[xGridSize][][][];

        for (int x = 0; x < xGridSize; x++)
        {
            GridArray[x] = new int[yGridSize][][];

            for (int y = 0; y < xGridSize; y++)
            {
                GridArray[x][y] = new int[zGridSize][];

                for (int z = 0; z < xGridSize; z++)
                {
                    GridArray[x][y][z] = new int[defaultVerticesPerCell];
                }
            }
        }
    }

    void AddVertexToGrid(int x, int y, int z, int value)
    {
        int[] array = GridArray[x][y][z];

        if (CheckIfArrayFull(array))
        {
            //Handle array full
        }

        array[array[0]] = value;
        array[0]++;
    }

    void RemoveVertexFromGrid(int x, int y, int z, int value)
    {
        //Remove from grid
        int[] array = GridArray[x][y][z];

        bool found = false;

        for(int i = 1; i < array[0]; i++) //Ignore last in case array full
        {
            if (!found)
            {
                found = array[i] == value;

                continue;
            }

            array[i] = array[i + 1];
        }

        if (found)
        {
            array[array[0]] = 0;

            array[0]--;
        }

        //Decrement all vertices that are larger
        foreach (int[][][] arrayX in GridArray)
        {
            foreach (int[][] arrayY in arrayX)
            {
                foreach (int[] arrayZ in arrayY)
                {
                    for (int i = 1; i < arrayZ[0] + 1; i++)
                    {
                        if (arrayZ[i] > value) arrayZ[i]--;
                    }
                }
            }
        }
    }

    //Non-modifying functions:
    Vector3 GetCellCenter(Vector3Int cell)
    {
        return new Vector3(
            GetPositionFromGridIndex(cell.x, xCellSize),
            GetPositionFromGridIndex(cell.y, yCellSize),
            GetPositionFromGridIndex(cell.z, zCellSize)
            );
    }

    Vector3Int GetCellFromLocalPosition(Vector3 position)
    {
        return new Vector3Int(
            GetGridIndexFromPosition(position.x, xCellSizeInverted),
            GetGridIndexFromPosition(position.y, yCellSizeInverted),
            GetGridIndexFromPosition(position.z, zCellSizeInverted)
            );
    }

    float GetPositionFromGridIndex(int gridIndex, float cellSize)
    {
        if(gridIndex == 0) return 0;
        if (gridIndex % 2 == 1) return (gridIndex + 1) / 2;
        else return -(gridIndex / 2);
    }

    int GetGridIndexFromPosition(float position, float cellSizeInverted)
    {
        int roundedValue = Mathf.RoundToInt(position * cellSizeInverted);
        
        if (roundedValue == 0) return 0;
        if (roundedValue > 0) return roundedValue * 2 - 1;
        else return -(roundedValue * 2);
    }

    int[] GetVerticesInCell(int x, int y, int z)
    {
        int[] array = GridArray[x][y][z];

        int[] returnValue = new int[array[0]];

        for (int i = 0; i < array[0]; i++)
        {
            returnValue[i] = array[i + 1];
        }

        return returnValue;
    }

    void SetInvertedCellSizes()
    {
        xCellSizeInverted = 1 / xCellSize;
        yCellSizeInverted = 1 / yCellSize;
        zCellSizeInverted = 1 / zCellSize;
    }

    bool CheckIfPositionInGrid(Vector3 position)
    {
        if(GridArray.Length * xCellSize * 0.5 < Mathf.Abs(position.x)) return false;
        if (GridArray[0].Length * yCellSize * 0.5 < Mathf.Abs(position.y)) return false;
        if (GridArray[0][0].Length * zCellSize * 0.5 < Mathf.Abs(position.z)) return false;

        return true;
    }

    bool CheckIfArrayFull(int[] array)
    {
        return array.Length > array[0];
    }

    #endregion
}
