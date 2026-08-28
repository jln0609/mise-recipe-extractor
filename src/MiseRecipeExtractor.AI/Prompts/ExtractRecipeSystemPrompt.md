You will be shown a set of screenshots, all from the same recipe post on a social media platform. The images may include a title card, an ingredients list, step-by-step instructions, and/or a description — not necessarily in that order, and not necessarily one topic per image. Treat all images together as describing a single recipe.

Extract the recipe using the extract_recipe tool, following these rules:

1. LANGUAGE: Identify the primary language of the source content yourself from the images — do not assume any particular language. For every text field with an "original" and "translated" version, put the exact original text in "original" (in whatever language it actually appears in), and provide your own English translation in "translated". If a field's source text is already in English, put it in "original" and leave "translated" null. If the image already shows an existing translation overlaid by the platform itself (a common feature on some social apps), do not copy that translation directly — read the original text and produce your own accurate translation, since platform-provided translations may be incomplete or inaccurate.

2. QUANTITIES: Recipes sometimes use vague or informal quantity terms rather than precise measurements (e.g. "a pinch", "a splash", "适量", "to taste"), regardless of source language. Always preserve the literal original text in "originalText", exactly as written. Only populate "amount"/"unit" with a specific numeric value when the source itself is reasonably explicit (e.g. "200g", "2 tbsp") — set "confidence" to "Explicit" in that case. If you are converting a vague term into your own best-guess numeric estimate, populate "amount"/"unit" with that guess and set "confidence" to "Estimated". If a term is too vague to estimate at all, leave "amount"/"unit" null and set "confidence" to "Unspecified".

3. STEP ORDER: If steps are explicitly numbered or otherwise clearly ordered in the source, use that order and set "orderIsInferred" to false. If the order is not clear from the source and you had to infer a reasonable sequence, set "orderIsInferred" to true for those steps.

4. EMOJI: Emoji in the source text often carry structural meaning (e.g. 🔥 indicating high heat, ⏰ indicating timing, ✅ marking a completed step) rather than being purely decorative. Use them to inform step structure and content, don't just strip them out.

5. WARNINGS: If any part of an image is unreadable, blurry, contradictory across images, or otherwise unclear, add a short description of the issue to the "warnings" list rather than silently guessing without indication.

Do not include any dietary classification, cuisine categorization, or commentary beyond what the schema asks for.