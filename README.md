# 🐕 CorgiAI.Client (.NET)

**Routing control for [`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway) —
first-class in Semantic Kernel, without replacing the OpenAI client.**

[![ci](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/ci.yml)
[![codeql](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/codeql.yml/badge.svg)](https://github.com/zhasouris/corgi-ai-client-dotnet/actions/workflows/codeql.yml)
![status](https://img.shields.io/badge/status-scaffold-orange)
![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-512bd4)
![license](https://img.shields.io/badge/license-MIT-blue)

> **Status: scaffold.** The solution builds and CI is green, but the client is not implemented
> yet. It is the home for the .NET client specified in the gateway's
> [ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md)
> — that ADR and the build plan it carries are authoritative. Progress tracks against
> [docs/build-plan.md](docs/build-plan.md).

---

## The premise

`corgi-ai-gateway` is deliberately **stateless** — it classifies and routes each request on
its own. But batch runs, scheduled jobs, and agent loops are stateful *across* requests, and
that state has nowhere to live in a stateless proxy. **This client is where it lives.** That —
not header ergonomics — is its entire justification. It provides four things the gateway
cannot:

1. **Pinning** — decide once for a homogeneous batch, then execute N times without re-paying
   classification latency/spend, and without breaking provider prompt caching by switching
   models mid-run. Backed by a **homogeneity check** so pinning is only applied where it is
   provably safe.
2. **Run-level budget** — cumulative spend across a whole run, with abort or
   strategy-downshift on approach. (`X-Router-Max-Cost` is per-request; jobs care about the
   total.)
3. **Decision capture** — surface `X-Router-Model` / `-Reason` / `-Warning` / `-Cost-Usd` to
   the caller, which is otherwise buried because Semantic Kernel owns the call.
4. **Run aggregation** — one structured summary per job (model distribution, spend,
   degraded-classifier count) instead of N log lines.

## It is *not* a wrapper

Semantic Kernel accepts an `OpenAIClient` and calls it internally. So this library does **not**
subclass, proxy, or reimplement it — it **configures a genuine `OpenAIClient`** by pointing its
endpoint at the gateway and adding a `System.ClientModel` **pipeline policy** that injects the
`X-Router-*` request headers and captures the response headers. Because the result is a *real*
`OpenAIClient`, it drops into `AddOpenAIChatCompletion(modelId, client)` and any third-party
library that accepts one — with zero compatibility surface of our own.

The primary API is an **ambient scope** (`AsyncLocal`), because SK resolves one client at
construction and owns it for the process lifetime:

```csharp
using var scope = router.BeginScope(new RouterScopeOptions {
    Strategy = RouterStrategy.Cost,
    Budget = 5.00m
});
var answer = await kernel.InvokePromptAsync("...");   // routed per the scope
var decision = scope.LastDecision;                    // model, reason, costUsd, decisionId
```

> **The `"auto"` convention.** SK requires a model id and sends it in the body; the gateway
> treats **`"auto"`** as *"you decide"*. Register the chat completion with `modelId: "auto"`.

## Packages (planned)

| Package | Purpose |
|---|---|
| `CorgiAI.Client` | Core: options, pipeline policy, ambient scopes, decisions, budget. **No** Semantic Kernel dependency. |
| `CorgiAI.Client.SemanticKernel` | Kernel-builder extensions and filters. |
| `CorgiAI.Client.Extensions.DependencyInjection` | `AddCorgiClient(...)`, keyed registration, `IOptions`. |

Targets `net8.0` and `net9.0`.

## Build & test

```bash
dotnet build
dotnet test          # add -f net8.0 if only one runtime is installed locally
```

CI (`ci.yml`) builds and tests both target frameworks and collects code coverage on every
push and PR.

## Security & supply chain

| Concern | Tool | Where |
|---|---|---|
| **SAST** (static analysis) | CodeQL (`csharp`, security-and-quality) | `codeql.yml` → Code Scanning tab |
| **Secret scanning** | gitleaks (full history) + GitHub secret scanning & push protection | `security.yml` + repo settings |
| **Dependency scanning** | Dependabot (NuGet + Actions), security updates | `dependabot.yml` |
| **Code coverage** | coverlet + ReportGenerator (CI summary + artifact) | `ci.yml` |

GitHub Actions are **SHA-pinned**; Dependabot updates the pins.

**Why there is no DAST.** Dynamic Application Security Testing exercises a *running* service.
This is a client library with no endpoint of its own, so DAST does not apply here — it belongs
to the gateway, [`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway), which runs
ZAP and Schemathesis against the live proxy.

## Part of a stack

This is the .NET client. The gateway (server) it talks to is
**[`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway)** — a drop-in
OpenAI-compatible gateway that picks the best model per request.

Per [ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md),
each language client is its own repository on its native registry (NuGet here), bound to the
gateway by a **versioned header contract** — not by co-bundled binaries. TypeScript and Python
clients will follow the same pattern. Four small gateway-side changes (the `X-Router-Pin`,
`-Cost-Usd`, and `-Decision-Id` headers; `/v1/router/explain` is already shipped) are tracked
in that ADR.

## License

MIT — see [LICENSE](LICENSE).
