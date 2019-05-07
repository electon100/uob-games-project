import json
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("recipes_file")
parser.add_argument("ingredients_file")
args = parser.parse_args()

def ingredients_match(ingredient_1, ingredient_2):
  return (ingredient_1['name'] == ingredient_2['name'] and ingredient_1['chopped'] == ingredient_2['chopped'] and ingredient_1['cooked'] == ingredient_2['cooked'])

def count_common_names(recipe_1, recipe_2):
  count = 0
  for ingredient_1 in recipe_1['ingredients']:
    for ingredient_2 in recipe_2['ingredients']:
      if ingredients_match(ingredient_1, ingredient_2):
        count += 1
  return count

def check_leq_n(recipes_json, n):
  result = True
  for recipe in recipes_json:
    if len(recipe['ingredients']) > n:
      print "Recipe has more than", n, "ingredients:", recipe['name']
      result = False
  return result

def check_duplicates(ingredients_json, recipes_json):
  result = True
  for recipe in recipes_json:
    ingredients = []
    for ingredient in recipe['ingredients']:
      if ingredient['name'] not in ingredients:
        ingredients.append(ingredient['name'])
      else:
        print "Ingredient:", ingredient['name'], "found twice in recipe:", recipe['name']
        result = False
  return result

def check_ambiguous_recipe(ingredients_json, recipes_json):
  result = True
  for i, recipe_1 in enumerate(recipes_json):
    for recipe_2 in recipes_json[i:]:
      if (recipe_1['name'] != recipe_2['name']):
        if (count_common_names(recipe_1, recipe_2) > 1):
          print "Ambiguous recipe found:", recipe_1['name'], recipe_2['name']
          result = False
  return result

def check_ingredients_match_recipes(ingredients_json, recipes_json):
  result = True
  for recipe in recipes_json:
    for recipe_ingredient in recipe['ingredients']:
      match_found = False
      for ingredient in ingredients_json:
        if recipe_ingredient['name'] == ingredient['name']:
          match_found = True
          if not ingredient['choppable'] and recipe_ingredient['chopped']:
            print "Recipe labelled as unchoppable but required as chopped in", recipe_ingredient['name']
            result = False
          if not ingredient['cookable'] and recipe_ingredient['cooked']:
            print "Recipe labelled as uncookable but required as cooked in", recipe_ingredient['name']
            result = False
      if not match_found:
        print "Recipe ingredient not found in ingredients JSON:", recipe_ingredient['name']
        result = False
  return result

with open(args.recipes_file) as r_file, open(args.ingredients_file) as i_file:
  recipes_json = json.load(r_file)['recipes']
  ingredients_json = json.load(i_file)['ingredients']

  test_1 = check_ambiguous_recipe(ingredients_json, recipes_json)
  test_2 = check_duplicates(ingredients_json, recipes_json)
  test_3 = check_leq_n(recipes_json, 3)
  test_4 = check_ingredients_match_recipes(ingredients_json, recipes_json)

  result = test_1 and test_2 and test_3 and test_4

  if result: print "All tests passed."
  else: print "One or more of the tests has failed. Details above."