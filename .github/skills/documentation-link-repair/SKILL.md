---

name: documentation-link-repair

description: Detect and repair broken local markdown links after content edits, moves, and renames.

title: Documentation Link Repair Skill

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: low

estimated_tokens: 500

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-file-placement

  - documentation-frontmatter

  - documentation-governance

  - documentation-maintenance

appliesTo: '**/*.md'

tags:

  - documentation

  - links

  - ste

---

# Documentation Link Repair Skill



Agent repairs local markdown links after documentation changes.



## Link Scope



| Link Type | Agent Check |

|---|---|

| Relative file links | Agent resolves each target from the source file location. |

| Heading anchors | Agent confirms the destination heading still exists. |

| Cross-folder references | Agent updates links affected by moves or renames. |



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Gather | Agent lists changed markdown files and moved paths. | Repair set |

| 2. Resolve | Agent resolves each local link target from the source path. | Resolution results |

| 3. Repair | Agent updates links that point to missing files or headings. | Corrected links |

| 4. Recheck | Agent reviews parent indexes and related guides. | Verified navigation |



## Repair Rules



Agent prefers relative links over machine-specific paths.

Agent keeps link text meaningful and stable.

Agent updates anchor references when headings change.

Agent leaves external links unchanged unless clear breakage evidence exists.



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| File targets | Resolve every changed local file link. | Every relative link target exists. |

| Anchor targets | Compare links to heading anchors in target files. | Every changed anchor link resolves. |

| Navigation review | Review parent indexes and nearby guides after moves. | No changed navigation entry points point to missing content. |

