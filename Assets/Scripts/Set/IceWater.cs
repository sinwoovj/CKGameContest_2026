using Shurub;
using UnityEngine;

public class IceWater : Ingredient
{
    public override string IngredientName => "IceWater";
    public override IngredientManager.SetType setType => IngredientManager.SetType.IceWater;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
