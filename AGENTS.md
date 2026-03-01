# AGENTS.md - Agentic Coding Guidelines

## Project Overview

This repository contains two main projects:
- **frontend-pc**: Vue Vben Admin monorepo (v5.5.9) - pnpm >=10.0.0, Node >=20.12.0, Turbo + Vite, Vitest + Playwright
- **backend-api**: .NET 10 Cits Framework backend (custom framework)

---

## Frontend (frontend-pc)

### Build Commands

```bash
pnpm build                    # Build all packages
pnpm build:antd              # Build Ant Design app
pnpm build:ele                # Build Element Plus app
pnpm build:naive              # Build Naive UI app
pnpm build:tdesign            # Build TDesign app
pnpm dev                      # Start all dev servers
pnpm dev:antd                # Start specific app dev server
pnpm dev:naive               # Start Naive UI app
pnpm dev:ele                 # Start Element Plus app
pnpm dev:tdesign             # Start TDesign app
pnpm preview                  # Preview production build
pnpm build:play              # Build playground
```

### Lint & Format

```bash
pnpm lint                     # Run all linting (ESLint, Stylelint, Prettier)
pnpm format                   # Format code with Prettier
pnpm eslint .                 # Run ESLint directly
pnpm stylelint "**/*.{vue,css,less.scss}"
pnpm prettier .               # Run Prettier
pnpm check                    # Full check (circular deps, deps, types, spelling)
pnpm check:type              # Type check only
```

### Testing

```bash
pnpm test:unit                # Run all unit tests (Vitest)
pnpm test:unit --watch       # Watch mode
pnpm vitest run path/to/test.test.ts    # Run single test file
pnpm vitest run -t "pattern"             # Run tests matching pattern
pnpm test:e2e                 # Run e2e tests
```

### Code Style (Frontend)

#### TypeScript (strict)
- `strict: true`, `strictNullChecks: true`, `noImplicitAny: true`
- `noUnusedLocals: true`, `noUnusedParameters: true`
- `verbatimModuleSyntax: true`, `noUncheckedIndexedAccess: true`

#### Prettier
- Single quotes, trailing commas, semicolons required
- 80 character line width, endOfLine: auto

#### Naming Conventions
- Files: kebab-case (`my-component.ts`, `use-auth.ts`)
- Components: PascalCase (`UserProfile.vue`)
- Variables/Functions: camelCase
- Constants: SCREAMING_SNAKE_CASE
- Interfaces: PascalCase with `I` prefix (e.g., `IUser`)

#### Vue Components
- Use `<script setup lang="ts">` with `defineProps` and `withDefaults`
- Use `computed`, `ref`, `reactive` from 'vue'
- Avoid `any` - use `unknown` or proper types

---

## Backend (backend-api)

### Running the Project

Open `backend-api/CITS.slnx` in Visual Studio or Rider, then run `MyApi.HttpApi` project.

### Technology Stack
- .NET 10.0
- Cits Framework (custom framework)
- PostgreSQL database
- FreeSql ORM
- FreeRedis for caching

### Project Structure

```
backend-api/
├── src/
│   ├── MyApi.HttpApi/        # Web API layer (entry point)
│   ├── MyApi.Application/    # Application services
│   ├── MyApi.Application.Contracts/ # DTOs and interfaces
│   ├── MyApi.Domain/        # Domain entities
│   └── MyApi.Domain.Shared/ # Shared domain types
└── framework/               # Base framework modules
    ├── Cits.Base/           # Base utilities
    ├── Cits.WebApi/         # Web API base
    ├── Cits.Domain/         # Domain base
    └── Cits.Identity/       # Identity module
```

### Code Style (Backend)

#### C# Conventions
- File-scoped namespaces: `namespace MyApi.HttpApi.Controllers;`
- PascalCase for classes, methods, properties
- camelCase for local variables and parameters
- Use `var` for implicit typing when type is obvious
- Enable nullable reference types
- Use record types for DTOs where appropriate

#### Logging
- Use Serilog with file logging
- Inject `ILogger<T>` for class logging

---

## Git Commit

Angular format: `<type>(<scope>): <subject>`

Types: feat, fix, style, perf, refactor, revert, test, docs, chore, workflow, ci, types

Example: `feat(auth): add login page`

Run `pnpm commit` (frontend) for interactive helper.

---

## File Structure

```
cits-all/
├── frontend-pc/              # Vue frontend
│   ├── apps/                # web-antd, web-ele, web-naive, web-tdesign
│   ├── packages/             # effects, stores, utils, etc.
│   └── internal/             # configs
└── backend-api/              # .NET backend
    ├── src/                  # Application modules
    └── framework/            # Base framework
```
