using Shurub;
using UnityEngine;

public class BakedFishSet : Ingredient
{
    public override string IngredientName => "BakedFishSet";
    public override IngredientManager.SetType setType => IngredientManager.SetType.BakedFishSet;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
