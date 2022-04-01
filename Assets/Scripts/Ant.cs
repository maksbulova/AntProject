using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ant : MonoBehaviour
{
    [Header("Parametrs")]
    [SerializeField, Range(0, 4)] private int turnAngle;
    [SerializeField] private float feromoneAmount;

    [Header("Agent coefficient")]
    [SerializeField] private float sideChanse;
    [SerializeField] private float forwardChanse;
    [SerializeField] private AnimationCurve turnCurve;
    [SerializeField] private AnimationCurve homeCurve;

    private FeromoneMatrix feromoneMatrix;
    private Vector3 homePosition;

    private enum BehaviorState{searching, carrying};
    private BehaviorState behaviorState;
    private Vector2Int currentDirection;

    private void Start()
    {
        feromoneMatrix = GameObject.FindObjectOfType<FeromoneMatrix>();
        behaviorState = BehaviorState.searching;
        currentDirection = RandomDirection();
        // Debug.Log($"{gameObject.name} moves {currentDirection}");
        homePosition = FindObjectOfType<AntHill>().transform.position;
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

        Vector3 movePosition = feromoneMatrix.GetCellPosition(moveCell);
        gameObject.transform.position = movePosition;   
    }

    public Vector2Int ChooseCell(int feromonType)
    {
        List<Vector2Int> possibleCells;
        List<float> possibleFeromones;
        Vector2Int currentCell;
        Vector2Int cellPosition;

        do
        {
            possibleCells = new List<Vector2Int>();
            possibleFeromones = new List<float>();
            currentCell = feromoneMatrix.GetCellIndex(transform.position);

            for (int i = -turnAngle; i <= turnAngle; i++)
            {
                float rotateAngle = i * Mathf.PI / 4;
                int rotatedX = Mathf.RoundToInt(currentDirection.x * Mathf.Cos(rotateAngle) + currentDirection.y * Mathf.Sin(rotateAngle));
                int rotadedY = Mathf.RoundToInt(-currentDirection.x * Mathf.Sin(rotateAngle) + currentDirection.y * Mathf.Cos(rotateAngle));
                Vector2Int newDirection = new Vector2Int(rotatedX, rotadedY);

                cellPosition = currentCell + newDirection;
                if (feromoneMatrix.CheckMatrixContain(cellPosition))
                {
                    possibleCells.Add(cellPosition);
                    possibleFeromones.Add(feromoneMatrix.GetSmell(cellPosition, feromonType));
                }
            }

            // Met world`s bounds, turn back.
            if (possibleCells.Count == 0)
            {
                currentDirection = -currentDirection;
            }

        } while (possibleCells.Count == 0);


        float sumFeromone = 0;
        float[] p = new float[possibleFeromones.Count];
        Vector2Int forwardCell = currentCell + currentDirection;

        
        if (possibleCells.Contains(forwardCell))
        {
            int forwardndex = possibleCells.IndexOf(forwardCell);
            p[forwardndex] += forwardChanse;

        }



        for (int i = 0; i < possibleFeromones.Count; i++)
        {
            // Smaller turn angle is more attractive.
            /*
            Vector2Int dir = possibleCells[i] - currentCell;
            float angleDif = Vector2.Angle(currentDirection, dir);
            float t = Mathf.InverseLerp(0, angle * Mathf.PI, angleDif);
            float turnKoof = turnCurve.Evaluate(t);
            */
            p[i] += (possibleFeromones[i] + sideChanse);

        }

        if (behaviorState == BehaviorState.carrying)
        {
            float[] homeOrientir = new float[possibleCells.Count];
            for (int i = 0; i < homeOrientir.Length; i++)
            {
                homeOrientir[i] = (feromoneMatrix.GetCellPosition(possibleCells[i]) - homePosition).sqrMagnitude;
            }
            float min = homeOrientir.Min();
            float max = homeOrientir.Max();

            for (int i = 0; i < homeOrientir.Length; i++)
            {
                homeOrientir[i] = (homeOrientir[i] - min) / (max - min);


                p[i] *= homeCurve.Evaluate(homeOrientir[i]);
            }

        }

        for (int i = 0; i < p.Length; i++)
        {
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
            food.TakeFood();

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
