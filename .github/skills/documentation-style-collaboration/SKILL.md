---

name: documentation-style-collaboration

description: Improve documentation wording with collaborative, clear, and direct language.

title: Documentation Style Collaboration Skill

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: low

estimated_tokens: 420

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

  - frontmatter-standard

related_skills:

  - documentation-file-placement

  - documentation-frontmatter

  - documentation-governance

  - documentation-link-repair

appliesTo: '**/*.md'

tags:

  - documentation

  - style

  - ste

---

# Documentation Style Collaboration Skill



Agent improves documentation tone without changing technical meaning.



## Style Goals



Agent uses clear and direct wording.

Agent uses collaborative phrasing instead of command-heavy prose.

Agent keeps rationale visible when rationale reduces ambiguity.

Agent keeps sentences concise and concrete.



## Workflow



| Step | Agent Action | Output |

|---|---|---|

| 1. Identify | Agent finds imperative-heavy or unclear sections. | Rewrite targets |

| 2. Rephrase | Agent adds explicit subjects and collaborative wording. | Revised prose |

| 3. Preserve | Agent keeps technical constraints and required facts unchanged. | Accurate content |

| 4. Review | Agent checks edited sections for tone and clarity. | Style confirmation |



## Preferred Patterns



| Use | Avoid |

|---|---|

| `Agent updates...` | Bare imperative lines with no subject |

| `Developers use...` | Vague actor-free instructions |

| `This step keeps...` | Unexplained procedure blocks |



## Quality Gate



| Check | Test | Pass Criteria |

|---|---|---|

| Explicit subjects | Review edited sentences. | Every edited sentence names a subject. |

| Collaborative tone | Review edited sections for command-heavy phrasing. | Edited sections use clear, non-commanding language. |

| Technical fidelity | Compare revised text to source requirements. | Revised wording preserves the original technical meaning. |

