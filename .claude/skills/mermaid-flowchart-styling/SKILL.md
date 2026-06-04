---
name: mermaid-flowchart-styling
description: Professional Mermaid flowchart styling with advanced themes, spacing, and visual consistency. Use when creating business process flows, technical diagrams, or any flowchart requiring clean layouts, consistent node styling, and professional appearance.
---

# Mermaid Flowchart Professional Styling

Advanced styling techniques for creating clean, professional flowcharts with consistent visual appearance.

## When to Apply

- Creating business process flows with professional appearance
- Technical diagrams requiring consistent node styling
- Flowcharts needing precise spacing and layout control
- Converting hand-drawn processes to polished digital diagrams

## Critical Rules

**Use classDef for Consistency**: Define reusable style classes instead of individual node styling for scalable, maintainable diagrams.

```mermaid
// WRONG - Individual styling for each node
flowchart TD
A[Process] --> B[Decision]
style A fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
style B fill:#fff3e0,stroke:#ef6c00,stroke-width:2px

// RIGHT - Reusable classes
flowchart TD
A[Process] --> B[Decision]
classDef processNode fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
classDef decisionNode fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
class A processNode
class B decisionNode
```

**Control Spacing with Config**: Use frontmatter configuration to control layout spacing rather than adjusting diagram structure.

```mermaid
---
config:
  flowchart:
    nodeSpacing: 50
    rankSpacing: 50
---
flowchart TD
A --> B --> C
```

**Theme Variables for Professional Colors**: Use base theme with custom themeVariables for consistent professional color schemes.

```mermaid
---
config:
  theme: base
  themeVariables:
    primaryColor: '#2563eb'
    primaryTextColor: '#ffffff'
    primaryBorderColor: '#1d4ed8'
    lineColor: '#64748b'
    secondaryColor: '#f8fafc'
    tertiaryColor: '#e2e8f0'
---
flowchart TD
A[Start] --> B{Decision}
```

## Key Patterns

### Professional Color Scheme

```mermaid
---
config:
  theme: base
  themeVariables:
    primaryColor: '#1e40af'
    primaryTextColor: '#ffffff'
    primaryBorderColor: '#1e3a8a'
    lineColor: '#6b7280'
    secondaryColor: '#f3f4f6'
    tertiaryColor: '#e5e7eb'
---
flowchart TD
A[Input] --> B{Validate}
B -->|Valid| C[Process]
B -->|Invalid| D[Error]
```

### Consistent Node Classes

```mermaid
flowchart TD
A[Start] --> B{Check Status}
B -->|Active| C[Process Data]
B -->|Inactive| D[Skip]
C --> E[Save Results]
D --> E

classDef startEnd fill:#10b981,stroke:#047857,stroke-width:3px,color:#ffffff
classDef process fill:#3b82f6,stroke:#1d4ed8,stroke-width:2px,color:#ffffff
classDef decision fill:#f59e0b,stroke:#d97706,stroke-width:2px,color:#ffffff

class A,E startEnd
class C,D process
class B decision
```

### Controlled Spacing Layout

```mermaid
---
config:
  flowchart:
    nodeSpacing: 60
    rankSpacing: 80
    curve: basis
---
flowchart TD
subgraph main[Main Process]
  A[Initialize] --> B{Ready?}
  B -->|Yes| C[Execute]
  B -->|No| A
  C --> D[Complete]
end

style main fill:#f8fafc,stroke:#cbd5e1,stroke-width:2px
```

### Link Styling for Emphasis

```mermaid
flowchart TD
A[Request] --> B{Validate}
B -->|Success| C[Process]
B -->|Error| D[Handle Error]
C --> E[Response]
D --> F[Error Response]

linkStyle 0 stroke:#64748b,stroke-width:2px
linkStyle 1 stroke:#10b981,stroke-width:3px
linkStyle 2 stroke:#ef4444,stroke-width:3px
linkStyle 3,4 stroke:#64748b,stroke-width:2px
```

## Common Mistakes

- **Individual node styling** — Use classDef and class assignments for consistency across large diagrams
- **Default spacing** — Configure nodeSpacing and rankSpacing to prevent cramped layouts
- **Random colors** — Use theme-based color schemes with themeVariables for professional appearance
- **Missing subgraph organization** — Group related processes in styled subgraphs for better hierarchy
- **Inconsistent link styling** — Apply linkStyle systematically to create visual flow hierarchy