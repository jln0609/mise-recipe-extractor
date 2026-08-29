# Sample extraction output

A real run of `AnthropicRecipeExtractor` against a 4-image Xiaohongshu recipe post
(via `tools/MiseRecipeExtractor.Sandbox`), included as a concrete reference for what
the extractor actually produces — useful for judging prompt quality over time as the
prompt evolves, and for anyone reviewing the project who wants to see real output
rather than just a description of the design.

**Source**: 4 screenshots, one Xiaohongshu post (Chinese, mixed text/photo captions),
not included in the repo (real third-party content).

**Cost**: this run + two earlier iterations while adjusting console output formatting
totaled ~\$0.14 in Anthropic API usage — roughly $0.05 per 4-image extraction.

**Model**: `claude-sonnet-4-6`

## Output
```
Loaded 4 images from C:\Users\jli08\Documents\Git\MiseRecipeExtractor\test screenshots
Detected language: zh
Title: 🇬🇧2镑中式点心｜10分钟速成糯叽叽老婆饼 / 🇬🇧 £2 Chinese Pastry | 10-Minute Quick Chewy Wife Cake

Ingredients (9):
 - Puff pastry sheet (酥皮): 1sheet [Explicit] (Cut into 15 pieces (5×3 grid of near-squares))
 - Glutinous rice flour (糯米粉): 50g [Explicit] (If no coconut is used, add an extra 20g glutinous rice flour (total 70g))
 - Water (水): 100g [Explicit] (Part of the water can be replaced with an equal amount of milk for extra milky flavour)
 - Butter (黄油): 25g [Explicit]
 - Sugar (糖): 50g [Explicit]
 - Desiccated coconut (椰蓉): 20g [Explicit] (Optional; if unavailable, substitute with an extra 20g glutinous rice flour)
 - Milk (牛奶): 20g [Explicit] (Optional substitute for part of the water; adds a stronger milky aroma)
 - Egg yolk (蛋黄): 1 [Explicit] (For brushing on the surface as egg wash)
 - Sesame seeds (芝麻): 适量 [Unspecified] (Sprinkled on top before baking)

Steps (7):
 1. Combine glutinous rice flour, water, butter, and sugar — and if available, add desiccated coconut and white sesame seeds — and pour all ingredients into a pot.
 2. Cook over medium-low heat, stirring constantly until the mixture comes together into a dough/ball. All ingredients must be cooked over medium-low heat while stirring non-stop until they clump together.
 3. Cut one sheet of puff pastry into a 5×3 grid, yielding 15 roughly square pieces.
 4. Once the filling has cooled, wrap it inside each piece of puff pastry. Don't overfill — use just enough filling to be enclosed. Place the pastries seam-side down on a baking tray.
 5. Brush the surface with egg yolk wash, sprinkle with sesame seeds, and score three cuts on the top of each pastry.
 6. Bake in the oven at 180°C for 15–20 minutes, until the egg wash on the surface is golden and lightly browned. Oven performance varies, so judge by the colour of the egg wash — remove when it turns a slightly caramelised yellow. [1200]
 7. If finishing within 1–2 days, refrigerate: fresh out of the oven the filling is molten and chewy, but after refrigerating it tastes just like traditional wife cake from China (no need for coconut or sesame). Strongly recommended to eat straight from the fridge. If there are leftovers, freeze them and reheat in the microwave for 2–3 minutes or in a 160°C oven for 10 minutes. (order inferred)

Warnings: The bottom of the first image is partially cut off, obscuring the end of the storage instructions. The full text is recovered from the second image.; The step-by-step photo captions in image 3 mention '白芝麻' (white sesame seeds) mixed into the filling (step 1 caption), but the ingredients list in the text only mentions sesame seeds for topping. It is unclear whether white sesame seeds should also be mixed into the filling — this may be an optional addition consistent with the '有条件' (if available) note.; The recipe yield is 15 pieces (from one sheet of puff pastry), but the baking tray photo in image 4 appears to show approximately 13–14 pieces, which may indicate some filling was used differently or one sheet was not fully used.
```

## Notes

- Correctly distinguished explicit quantities (50g, 100g) from a genuinely vague one
  (适量 / "appropriate amount"), assigning `Explicit` vs `Unspecified` confidence
  correctly.
- Warnings were substantive, not noise: recovered text cut off in one image by
  cross-referencing a second image; flagged a text/photo discrepancy (sesame seed
  colour); flagged a recipe-yield-vs-photo discrepancy (15 stated vs ~13-14 visible).
- Step count varied slightly (7 vs 8) between runs — same underlying content,
  different grouping. Worth keeping in mind: extraction is not perfectly
  deterministic run-to-run.