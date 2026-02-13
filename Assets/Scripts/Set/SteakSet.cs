using Shurub;
using UnityEngine;

public class SteakSet : Ingredient
{
    public override string IngredientName => "SteakSet";
    public override IngredientManager.SetType setType => IngredientManager.SetType.SteakSet;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
