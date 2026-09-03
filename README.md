# Mise

An application that uses AI (multimodal vision/language models) to extract recipes from social media content — starting with Xiaohongshu (Little Red Book) screenshots — and converts them into a standardized, editable format that can be refined after actually cooking and testing the recipe.

The name comes from *mise en place* — having everything in its place before you start cooking. The app's job is to take the chaos of a screenshot (mixed Chinese/English text, vague quantities, emoji-as-structure) and put it in its place.


## Status: full pipeline working end-to-end — screenshot upload → AI extraction → persistence → retrieval, via the real HTTP API


## What's working right now

- Full request round trip: `POST /api/recipes` → controller → Core domain entity construction → **EF Core + SQLite persistence** → `GET /api/recipes` → retrieved → mapped to DTO → returned as JSON. Verified to survive a full app process restart, confirming genuine on-disk persistence, not just in-memory state.
- Verified correct handling of non-ASCII (Chinese) text end-to-end — this matters a lot given the primary source content is Chinese-language
- Core domain model has a first real invariant (`Recipe.AddVersion` auto-incrementing version numbers) covered by unit tests
- `EfRecipeRepository` implements `IRecipeRepository` against `RecipeDbContext`, using `ComplexProperty` (not `OwnsOne`) for value objects (`LocalizedText`, `Quantity`, `SourceMetadata`), eager loading via `Include`/`ThenInclude`, and split-query behavior configured to avoid cartesian-product query blow-up across sibling collections (`Ingredients`/`Steps`)
- `AnthropicRecipeExtractor` implements `IRecipeExtractor` via a direct call to the Anthropic Messages API, using tool-use (schema-constrained structured output) rather than prose-parsed JSON. Tested end-to-end against a real, moderately complex 4-image Xiaohongshu recipe post (mixed Chinese text, ingredient substitution notes, storage instructions) via a standalone sandbox console app (`tools/MiseRecipeExtractor.Sandbox`). Results: correct language detection, sensible translations, correct `ConfidenceLevel` assignment (explicit gram amounts vs. a genuinely vague "适量"/"appropriate amount" ingredient), successful multi-image merging into one coherent recipe, and useful, substantive entries in `Warnings` (a cross-referenced cut-off text recovered from a second image; a text/photo discrepancy noted; a recipe-yield-vs-photo discrepancy noted). Sample output and cost data: [`docs/sample-extraction-260829.md`](docs/sample-extraction-260829.md).
- **`POST /api/extractions` implemented and verified end-to-end**: multipart image upload → `ExtractionsController` → `ExtractAndCreateRecipeCommand` (Core use case) → `AnthropicRecipeExtractor` → `Recipe`/`RecipeVersion` construction (correct `VersionNumber` via `Recipe.AddVersion`, `DetectedSourceLanguage` → `SourceMetadata.OriginalLanguage`) → `EfRecipeRepository` persistence → returned as `RecipeResponse`. Confirmed the persisted recipe round-trips correctly through the separate `GET /api/recipes` endpoint too. Tested from a clean clone on a different machine than where it was built.
- **`Api.IntegrationTests` created**, using `WebApplicationFactory<Program>` against a `CustomWebApplicationFactory` that swaps in an in-memory SQLite database and a `FakeRecipeExtractor` (avoiding real, billed Anthropic API calls in the test suite). `ExtractionsControllerTests` covers the `POST /api/extractions` write path end-to-end through real `EfRecipeRepository`/EF Core persistence (split-query behavior included). `RecipesControllerTests` separately covers the `POST /api/recipes` → `GET /api/recipes/{id}` round trip, seeded independently of the extraction pipeline to keep the two controllers' tests isolated.

## Architecture

This project follows a **Hexagonal (Ports & Adapters) architecture**, not a traditional layered/n-tier structure. The distinction matters: `Infrastructure` and `AI` are not nested layers, they're peer adapters, both implementing interfaces (`ports`) defined by `Core`. `Core` depends on nothing else in the solution — a mechanically enforced rule, not just a convention (attempting to reference EF Core, ASP.NET, or the Anthropic SDK from `Core` fails to compile).

```
src/
- MiseRecipeExtractor.Core/            — domain entities, value objects, interfaces (ports). No external dependencies.
- MiseRecipeExtractor.Infrastructure/  — persistence adapter (EF Core, coming next). Depends on Core.
- MiseRecipeExtractor.AI/              — AI extraction adapter (Anthropic API). Depends on Core.
- MiseRecipeExtractor.Api/             — ASP.NET Core Web API. Composition root + driving adapter. Depends on Core, Infrastructure, AI.

tests/
- MiseRecipeExtractor.Core.Tests/
- MiseRecipeExtractor.Infrastructure.Tests/
- MiseRecipeExtractor.AI.Tests/
- MiseRecipeExtractor.Api.IntegrationTests/
```

### Dependency rule

```
Core  ←  Infrastructure
Core  ←  AI
Core, Infrastructure, AI  ←  Api  (composition root)
```

`Core` defines two ports so far:
- `IRecipeRepository` — persistence
- `IRecipeExtractor` — AI-based extraction from images

`Infrastructure` implements `IRecipeRepository` via `EfRecipeRepository`, backed by `RecipeDbContext` (EF Core + SQLite), registered as `Scoped` in DI (Dependency Injection). An earlier `InMemoryRecipeRepository` fake was used to prove the API → Core wiring before real persistence existed, and has since been removed.

`Core` also has its first **use case** (application service): `ExtractAndCreateRecipeCommand`, in `Core/UseCases/`. It orchestrates `IRecipeExtractor` + `IRecipeRepository` together — calls the extractor, builds a new `Recipe`/`SourceMetadata` from the result (including mapping `ExtractionResult.DetectedSourceLanguage`), adds the extracted content as version 1 via `Recipe.AddVersion` (which resolves the `VersionNumber` placeholder noted below), and persists it. This is the layer that was always meant to own "what does an extraction result actually become" — deliberately kept out of both `IRecipeExtractor` (stateless, no knowledge of new-vs-existing recipes) and the controller (thin HTTP boundary only).

**Two separate controllers**, deliberately: `RecipesController` (`GET/POST /api/recipes`, `GET /api/recipes/{id}`) depends only on `IRecipeRepository` — plain CRUD over the recipe resource. `ExtractionsController` (`POST /api/extractions`) depends only on `ExtractAndCreateRecipeCommand` — a distinct operation with a different request shape (multipart file upload vs. JSON), different dependency weight (an external AI call, with its own latency/cost/failure modes, vs. plain local persistence), and a genuinely different realistic caller (e.g. the planned iOS Shortcut would call only `/api/extractions`, never touching `/api/recipes` directly). The two controllers share `RecipeResponse` and a `RecipeResponseMapper.ToResponse` static helper (both in `Api/Dtos/`) rather than duplicating the mapping — the only remaining coupling is `ExtractionsController` referencing `RecipesController`'s `GetById` action by name in `CreatedAtAction`, for a correct `Location` header.


## Domain model

Core entities, using `init`-only properties and immutable-by-default construction (no setters unless the field is genuinely meant to be editable, e.g. during recipe testing/adjustment):

- **`Recipe`** — aggregate root. Holds `SourceMetadata` and a history of `RecipeVersion`s. `AddVersion(...)` is the only way to add a version; it owns version-number incrementing.
- **`RecipeVersion`** — a full snapshot: title, ingredients, steps, status (`Draft` / `Tested` / `Adjusted`), free-text notes. Snapshots (not diffs/patches) were chosen deliberately for simplicity at this data scale — diffing between versions can be computed on read rather than stored.
- **`Ingredient`** — name (`LocalizedText`), `Quantity`, optional notes (e.g. "or substitute with X").
- **`Step`** — ordered instruction text, optional duration, and an `OrderIsInferred` flag for cases where the AI had to guess step order rather than reading explicit numbering.
- **`SourceMetadata`** — platform, source URL, original language, extraction timestamp.

Value objects:

- **`LocalizedText`** — pairs an `Original` (immutable, ground truth from the source) with an optional `Translated` (mutable — translations can be corrected). Used for titles, ingredient names, and step text. If source and target language are the same (or content was hand-typed in English), `Translated` stays null.
- **`Quantity`** — deliberately does *not* force a numeric amount. Holds `OriginalText` (immutable — always preserves exactly what the source said, e.g. "适量" or "200g"), plus a mutable `Amount`/`Unit` that can be filled in as a guess or corrected after testing, and a `ConfidenceLevel` (`Explicit` / `Estimated` / `Unspecified`) flagging how trustworthy the numeric value is. This directly addresses a known domain hurdle: Chinese recipes frequently use vague quantity terms (适量 "appropriate amount", 少许 "a little") that shouldn't be silently converted into false-precision numbers.

## Known domain hurdles this is designed around

- **Vague Chinese quantities** — handled via `Quantity.ConfidenceLevel`, see above.
- **Multi-slide/carousel screenshots** — a single extraction call takes multiple images and merges them into one `RecipeVersion`. Validated against a real 4-image post.
- **Mixed original + app-translated text in the same screenshot** — the extraction prompt explicitly instructs the model to distinguish genuine source text from a platform's own baked-in machine translation and produce its own translation rather than copying it. Not yet stress-tested against a screenshot that actually contains this specific case.
- **Emoji-as-structure** — emoji in captions often function as semantic bullets/section markers (🔥 heat, ⏰ timing), not decoration; the extraction prompt needs to account for this.
- **Confidence/ambiguity surfacing** — `Quantity.ConfidenceLevel` and `Step.OrderIsInferred` are the first-class ways ambiguity is represented, rather than silently resolved.

## AI extraction: two parallel adapters (planned)

`IRecipeExtractor` will get two separate implementations in `AI`, both satisfying the same interface:

- **`AnthropicRecipeExtractor`** — direct call to the Anthropic Messages API (C#, `HttpClient`), using tool-use for schema-constrained structured output. Billed per-token via standard API credits. **Implemented and validated against real data** (see "What's working right now" above).
- **`AgentSdkRecipeExtractor`** — uses Anthropic's Agent SDK (officially Python/TypeScript only) to draw on the separate monthly Agent SDK credit bundled with a Pro/Max subscription, rather than pay-per-token billing. Since the Agent SDK has no official .NET package, this adapter wraps a small TypeScript process, called from its C# implementation of `IRecipeExtractor`.
- opencode.ai investigated as a potential third `IRecipeExtractor` adapter (multi-model gateway) — see [`docs/opencode-260902.md`](docs/opencode-investigation-260902.md). Tool-use confirmed working via `deepseek-v4-flash`; Claude models currently broken on this gateway (external issue, not ours). Not yet built.

The direct API version was built and validated first (simpler, keeps prompt/schema/parsing work in one language while that gets nailed down); the Agent SDK version follows.



## Video ingestion (future scope)

No Xiaohongshu API exists, and scraping the platform was deliberately ruled out (ToS violations, legal exposure, fragile reverse-engineering). The planned approach: treat video the same as screenshots — accept whatever file the user has already extracted themselves (manual save, screen recording, or a share link), keeping the app in "processes content the user already has" territory rather than "accesses the platform directly."

## Conversational recipe editing (future scope)

Longer-term idea: a chat-style interface for editing recipes conversationally (e.g. "make this vegetarian," "double the recipe," "is this already in my collection?"), rather than only a structured edit form.

This is a good candidate for **Semantic Kernel** (Microsoft's .NET AI orchestration framework), specifically its **plugin** model — wrapping application functions (duplicate-recipe lookup, dietary classification, unit conversion, ingredient-name glossary lookups) as tools the model can choose to call mid-conversation, in whatever order the conversation actually calls for.

This is a deliberately different problem from the structured extraction pipeline (`IRecipeExtractor`), where the sequence of steps is always fixed and known ahead of time — for that pipeline, plain orchestration code in `Core`'s use cases is simpler and sufficient; there's no ambiguity for an AI to resolve about *when* to call something. Semantic Kernel earns its place specifically once the flow becomes open-ended and user-driven, which the extraction pipeline currently isn't.


## iOS ingestion (future scope)

Planned approach: iOS Shortcuts app (or a Share Sheet integration) POSTs screenshots/videos to this API. No native iOS app planned initially — starting with the lowest-friction option (Shortcuts → webhook) before considering a Share Extension.

## Tech stack

- **.NET 10** / **ASP.NET Core Web API** (controller-based, not minimal APIs — chosen partly to build a solid understanding of the controller model)
- **xUnit** for testing
- **EF Core** (Entity Framework Core) with **SQLite** — in place; entities use `init`-only properties (not primary constructors — see "Known open items" below) and `ComplexProperty` for value objects

- AI provider — not yet integrated; Anthropic's API is the current plan


## Why ASP.NET Core

The domain — structured recipes with ingredients, steps, and version history — maps naturally onto EF Core's relational modeling, and ASP.NET Core's controller/DI (Dependency Injection) conventions give each concern (HTTP boundary, orchestration, persistence, AI integration) a clean, separated home. A direct HTTP call to an LLM (Large Language Model) API is no harder in C# than in Python or Node.js. The one place the stack does expand beyond C# is the planned `AgentSdkRecipeExtractor` (see "AI extraction" below) — Anthropic's Agent SDK, needed to draw on subscription credit rather than pay-per-token billing, is officially Python/TypeScript only. That's a real, product-driven constraint, not a limitation of C#/ASP.NET Core itself, and the Hexagonal architecture contains its impact to a single adapter.


The trade-off: more ceremony around async orchestration and DI container setup than a comparable Python/Node script would need for the same functionality.

## Known open items / deliberate deferrals

- `Microsoft.OpenApi` transitive package currently resolves to a version with a known (low-real-world-risk for this project) CVE (GHSA-v5pm-xwqc-g5wc), pending an upstream fix in `Microsoft.AspNetCore.OpenApi`. Deliberately not addressed yet.
- `app.UseHttpsRedirection()` is commented out in `Program.cs` for local development convenience (avoids local dev-certificate friction). Needs to be reinstated before any real deployment.
- DTOs currently flatten `LocalizedText` into `XOriginal`/`XTranslated` field pairs rather than nesting. Fine for the current single-field (title-only) request shape; will likely need revisiting once ingredients/steps are included in create/update requests.
- Core entities originally used C# primary constructors (immutable, constructor-enforced). This was reverted to `init`-only properties with no custom constructors, after hitting a confirmed open EF Core bug: complex types (`LocalizedText`, `Quantity`) cannot be constructor-bound when nested inside another type's primary constructor. `init` properties preserve immutability-after-construction without triggering this limitation.
- `EfRecipeRepository.UpdateAsync` reconciles `Recipe` and top-level `RecipeVersion` fields precisely (only changed fields generate SQL updates), but does not reconcile `Ingredient`/`Step` fields within an *already-saved* version. This is deliberate: the current domain model treats a `RecipeVersion`'s ingredients/steps as an immutable snapshot — "adjusting" a recipe is expected to mean creating a new version via `Recipe.AddVersion`, not editing an existing version's ingredients in place. Revisit if in-place editing of `Draft`-status versions becomes a real feature.
- `AnthropicRecipeExtractor.MapToExtractionResult` sets `RecipeVersion.VersionNumber` to a placeholder value of `1`, since `IRecipeExtractor` is stateless and has no knowledge of whether an extraction is for a new `Recipe` or a re-extraction being added to an existing one's version history. Callers must treat this as provisional and always go through `Recipe.AddVersion` (which handles correct auto-incrementing) rather than relying on the returned value directly. Only `AnthropicRecipeExtractor` exists so far — this note will need revisiting once the Core use-case layer (`ExtractAndCreateRecipeCommand` / `ExtractAndAddVersionCommand`, not yet built) is implemented, since it's the natural place to resolve this properly.
  - Resolved for the new-recipe path: `ExtractAndCreateRecipeCommand` now exists and correctly goes through `Recipe.AddVersion` rather than trusting the placeholder. Still open for re-extraction into an *existing* recipe's history — `ExtractAndAddVersionCommand` isn't built yet.

- `AnthropicRecipeExtractor` currently only supports PNG and JPEG images (detected via byte signature, not file extension). HEIC (the default format for photos taken directly on iOS) is not yet handled. Not currently a problem since iOS screenshots specifically are PNG by convention regardless of the photo-format setting, but worth revisiting if actual camera photos (not screenshots) are ever fed in directly.
- `Quantity` has no `Translated` field — only `OriginalText` plus optional numeric `Amount`/`Unit`. For purely numeric quantities this is fine (`50g` needs no translation), but word-based quantity descriptions (e.g. "一张"/"one sheet") currently have no English rendering anywhere in the data. Deliberately deferred; will need a schema/prompt/domain-model/migration change together when addressed.
- `CreateRecipeRequest.Platform` previously defaulted to `"Xiaohongshu"` — a leftover from before the project's scope generalized away from a single-platform assumption. Fixed to default to `string.Empty`, consistent with `CreateExtractionRequest`. No request validation (e.g. rejecting an empty platform) added yet.
  d

## Running locally

```powershell
dotnet build
dotnet run --project src/MiseRecipeExtractor.Api
```

Server listens on `http://localhost:5249` (HTTP profile) per `launchSettings.json`.

Example request (PowerShell — note: explicit UTF-8 byte conversion is required for non-ASCII text to survive the request correctly; passing a raw string to `-Body` can silently mangle Chinese characters depending on console encoding):

```powershell
$bodyObject = @{
    platform = "Xiaohongshu"
    titleOriginal = "红烧肉"
    titleTranslated = "Braised Pork"
}
$jsonString = $bodyObject | ConvertTo-Json
$utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($jsonString)

Invoke-RestMethod -Uri "http://localhost:5249/api/recipes" -Method Post -ContentType "application/json" -Body $utf8Bytes
Invoke-RestMethod -Uri "http://localhost:5249/api/recipes"
```

## Running tests

```powershell
dotnet test
```

## Testing the AI extractor manually

`tools/MiseRecipeExtractor.Sandbox` is a small standalone console app for running `AnthropicRecipeExtractor` against real images without going through the full API — useful for iterating on the prompt/schema. Requires `Anthropic:ApiKey` in User Secrets (see "Running locally") and a local folder of test screenshots (not committed — real recipe content, gitignored).

```powershell
cd tools/MiseRecipeExtractor.Sandbox
dotnet run
```

## Next steps

1. `ExtractAndAddVersionCommand` — re-extraction into an existing recipe's version history
2. opencode.ai IRecipeExtractor implementation 
3. Broader prompt testing (other languages, messier source posts)
4. iOS ingestion via Shortcuts
5. `AgentSdkRecipeExtractor` (TypeScript, Agent SDK)?
