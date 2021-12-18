using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    private FeromoneMatrix feromoneMatrix;
    [Range(0, 4)] public int angle;
    public float feromoneAmount;

    private const float baseChanse = 0.1f;

    private enum BehaviorState{searching, carrying};
    private BehaviorState behaviorState;
    private Vector2Int currentDirection;


    private void Start()
    {
        feromoneMatrix = GameObject.FindObjectOfType<FeromoneMatrix>();
        behaviorState = BehaviorState.searching;
        currentDirection = RandomDirection();
        Debug.Log($"{gameObject.name} moves {currentDirection}");
    }

    private void Update()
    {
        Act();
    }

    private Vector2Int RandomDirection()
    {
        int x,y;
        do
        {
            x = Random.Range(-1, 2);
            y = Random.Range(-1, 2);

        } while (x == 0 && y == 0);

        return new Vector2Int(x, y);
    }

    public void Move(Vector2Int moveCell)
    {
        Vector2Int currentCell = feromoneMatrix.GetCellIndex(transform.position);
        currentDirection = moveCell - currentCell;

        // Vector3 rayDirection = new Vector3(currentDirection.x, currentDirection.y);
        // Debug.DrawRay(transform.position, transform.position + rayDirection * 10, Color.blue);

        Vector3 movePosition = feromoneMatrix.GetCellPosition(moveCell);
        gameObject.transform.position = movePosition;   
    }

    public Vector2Int ChooseCell(int feromonType)
    {
        // int amount = (angle + 1) * 2 - 1;
        List<Vector2Int> possibleCells = new List<Vector2Int>();
        List<float> possibleFeromones = new List<float>();
        Vector2Int currentCellIndex = feromoneMatrix.GetCellIndex(transform.position);
        Vector2Int cellPosition;

        do
        {
            for (int i = -angle; i <= angle; i++)
            {
                float rotateAngle = i * Mathf.PI / 4;
                int rotatedX = Mathf.RoundToInt(currentDirection.x * Mathf.Cos(rotateAngle) + currentDirection.y * Mathf.Sin(rotateAngle));
                int rotadedY = Mathf.RoundToInt(-currentDirection.x * Mathf.Sin(rotateAngle) + currentDirection.y * Mathf.Cos(rotateAngle));
                Vector2Int newDirection = new Vector2Int(rotatedX, rotadedY);

                cellPosition = currentCellIndex + newDirection;
                if (feromoneMatrix.CheckMatrixContain(cellPosition))
                {
                    possibleCells.Add(cellPosition);
                    possibleFeromones.Add(feromoneMatrix.GetSmell(cellPosition, feromonType));
                }
                // Debug.DrawLine(transform.position, transform.position + new Vector3(newDirection.x, newDirection.y) * 10, Color.yellow, 10);
            }

            // Debug.DrawLine(transform.position, transform.position + new Vector3(currentDirection.x, currentDirection.y) * 10, Color.red, 10);
            if (possibleCells.Count == 0)
                currentDirection = -currentDirection;

        } while (possibleCells.Count == 0);


        float sumFeromone = 0;
        float[] p = new float[possibleFeromones.Count];

        for (int i = 0; i < possibleFeromones.Count; i++)
        {
            p[i] =  possibleFeromones[i] + baseChanse;
            sumFeromone += p[i];
        }

        for (int i = 0; i < p.Length; i++)
        {
            p[i] /= sumFeromone;
        }

        float rnd = Random.Range(0f, 1f);
        int chosenIndex = 0;
        float pSum = 0;

        for (int i = 0; i < p.Length; i++)
        {
            if (rnd < p[i] + pSum)
            {
                chosenIndex = i;
                break;
            }
            pSum += p[i];
        }

        return possibleCells[chosenIndex];
    }

    private void Act()
    {
        Vector2Int cell = ChooseCell((int)behaviorState);
        Move(cell);
        feromoneMatrix.SetSmell(transform.position, (1 - (int)behaviorState), feromoneAmount);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Food" && behaviorState == BehaviorState.searching)
        {
            FoodSource food = collision.gameObject.GetComponent<FoodSource>();
            food.foodAmount -= 1;

            behaviorState = BehaviorState.carrying;
            currentDirection = -currentDirection;

        }
        else if (tag == "AntHill" && behaviorState == BehaviorState.carrying)
        {
            AntHill antHill = collision.gameObject.GetComponent<AntHill>();
            antHill.foodStorage += 1;

            behaviorState = BehaviorState.searching;
            currentDirection = -currentDirection;

        }
    }
}
