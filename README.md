# CorgiAI.Client (.NET)

**Routing control for [`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway),
first-class in Semantic Kernel — without replacing the OpenAI client.**

> **Status: scaffold.** The solution builds and CI is green, but the client is not
> implemented yet. This repository is the home for the .NET client described in the router's
> [ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md);
> that ADR (and the build plan it carries) is the authoritative specification. Work proceeds
> in phases — see [docs/build-plan.md](docs/build-plan.md).

## Why this exists

The router is deliberately **stateless** — it classifies and routes each request on its own.
Batch runs, scheduled jobs, and agent loops are stateful *across* requests, and that state
has nowhere to live in a stateless proxy. **This client is where it lives**, providing four
things the proxy cannot:

1. **Pinning** — decide once for a homogeneous batch, execute N times without re-paying
   classification or breaking provider prompt caching.
2. **Run-level budget** — cumulative spend across a run, with abort or strategy-downshift.
3. **Decision capture** — surface `X-Router-Model` / `-Reason` / `-Warning` even when
   Semantic Kernel owns the call.
4. **Run aggregation** — one structured summary per job, not N log lines.

It is **not** a wrapper. It configures a *genuine* `OpenAIClient` via a `System.ClientModel`
pipeline policy, so it drops into `AddOpenAIChatCompletion(modelId, client)` and any library
that accepts one — no compatibility surface of its own.

## Packages (planned)

| Package | Purpose |
|---|---|
| `CorgiAI.Client` | Core: options, pipeline policy, ambient scopes, decisions, budget. No Semantic Kernel dependency. |
| `CorgiAI.Client.SemanticKernel` | Kernel-builder extensions and filters. |
| `CorgiAI.Client.Extensions.DependencyInjection` | `AddCorgiAI.Client`, keyed registration, `IOptions`. |

Targets `net8.0` and `net9.0`.

## Build

```bash
dotnet build
dotnet test          # add -f net8.0 if only one runtime is installed locally
```

## Part of a stack

This is the .NET client. The router (server) it talks to lives in
[`corgi-ai-gateway`](https://github.com/zhasouris/corgi-ai-gateway). Per
[ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md),
each language client is its own repository on its native registry (NuGet here), bound to the
server by a **versioned header contract**, not by co-bundled binaries. TypeScript and Python
clients will follow the same pattern.

## License

MIT — see [LICENSE](LICENSE).
