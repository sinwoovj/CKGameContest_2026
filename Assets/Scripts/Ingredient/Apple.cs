using Shurub;
using UnityEngine;

public class Apple : Ingredient
{
    protected override string IngredientName => "Apple";
    protected override bool IsCuttable => true;
    protected override bool IsBakable => false;
}
