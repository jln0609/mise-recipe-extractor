# OpenCode.ai gateway — investigation notes

Preliminary investigation into using opencode.ai as a multi-model gateway for
`IRecipeExtractor`, before building a real adapter. Not yet implemented — this
documents findings from manual `curl`/API testing only.

## What it is

An OpenAI-compatible API gateway (not an Anthropic product) proxying to multiple
underlying providers (OpenAI, Anthropic, Google, DeepSeek, xAI, and more) behind
one endpoint and API key. Two gateways: OpenCode Zen (pay-as-you-go, ~50 models)
and OpenCode Go (subscription, ~20 models). Standard Chat Completions format
(`/v1/chat/completions`), `Authorization: Bearer <key>` auth.


## Findings

- **`GET /v1/models`** — works reliably, lists available models.
- **Plain chat completion** (`deepseek-v4-flash`) — works correctly.
- **Forced tool-use** (`tool_choice` set to a specific function) — works correctly
  with `deepseek-v4-flash`. Response lands in
  `choices[0].message.tool_calls[0].function.arguments`, as a **JSON string**
  (needs a second parse step — unlike Anthropic's native format, where tool
  input is already a nested JSON object).
- **`claude-sonnet-5`** — fails with `{"type":"error","error":{"type":"error","message":"Internal server error"}}`
  on every attempt: forced tool_choice, `tool_choice: "auto"`, both fail
  identically. Plain chat completion with `claude-sonnet-5` was not separately
  retested but is likely also affected given the consistency of the failure.
- This matches publicly reported GitHub issues describing inconsistent 500s on
  this gateway, in some cases specific to certain models/providers. Appears to
  be a known, current issue with how OpenCode proxies to the Anthropic backend
  specifically — not something fixable on our end.

## Conclusion

- The gateway's OpenAI-compatible tool-use mechanism itself is confirmed working.
- Claude models are **not currently usable** through this gateway (at least
  `claude-sonnet-5`, at time of testing).
- Non-Claude models (confirmed: `deepseek-v4-flash`) work correctly and are a
  viable target for `OpenCodeRecipeExtractor`.
- `AnthropicRecipeExtractor` (direct-to-Anthropic) remains the path for Claude
  specifically; this gateway is for trying other model families
  (DeepSeek, GPT variants, etc.) on the same extraction task.

## Available models seen in this workspace (via `GET /v1/models`)

claude-fable-5-1, claude-sonnet-5, claude-haiku-4-5, gpt-5.6-sol, gpt-5.6-luna,
gpt-5-nano, muse-spark-1.2, deepseek-v4-flash, big-pickle, deepseek-v4-flash-free,
muse-spark-1.2-contributor-free, mimo-v2.5-free, ling-3.0-flash-fin-free,
nemotron-3-ultra-free, nemotron-3.5-lightning-free, laguna-s-2.1-free

## Next step when building the adapter

- Reuse existing DTOs (`ExtractedRecipeDto` and friends) and mapping methods —
  fully provider-agnostic.
- Reuse the system prompt text as-is.
- New request/response shape needed: OpenAI-style image content blocks (data
  URI `image_url`, not Anthropic's `source` object), OpenAI-style tool
  definition wrapper (`{"type": "function", "function": {...}}`), and the
  extra JSON-string-parsing step for tool call arguments.
- Start with `deepseek-v4-flash` as the target model, given confirmed working
  tool-use.