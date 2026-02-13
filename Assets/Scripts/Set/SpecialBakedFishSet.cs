using Shurub;
using UnityEngine;

public class SpecialBakedFishSet : Ingredient
{
    public override string IngredientName => "SpecialBakedFishSet";
    public override IngredientManager.SetType setType => IngredientManager.SetType.SpecialBakedFishSet;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
