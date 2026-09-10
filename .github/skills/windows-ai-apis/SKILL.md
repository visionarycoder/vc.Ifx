---
name: windows-ai-apis
title: Windows AI APIs
description: Implement Windows AI APIs when work needs WinML, Windows ML, DirectML, ONNX Runtime, on-device inference, or NPU-aware acceleration flows.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium-high
estimated_tokens: 1820
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - platform-detection
  - dotnet-pinvoke
  - windows-ai-mcp-integration
appliesTo: '**/*.{cs,csproj,json,onnx,md}'
tags:
  - windows
  - ai
  - windows-ml
  - winml
  - directml
  - onnx-runtime
  - on-device
  - npu
---
# Windows AI APIs

Agent implements Windows AI workloads with Windows ML, WinML, DirectML, ONNX Runtime, and on-device inference paths that stay local to the host machine.

## When to Use

| Condition | Use |
|---|---|
| App needs local inference with no network dependency in the steady state | Use this skill |
| Work needs GPU- or NPU-accelerated ONNX execution on Windows hardware | Use this skill |
| Solution targets Windows-native APIs such as `Windows.AI.MachineLearning` or Windows ML-managed execution providers | Use this skill |
| Requirement includes privacy-sensitive or low-latency inferencing on device | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Work depends on hosted foundation models or managed search | Use `azure-ai-services` |
| Work focuses on Foundry Local prompt iteration and local model catalog use | Use `windows-ai-foundry-local` |
| Target platform excludes Windows | Use cross-platform ONNX or cloud guidance outside this skill |
| Requirement is large-scale distributed training | Use cloud ML guidance outside this skill |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Target Windows version and packaging model | Yes | WinUI, WPF, console, or service host boundary. |
| Model format and input schema | Yes | ONNX path, tensor shapes, and output contract. |
| Hardware target | Yes | CPU, GPU, NPU, or fallback order across them. |
| Latency or throughput target | Yes | Agent uses it for provider and batching choices. |
| Privacy boundary | No | Agent uses it for telemetry and data-retention decisions. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects Windows ML, WinML, ONNX Runtime, or hybrid interop path. | Review platform and packaging constraints. | One runtime path fits the host and model requirements. |
| 2 | Agent maps model inputs, preprocessing, and output contract. | Inspect schema and tensor bindings. | Every model tensor has one verified application mapping. |
| 3 | Agent selects execution provider and fallback order across NPU, GPU, and CPU. | Review hardware inventory and deployment target. | Provider order aligns with latency target and available hardware. |
| 4 | Agent wires local model loading, session reuse, and inference calls. | Inspect runtime initialization path. | Model session initializes once and serves repeated requests. |
| 5 | Agent verifies correctness, provider selection, and degraded-mode behavior. | Run inference tests on target hardware. | Outputs match expectations and fallback path stays functional. |

## Runtime Selection Matrix

| Pattern | Use | Avoid |
|---|---|---|
| Windows ML-managed providers | New Windows deployments that want OS-managed hardware selection, including NPU paths | Manual provider choice with no Windows capability check |
| WinML session | Windows app uses `Windows.AI.MachineLearning` APIs directly | Cloud-only inference for local-first workloads |
| ONNX Runtime plus DirectML | App needs fine-grained DirectX GPU control | CPU-only default on GPU-capable hardware |
| Session reuse | Repeated inference requests share one initialized model | Reloading the model per request |
| Explicit fallback order | NPU-first, GPU-second, CPU-last when hardware supports that order | Hidden provider changes with no traceability |

### Example: WinML Session and Binding

```csharp
var model = await LearningModel.LoadFromStorageFileAsync(modelFile);
var session = new LearningModelSession(
    model,
    new LearningModelDevice(LearningModelDeviceKind.DirectXHighPerformance));

var binding = new LearningModelBinding(session);
binding.Bind("input", imageFeatureValue);

var result = await session.EvaluateAsync(binding, "invoice-classification");
```

### Example: ONNX Runtime with DirectML

```csharp
var sessionOptions = new SessionOptions();
sessionOptions.AppendExecutionProvider_DML(0);

using var session = new InferenceSession("models\\classifier.onnx", sessionOptions);
using var inputs = new List<NamedOnnxValue>
{
    NamedOnnxValue.CreateFromTensor("input", inputTensor)
};

using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
```

## MCP Hooks

| Task | MCP Tool or Connector | Assistance |
|---|---|---|
| Locate model assets, feature flags, and packaging files in source | `github-mcp-server-search_code` | Finds ONNX paths, runtime setup code, and packaging manifests. |
| Inspect checked-in model metadata or runtime config | `github-mcp-server-get_file_contents` | Reads model manifests, JSON config, and host initialization files. |
| Validate packaging or release workflows | `github-mcp-server-actions_list` | Lists build and publish workflows for Windows AI artifacts. |
| Resolve Windows-local file input through an approved connector | Windows AI MCP file connector | Supplies local input documents or images without expanding direct file-system scope. |
| Route local inference through a connector-managed model surface | Windows AI MCP model connector | Connects runtime-discovered model tools to local inference workflows. |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Model load | Start host and initialize model session | Session initializes with no missing-asset fault |
| Provider selection | Inspect runtime provider on target device | Execution path uses intended NPU, GPU, or CPU provider |
| Inference correctness | Run fixed sample inputs | Outputs match expected labels, scores, or shapes |
| Session reuse | Execute repeated inferences in one process | Throughput improves and memory stays stable |
| Fallback path | Disable preferred accelerator and rerun | Alternate provider still returns valid output |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Model reload happens per request | Agent hoists model session creation into application startup or shared service scope. |
| Provider selection stays implicit | Agent sets an explicit provider order and logs the active choice. |
| Preprocessing logic drifts from training assumptions | Agent centralizes normalization and tensor-shape mapping. |
| Local inference writes sensitive payloads to logs | Agent limits telemetry to nonsensitive runtime metadata. |
| NPU path lacks fallback validation | Agent verifies GPU or CPU fallback before release. |
