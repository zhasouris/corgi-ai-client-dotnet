# ADR 0001 — Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-07-23

## Context

This client's two load-bearing decisions — configuring a genuine `OpenAIClient` via a
pipeline policy (never subclassing it), and ambient `AsyncLocal` scopes as the primary API —
demonstrate design judgment that a reviewer will scrutinise. They deserve to be written down
with their reasoning, not just implemented. The sibling `corgi-ai-gateway` repository keeps a
numbered ADR trail; this repository follows the same discipline.

## Decision

Record every significant decision as a numbered ADR in `docs/adr/`, each with **Context**,
**Decision**, and **Consequences**.

The two design ADRs the build plan calls out — *no subclassing of `OpenAIClient`* and *ambient
scopes over derived clients* — are written **before** their implementation, seeded from the
authoritative discussion in the router's
[ADR 0014](https://github.com/zhasouris/corgi-ai-gateway/blob/main/docs/decisions/0014-dotnet-client-and-prerequisites.md).

## Consequences

- A future contributor can see *why* the client is a real `OpenAIClient` and not a wrapper —
  the single most important thing to not get wrong.
- ADR numbering here is independent of the router repo's `docs/decisions/` sequence.
