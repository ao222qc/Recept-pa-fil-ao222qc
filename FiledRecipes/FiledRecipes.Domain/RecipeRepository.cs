﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        //protected enum Present {Recept, Ingredienser, Instruktioner}
        public void Load()
        {
            //lista
            List<IRecipe> lista = new List<IRecipe>();
            Recipe recept = null;
            
            RecipeReadStatus status = new RecipeReadStatus();


            using (StreamReader reader = new StreamReader(@"Recipes.txt", System.Text.Encoding.UTF7))
            {

                string line;

                while ((line = reader.ReadLine()) != null)
                {

                    switch (line)
                    {
                        case SectionRecipe:
                            status = RecipeReadStatus.New;
                            continue;
                        case SectionIngredients:
                            status = RecipeReadStatus.Ingredient;
                            continue;
                        case SectionInstructions:
                            status = RecipeReadStatus.Instruction;
                            continue;
                    }
                    switch (status)
                    {
                        case RecipeReadStatus.New:
                            recept = new Recipe(line);
                            lista.Add(recept);
                            break;
                        case RecipeReadStatus.Ingredient:
                            string[] ingredients = line.Split(';');

                            Ingredient ingredienser = new Ingredient();
                            ingredienser.Amount = ingredients[0];
                            ingredienser.Measure = ingredients[1];
                            ingredienser.Name = ingredients[2];

                            if (ingredients.Length % 3 != 0)
                            {
                                throw new FileFormatException();
                            }
                            recept.Add(ingredienser);

                            break;
                        case RecipeReadStatus.Instruction:
                            recept.Add(line);
                            break;
                        case RecipeReadStatus.Indefinite:
                            throw new FileFormatException();

                    }
                }
            }
            _recipes = lista.OrderBy(recipe => recipe.Name).ToList();
                    IsModified = false;
                    OnRecipesChanged(EventArgs.Empty);
        }
      
        public void Save()
        {
            
            using (StreamWriter writer = new StreamWriter(@"Recipes.txt", false, Encoding.Default)) 
            {
               

                foreach (IRecipe recipe in _recipes)
                {
                    writer.WriteLine(SectionRecipe);
                    writer.WriteLine(recipe.Name);
                    writer.WriteLine(SectionIngredients);

                    foreach (IIngredient ingrediens in recipe.Ingredients)
                    {
                        writer.WriteLine("{0};{1};{2}", ingrediens.Amount, ingrediens.Measure, ingrediens.Name);
                    }

                    writer.WriteLine(SectionInstructions);

                    foreach (string instruction in recipe.Instructions)
                    {
                        writer.WriteLine(instruction);
                    }
                    

                }

            }
            

        }

    }
}
