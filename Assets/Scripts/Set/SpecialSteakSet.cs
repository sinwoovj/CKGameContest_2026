using Shurub;
using UnityEngine;

public class SpecialSteakSet : Ingredient
{
    public override string IngredientName => "SpecialSteakSet";
    public override IngredientManager.SetType setType => IngredientManager.SetType.SpecialSteakSet;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
