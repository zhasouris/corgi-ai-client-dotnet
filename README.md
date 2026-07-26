# 🐕 CorgiAI.Client (.NET)

**The .NET client that keeps the herd together across a run — routing control first-class in
Semantic Kernel, without replacing the OpenAI client.**

[![ci](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/ci.yml)
[![codeql](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/codeql.yml/badge.svg)](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/codeql.yml)
![status](https://img.shields.io/badge/status-scaffold-orange)
![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-512bd4)
![license](https://img.shields.io/badge/license-MIT-blue)

The [`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway) herds each request to
the best model — one request at a time. This client is what remembers the herd *across*
requests: it pins a decision for a batch, tracks a run's budget, and hands you back the model
that answered — all while staying a genuine `OpenAIClient` that Semantic Kernel can't tell
apart from a plain one.

```
your job ──▶ CorgiAI.Client (scope: cost, budget $5) ──▶ corgi-ai-gateway ──▶ the right model
                   │                                             │
          remembers the decision, the spend,      ◀──────────────┘  X-Router-* headers
          and the running total for the job
```

> **About this project.** A .NET client built to be *read* as much as used. Its two
> load-bearing decisions are written down as ADRs before any code exists; the awkward part —
> that pinning is really a way of turning routing *off* — is faced head-on with a measured
> homogeneity check rather than glossed over. If you're evaluating the engineering, start with
> [the premise](#the-premise), [not a wrapper](#not-a-wrapper), and the
> [build plan](docs/build-plan.md).

> **Status: scaffold.** The solution builds, CI is green, and the security posture is in place —
> but the client is not implemented yet. It is the home for the .NET client specified in the
> gateway's [ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md),
> which is authoritative. Work proceeds in phases — see [docs/build-plan.md](docs/build-plan.md).

---

## At a glance

- **Not a wrapper.** It configures a *real* `OpenAIClient` via a `System.ClientModel` pipeline
  policy — so it drops into `AddOpenAIChatCompletion(modelId, client)` and any library that
  accepts one, with zero compatibility surface of its own.
- **State the proxy can't hold**, not header ergonomics: pinning, run-level budget, decision
  capture, run aggregation — the four things a *stateless* gateway leaves to the caller.
- **Ambient by design.** Semantic Kernel owns the client for the process lifetime, so control
  flows through an `AsyncLocal` scope — `BeginScope(...)` — not a client you hand it.
- **Pinning is proven, not assumed.** Because pinning a batch is strictly a way of *not*
  re-deciding, it ships with a homogeneity check that refuses to pin work that wouldn't route
  the same way.
- **Traced end to end.** W3C `traceparent` flows through the pipeline to the gateway, so one
  trace shows the routing decision inline — the payoff of the gateway's OpenTelemetry work.
- **Yours.** MIT, multi-targeted `net8.0`/`net9.0`, published to NuGet — no wrapper to
  outgrow, no version-coupling to SDK internals.

---

## The premise

The gateway is deliberately **stateless** — it classifies and routes each request on its own,
and forgets it the moment the response streams back. But real work is stateful *across*
requests: a batch job, a scheduled run, an agent loop. **That state has nowhere to live in a
stateless proxy — so it lives here.** That, and not header convenience, is the whole reason
this client exists. It provides four things the gateway structurally cannot:

1. **Pinning** — decide once for a homogeneous batch, then execute N times without re-paying
   classification latency and spend, and without breaking provider prompt caching by switching
   models mid-run.
2. **Run-level budget** — cumulative spend across a whole run, with abort or strategy-downshift
   on approach. `X-Router-Max-Cost` is per request; a *job* cares about the total.
3. **Decision capture** — surface `X-Router-Model` / `-Reason` / `-Warning` / `-Cost-Usd` to
   the caller, otherwise buried because Semantic Kernel owns the call.
4. **Run aggregation** — one structured summary per job (model distribution, spend,
   degraded-classifier count) instead of N scattered log lines.

## Not a wrapper

Semantic Kernel accepts an `OpenAIClient` and calls it internally. Anything that *isn't* one
can't be injected — so wrapping it would collapse the library's usefulness to code you wrote by
hand. This client doesn't wrap, subclass, or reimplement it. It **configures a genuine
`OpenAIClient`**: points its endpoint at the gateway and adds a `System.ClientModel` **pipeline
policy** that injects the `X-Router-*` request headers and captures the response headers. The
policy is the entire interception mechanism, and it's a supported extension point — so there is
no coupling to SDK internals to break on the next release.

The primary API is an **ambient scope**, because SK resolves one client at construction and
never lets you hand a different one to a built `Kernel`:

```csharp
using var scope = router.BeginScope(new RouterScopeOptions {
    Strategy = RouterStrategy.CheapestCapable,   // → X-Router-Strategy: cheapest-capable
    Budget   = 5.00m
});

var answer   = await kernel.InvokePromptAsync("...");   // routed per the scope
var decision = scope.LastDecision;                      // model, reason, costUsd, decisionId
```

> **`RouterStrategy`** mirrors the gateway's strategy taxonomy
> ([ADR 0021](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0021-strategy-taxonomy.md)):
> `Best`, `CheapestCapable`, `Cheapest`, `FastestCapable`, `Fastest` — each maps 1:1 to the
> `X-Router-Strategy` header value. The gateway also accepts `value`/`fast` as **deprecated
> aliases** of `cheapest-capable`/`fastest-capable`.

> **The `"auto"` convention.** SK requires a model id and sends it in the body; the gateway
> reads **`"auto"`** as *"you decide."* Register the chat completion with `modelId: "auto"` —
> it's the one thing an SK user will otherwise trip on.

---

## Packages

| Package | What it is |
|---|---|
| `CorgiAI.Client` | Core: options, pipeline policy, ambient scopes, decisions, budget. **No** Semantic Kernel dependency — the core outlives an SK major version. |
| `CorgiAI.Client.SemanticKernel` | Kernel-builder extensions and function-invocation filters. |
| `CorgiAI.Client.Extensions.DependencyInjection` | `AddCorgiClient(...)`, keyed registration, `IOptions`. |

Targets `net8.0` and `net9.0`.

## Build & test

```bash
dotnet build
dotnet test          # add -f net8.0 if only one runtime is installed locally
```

CI builds and tests both frameworks and collects coverage on every push and PR.

## Security & supply chain

Measured, not assumed — the same posture as the gateway:

| Concern | Tool | Where |
|---|---|---|
| **SAST** — static analysis | CodeQL (`csharp`, security-and-quality) | `codeql.yml` → Code Scanning |
| **Secret scanning** | gitleaks (full history) + GitHub secret scanning & push protection | `security.yml` + repo settings |
| **Dependency scanning** | Dependabot — NuGet + Actions, with security updates | `dependabot.yml` |
| **Coverage** | coverlet + ReportGenerator (CI summary + artifact) | `ci.yml` |

GitHub Actions are **SHA-pinned**; Dependabot keeps the pins current.

**There is no DAST, on purpose.** Dynamic testing exercises a *running* service, and a client
library has no endpoint of its own to test. DAST belongs to the gateway
([`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway)), which runs ZAP and
Schemathesis against the live proxy. Claiming it here would be theatre.

---

## Part of a stack

This is the .NET client. The gateway it talks to is
**[`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway)** — a drop-in
OpenAI-compatible gateway that picks the best model per request.

The two read as one stack but ship separately, on purpose. Per
[ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md),
each language client is its own repository on its native registry — bound to the gateway by a
**versioned header contract**, never by co-bundled binaries. TypeScript and Python clients will
follow the same pattern. Four small gateway-side headers are the contract this client depends on
(`X-Router-Pin`, `-Cost-Usd`, `-Decision-Id`; `/v1/router/explain` already ships) — tracked in
that ADR.

## License

MIT — see [LICENSE](LICENSE).
