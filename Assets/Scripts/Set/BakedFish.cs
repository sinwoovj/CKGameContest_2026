using Shurub;
using UnityEngine;

public class BakedFish : Ingredient
{
    public override string IngredientName => "BakedFish";
    public override IngredientManager.SetType setType => IngredientManager.SetType.BakedFish;
    private void Start()
    {
        state = IngredientState.unCookable;
    }
}
