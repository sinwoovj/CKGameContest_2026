using Shurub;
using UnityEngine;

public class Ice : Ingredient
{
    protected override string IngredientName => "Ice";
    protected override bool IsCuttable => false;
    protected override bool IsBakable => false;
}
