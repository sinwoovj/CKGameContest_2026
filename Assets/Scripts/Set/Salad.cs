using Shurub;
using UnityEngine;

public class Salad : Ingredient
{
    public override string IngredientName => "Salad";
    public override IngredientManager.SetType setType => IngredientManager.SetType.Salad;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
