using Shurub;
using UnityEngine;

public class Strange : Ingredient
{
    public override string IngredientName => "Strange";
    public override IngredientManager.SetType setType => IngredientManager.SetType.Strange;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
