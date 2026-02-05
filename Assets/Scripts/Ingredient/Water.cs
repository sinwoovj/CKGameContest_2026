using Shurub;
using UnityEngine;

public class Water : Ingredient
{
    protected override string IngredientName => "Water";
    protected override bool IsCuttable => false;
    protected override bool IsBakable => false;
}
