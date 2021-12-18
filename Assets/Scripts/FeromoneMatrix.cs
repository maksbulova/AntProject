using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeromoneMatrix : MonoBehaviour
{
    public Transform groundContainer;
    public float cellSize;
    public int matrixSize;
    public float evaporateAmount;

    private float[,,] feromoneMatrix;
    private GameObject[,] planeMatrix;

    private void Awake()
    {
        GenerateField();
        ColorFeromones();
    }

    private void Update()
    {
        ColorFeromones();
        Evaporate();
    }

    private void GenerateField()
    {
        feromoneMatrix = new float[matrixSize, matrixSize, 2];
        planeMatrix = new GameObject[matrixSize, matrixSize];

        float offset = cellSize * matrixSize / 2;

        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < matrixSize; j++)
            {
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.SetParent(groundContainer);
                float x = (i + 0.5f) * cellSize;
                float y = (j + 0.5f) * cellSize;
                plane.transform.position = new Vector3(x - offset, y - offset);
                plane.transform.Rotate(-90, 0, 0);
                plane.transform.localScale *= 0.1f * cellSize;
                planeMatrix[i, j] = plane;
                plane.name = $"({i}; {j})";
            }
        }
    }

    public Vector3 GetCellPosition(Vector2Int cellIndex)
    {
        float offset = matrixSize * cellSize / 2;
        float x = (cellIndex.x + 0.5f) * cellSize - offset;
        float y = (cellIndex.y + 0.5f) * cellSize - offset;
        return new Vector3(x, y);
    }

    private void ColorFeromones()
    {
        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < matrixSize; j++)
            {
                float feromon = feromoneMatrix[i, j, 1]; //!!!!!!!!!!!!!!!!!!!!!
                Color color = Color.Lerp(Color.white, Color.red, feromon);
                planeMatrix[i, j].GetComponent<Renderer>().material.color = color;
            }
        }
    }

    public Vector2Int GetCellIndex(Vector3 position)
    {
        float offset = matrixSize * cellSize / 2;
        int i = Mathf.FloorToInt((position.x + offset) / cellSize);
        int j = Mathf.FloorToInt((position.y + offset) / cellSize);
        return new Vector2Int(i, j);
    }

    public float GetSmell(Vector2Int cellIndex, int feromoneType)
    {
        return feromoneMatrix[cellIndex.x, cellIndex.y, feromoneType];
    }


    public void SetSmell(Vector3 position, int feromoneType, float value)
    {
        Vector2Int cellIndex = GetCellIndex(position);

        SetSmell(cellIndex, feromoneType, value);
    }
    public void SetSmell(Vector2Int cellIndex, int feromoneType, float value)
    {
        float clamped = Mathf.Clamp(feromoneMatrix[cellIndex.x, cellIndex.y, feromoneType] += value, 0, 1);
        feromoneMatrix[cellIndex.x, cellIndex.y, feromoneType] = clamped;
    }


    private void Evaporate()
    {
        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < matrixSize; j++)
            {
                Vector2Int cell = new Vector2Int(i, j);
                SetSmell(cell, 0, -evaporateAmount);
                SetSmell(cell, 1, -evaporateAmount);
            }
        }
    }
}

