# SDK notes

> **Required reading before Phase 1.** The build plan (ADR 0014, §0) mandates verifying the
> current APIs of the OpenAI .NET SDK and the Semantic Kernel OpenAI connector *before*
> writing code — both moved significantly during the `Azure.AI.OpenAI` → `OpenAI` +
> `System.ClientModel` transition, so type and method names must be **confirmed, not assumed.**
> This file is where those findings are recorded. It is intentionally empty until then.

## To confirm

- [ ] `OpenAI` (official .NET SDK, v2.x): the exact type used to construct a client
      (`OpenAIClient`?), how `OpenAIClientOptions.Endpoint` is set, and how a custom
      `System.ClientModel` `PipelinePolicy` is added to the pipeline.
- [ ] `PipelinePolicy` API surface: request-header injection and response-header capture
      points; where in the pipeline the policy must sit to keep retry/telemetry context.
- [ ] `Microsoft.SemanticKernel.Connectors.OpenAI`: the signature of
      `AddOpenAIChatCompletion(modelId, openAIClient)` and whether it still accepts a
      pre-built client instance.
- [ ] Streaming: confirm SSE response headers are available at response-start (before the
      body) so a decision can be captured before the stream completes.
- [ ] Pinned package versions to target, recorded here once chosen.

## Findings

_(none yet)_
