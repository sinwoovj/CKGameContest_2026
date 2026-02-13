using Shurub;
using UnityEngine;

public class AppleJuice : Ingredient
{
    public override string IngredientName => "AppleJuice";
    public override IngredientManager.SetType setType => IngredientManager.SetType.AppleJuice;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
