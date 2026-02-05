using Shurub;
using UnityEngine;

public class Fish : Ingredient
{
    protected override string IngredientName => "Fish";
    protected override bool IsCuttable => false;
    protected override bool IsBakable => true;
}
