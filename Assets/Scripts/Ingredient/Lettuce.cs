using UnityEngine;

namespace Shurub
{
    public class Lettuce : Ingredient
    {
        public override string IngredientName => "Lettuce";
        public override bool IsCuttable => true;
        public override bool IsBakable => false;
    }
}
