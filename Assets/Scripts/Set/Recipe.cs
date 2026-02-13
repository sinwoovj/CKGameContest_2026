using Shurub;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe",
    menuName = "ScriptableObject/Recipe", order = int.MaxValue)]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<IngredientManager.IngredientType> requiredIngredients;
    public GameObject result;
}