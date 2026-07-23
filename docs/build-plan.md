# Build plan

The **authoritative** build plan for this client is preserved in the router repository, as the
appendix of
[ADR 0014 — Official .NET Client and Its Router-Side Prerequisites](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md).
It is kept there (single source of truth) rather than duplicated here, so the two cannot drift.

This file tracks **progress** against that plan.

## Phase status

### Scaffold (this repo, done)
- [x] Solution + three library projects + test project; `net8.0;net9.0`
- [x] `Directory.Build.props`: nullable, warnings-as-errors, deterministic, XML docs
- [x] Builds clean; xunit harness green
- [x] `.gitignore`, `.editorconfig`, MIT license, CI workflow

### Phase 1 — the shippable client (not started)
- [ ] **SDK audit first** — verify current `OpenAI` (v2.x) and
      `Microsoft.SemanticKernel.Connectors.OpenAI` APIs; record in [sdk-notes.md](sdk-notes.md)
      **before** writing code. Both moved during the `Azure.AI.OpenAI` → `OpenAI` +
      `System.ClientModel` transition.
- [ ] `System.ClientModel` pipeline policy: inject `X-Router-*` request headers, capture response headers
- [ ] `CorgiAI.ClientOptions`, DI registration, keyed registration
- [ ] `RouterStrategy` extensible enum
- [ ] Ambient scopes (`BeginScope`) + derived clients (`With`)
- [ ] Decision capture: ambient `LastDecision` + `IRouterDecisionSink`
- [ ] `PinAsync` with homogeneity check (needs router **R1**, **R2** — R2 is shipped)
- [ ] `RunAsync` with budget, abort/downshift, run summary (needs router **R3**)
- [ ] SK integration package + kernel-builder extension
- [ ] `ActivitySource` spans; `traceparent` end-to-end verification
- [ ] Sample: scheduled batch job (pin + budget + summary)
- [ ] Design ADRs written properly: no-subclassing, ambient-scopes-over-derived (docs/adr)

### Router-side prerequisites (tracked in the router repo)
`R1` `X-Router-Pin`, `R3` `X-Router-Cost-Usd`, `R4` `X-Router-Decision-Id` — additive headers;
`R2` `/v1/router/explain` is already shipped. See ADR 0014.

## Repository hygiene follow-ups
- [ ] SHA-pin GitHub Actions (currently tag-pinned) to match the router's zero-alerts posture
- [ ] Strong-named assemblies (`.snk`) — deferred from the scaffold
- [ ] Source Link + symbol packages wired for release
- [ ] NuGet publish from CI on tag; changelog from v0.1.0
- [ ] Semgrep + dependency scanning, matching the router repo
