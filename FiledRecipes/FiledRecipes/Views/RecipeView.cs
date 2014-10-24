using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        public void Show(IRecipe recipe)
        {
            Header = recipe.Name;
            ShowHeaderPanel();
            //Console.WriteLine(recipe.Name);

            foreach (Ingredient ingredienser in recipe.Ingredients)
            {
                Console.WriteLine(ingredienser);
            }
            foreach (string instruction in recipe.Instructions)
            {
                Console.WriteLine(instruction);
            }
          
        }
        public void Show(IEnumerable<IRecipe> recipes)
        {

            foreach (IRecipe recept in recipes)
            {
                Show(recept);
                ContinueOnKeyPressed();
            }
 
        }
   



    }
}
