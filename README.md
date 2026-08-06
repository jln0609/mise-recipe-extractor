# Mise

An application that uses AI (multimodal vision/language models) to extract recipes from social media content — starting with Xiaohongshu (Little Red Book) screenshots — and converts them into a standardized, editable format that can be refined after actually cooking and testing the recipe.

The name comes from *mise en place* — having everything in its place before you start cooking. The app's job is to take the chaos of a screenshot (mixed Chinese/English text, vague quantities, emoji-as-structure) and put it in its place.

## Status: early scaffolding — vertical slice proven, no real persistence or AI yet

This project is also an explicit learning exercise in:
- Clean separation of concerns (Hexagonal / Ports & Adapters architecture)
- ASP.NET Core Web API with controllers
- Entity Framework Core (coming next)
- AI-assisted structured data extraction from multimodal input

## What's working right now

- Full request round trip: `POST /api/recipes` → controller → Core domain entity construction → in-memory fake repository → stored → `GET /api/recipes` → retrieved → mapped to DTO → returned as JSON
- Verified correct handling of non-ASCII (Chinese) text end-to-end — this matters a lot given the primary source content is Chinese-language
- Core domain model has a first real invariant (`Recipe.AddVersion` auto-incrementing version numbers) covered by unit tests

## Architecture

This project follows a **Hexagonal (Ports & Adapters) architecture**, not a traditional layered/n-tier structure. The distinction matters: `Infrastructure` and `AI` are not nested layers, they're peer adapters, both implementing interfaces (`ports`) defined by `Core`. `Core` depends on nothing else in the solution — a mechanically enforced rule, not just a convention (attempting to reference EF Core, ASP.NET, or the Anthropic SDK from `Core` fails to compile).

```
src/
├── MiseRecipeExtractor.Core/            — domain entities, value objects, interfaces (ports). No external dependencies.
├── MiseRecipeExtractor.Infrastructure/  — persistence adapter (EF Core, coming next). Depends on Core.
├── MiseRecipeExtractor.AI/              — AI extraction adapter (Anthropic API, coming next). Depends on Core.
└── MiseRecipeExtractor.Api/             — ASP.NET Core Web API. Composition root + driving adapter. Depends on Core, Infrastructure, AI.

tests/
├── MiseRecipeExtractor.Core.Tests/
├── MiseRecipeExtractor.Infrastructure.Tests/
├── MiseRecipeExtractor.AI.Tests/
└── MiseRecipeExtractor.Api.IntegrationTests/
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

Right now, `Api` uses a temporary `InMemoryRecipeRepository` (in `Api/Fakes/`) as a stand-in for `IRecipeRepository`, registered as a DI (Dependency Injection) singleton, purely to prove the wiring works before real persistence exists. This fake will be deleted once the real EF Core-backed repository is built in `Infrastructure`.

## Domain model

Core entities, using C# primary constructors and immutable-by-default properties (no setters unless the field is genuinely meant to be editable, e.g. during recipe testing/adjustment):

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
- **Multi-slide/carousel screenshots** — a single extraction call will take multiple images and merge them into one `RecipeVersion` (not yet implemented).
- **Mixed original + app-translated text in the same screenshot** — the extraction prompt (not yet built) needs to distinguish genuine source text from Xiaohongshu's own baked-in machine translation.
- **Emoji-as-structure** — emoji in captions often function as semantic bullets/section markers (🔥 heat, ⏰ timing), not decoration; the extraction prompt needs to account for this.
- **Confidence/ambiguity surfacing** — `Quantity.ConfidenceLevel` and `Step.OrderIsInferred` are the first-class ways ambiguity is represented, rather than silently resolved.

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
- **EF Core** (Entity Framework Core) — not yet added; SQLite is the planned provider for local/personal use
- AI provider — not yet integrated; Anthropic's API is the current plan


## Why ASP.NET Core

The domain — structured recipes with ingredients, steps, and version history — maps naturally onto EF Core's relational modeling, and ASP.NET Core's controller/DI (Dependency Injection) conventions give each concern (HTTP boundary, orchestration, persistence, AI integration) a clean, separated home. Calling an LLM (Large Language Model) API from C# is no harder than from Python or Node.js — it's just an HTTP POST — so nothing about the AI integration specifically required a different stack.

The trade-off: more ceremony around async orchestration and DI container setup than a comparable Python/Node script would need for the same functionality.

## Known open items / deliberate deferrals

- `Microsoft.OpenApi` transitive package currently resolves to a version with a known (low-real-world-risk for this project) CVE (GHSA-v5pm-xwqc-g5wc), pending an upstream fix in `Microsoft.AspNetCore.OpenApi`. Deliberately not addressed yet.
- `app.UseHttpsRedirection()` is commented out in `Program.cs` for local development convenience (avoids local dev-certificate friction). Needs to be reinstated before any real deployment.
- DTOs currently flatten `LocalizedText` into `XOriginal`/`XTranslated` field pairs rather than nesting. Fine for the current single-field (title-only) request shape; will likely need revisiting once ingredients/steps are included in create/update requests.

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

## Next steps

1. Real `Infrastructure` implementation of `IRecipeRepository` using EF Core + SQLite; first migration
2. Delete `InMemoryRecipeRepository` once the real one is in place
3. `AI` implementation of `IRecipeExtractor` — prompt design, image input, structured JSON output, response parsing/validation
4. Wire AI + real persistence together in `Api`; add integration tests
5. iOS ingestion via Shortcuts