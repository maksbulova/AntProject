using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSource : MonoBehaviour
{
    [SerializeField] private float foodAmount;

    public void TakeFood()
    {
        foodAmount -= 1;
        if (foodAmount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
