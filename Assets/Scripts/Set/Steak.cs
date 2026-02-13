using Shurub;
using UnityEngine;

public class Steak : Ingredient
{
    public override string IngredientName => "Steak";
    public override IngredientManager.SetType setType => IngredientManager.SetType.Steak;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
