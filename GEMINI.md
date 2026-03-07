# GEMINI.md - CITS Project Guidelines

## Project Overview

This is a full-stack monorepo project comprising a modern Vue.js frontend and a .NET-based backend.

- **frontend-pc**: A high-performance admin dashboard based on **Vue Vben Admin (v5.5.9)**.
  - **Tech Stack**: Vue 3, Vite, TypeScript, Turbo, pnpm, Vitest, Playwright.
  - **Architecture**: Monorepo structure with multiple apps (Ant Design, Element Plus, Naive UI, TDesign) and shared internal packages.
- **backend-api**: A robust backend service built with **.NET 10**.
  - **Tech Stack**: .NET 10, Cits Framework (custom), PostgreSQL, FreeSql ORM, FreeRedis, Serilog.
  - **Architecture**: Modular design with a clear separation between Domain, Application, and API layers.

---

## Getting Started

### Prerequisites
- **Node.js**: >= 20.12.0
- **pnpm**: >= 10.0.0
- **.NET SDK**: 10.0
- **Database**: PostgreSQL (for backend)
- **Cache**: Redis (for backend)

### Frontend (frontend-pc)
1. Navigate to `frontend-pc/`.
2. Install dependencies: `pnpm install`.
3. Start development server:
   - All apps: `pnpm dev`
   - Ant Design version: `pnpm dev:antd`
   - Naive UI version: `pnpm dev:naive`
4. Build for production: `pnpm build`.

### Backend (backend-api)
1. Open `backend-api/CITS.slnx` in Visual Studio 2022 or JetBrains Rider.
2. Ensure PostgreSQL and Redis are running (configure in `appsettings.json`).
3. Run the `MyApi.HttpApi` project.

---

## Development Conventions

### General
- **Git Commits**: Use Angular convention: `<type>(<scope>): <subject>`
  - Types: `feat`, `fix`, `style`, `perf`, `refactor`, `revert`, `test`, `docs`, `chore`, `ci`, `types`.
  - Example: `feat(auth): add login page`.

### Frontend Guidelines
- **Naming**:
  - Files/Folders: `kebab-case` (e.g., `user-profile.vue`, `use-auth.ts`).
  - Components: `PascalCase` (e.g., `<UserProfile />`).
  - Interfaces: `IPascalCase` (e.g., `IUser`).
- **Composition API**: Always use `<script setup lang="ts">`.
- **Styling**: Prefer Vanilla CSS (via Tailwind) or the respective UI library's tokens.

### Backend Guidelines
- **Naming**:
  - Namespaces: File-scoped (e.g., `namespace MyApi.HttpApi.Controllers;`).
  - Classes/Methods/Properties: `PascalCase`.
  - Local Variables/Parameters: `camelCase`.
- **Patterns**:
  - Use `var` for implicit typing when the type is obvious.
  - Enable and respect Nullable Reference Types.
  - Use `Record` types for DTOs when applicable.
- **Logging**: Inject `ILogger<T>` and use Serilog for structured logging.

---

## Key Directories

- `frontend-pc/apps/`: Implementation of different UI versions of the admin panel.
- `frontend-pc/packages/`: Shared logic (stores, utils, core components).
- `backend-api/src/`: Core application modules (API, Application, Domain).
- `backend-api/framework/`: Reusable framework components (Base, Identity, WebApi).

Refer to `AGENTS.md` for more detailed technical specifications and coding standards.
