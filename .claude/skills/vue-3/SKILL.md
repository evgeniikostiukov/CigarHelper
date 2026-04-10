---
name: vue-3
description: >-
  Vue 3 application development with Composition API, script setup, TypeScript,
  reactivity, components, Pinia, Vue Router, and Vite. Use when building or
  refactoring Vue 3 SFCs, SPA routing, state, forms, or when the user mentions
  Vue 3, Vite + Vue, Composition API, Pinia, or Vue Router.
---

# Vue 3

## Defaults

- Prefer **Composition API** and **`<script setup lang="ts">`** in `.vue` files.
- Use **Vue 3.3+** features where applicable (`defineModel`, `defineOptions` if the toolchain supports them).
- Match the project stack: if the repo uses Pinia, Router, PrimeVue, Tailwind — follow existing imports and folder layout (`docs/memory-bank/frontend/` when in CigarHelper).

## Single-file components

- Order blocks: `<script setup>` → `<template>` → `<style scoped>` (or unscoped when global tokens are intentional).
- **Props**: `defineProps` with TypeScript interface or `withDefaults` for optional props; document `required` vs optional clearly.
- **Emits**: `defineEmits<{ ... }>()` with typed payload shapes; emit kebab-case event names in template, camelCase in script is fine for `defineEmits`.
- **Expose**: `defineExpose` only when parent must call child methods (prefer props/events otherwise).
- **Slots**: named slots + `v-slot` / `#` shorthand; provide fallbacks where UX needs them.

## Reactivity

- **Primitives / single values**: `ref`; unwrap in script with `.value`, in template auto-unwrapped.
- **Objects**: `reactive` for local structured state; avoid spreading reactive into plain objects without care (loss of reactivity).
- **Derived state**: `computed` with explicit return type when non-obvious.
- **Side effects**: `watch` / `watchEffect`; prefer `watch` with explicit sources when dependencies must be clear; flush `post` when DOM reads after update are required.
- **Large / external data**: `shallowRef` / `shallowReactive` when deep reactivity is costly and not needed.
- **Cleanup**: `onBeforeUnmount` / `onUnmounted` for timers, listeners, subscriptions.

## Async and data loading

- Use **Suspense** only where the app already adopts it; otherwise load in `onMounted` + local `loading` / `error` refs (or a small composable).
- Avoid unhandled promise rejections in handlers; align with existing API client patterns in the project.

## Pinia

- One store per domain slice; **actions** for async and mutations of state; **getters** for derived data.
- Do not mutate state outside store actions/getters when the project follows that convention.
- Prefer `storeToRefs` when destructuring reactive store state in components.

## Vue Router

- Use **navigation guards** (`beforeEach`, etc.) consistently with existing auth flow.
- **Lazy routes**: `() => import('...')` for code splitting on large views.
- Typed routes: follow project’s approach (`RouteRecordRaw`, meta types) if already present.

## Performance

- **`v-once`** for static subtrees; **`v-memo`** for list cells when parent re-renders often and cell props are stable.
- Stable **`key`** on `v-for` (never index as key when list order/mutations change identity).
- Heavy lists: virtualize if the codebase already uses a pattern; otherwise avoid rendering huge DOM without pagination.

## TypeScript

- Type **props**, **emits**, and **composable** return values explicitly when inference is weak.
- For **template** type checking, rely on project `vue-tsc` / Volar settings; fix component generic edge cases per project config.

## Templates

- Prefer **`v-bind` shorthand** and consistent attribute order (project ESLint if any).
- **`v-if` vs `v-show`**: `v-if` for toggling cost; `v-show` for frequent CSS toggles.
- Avoid complex logic in templates; move to `computed` or functions in `<script setup>`.

## Style

- **`scoped`** by default; `:deep()` for piercing when wrapping third-party components.
- If Tailwind / PrimeVue / CSS variables are in use, follow existing design tokens and utility patterns (`frontend-design` / `ui-ux-pro-max` skills for visual work).

## Testing (when relevant)

- Component tests: mount with Vue Test Utils or project choice; assert behavior, not implementation details.
- E2E: align with Playwright/Cypress conventions in the repo.

## Anti-patterns

- Mutating props or `props` object directly.
- Relying on `reactive` for values that get reassigned (use `ref`).
- Giant monolithic `.vue` files: extract composables (`useXxx`) and presentational child components.
- Mixing Options API and Composition API in one component without a strong reason.

## Progressive detail

- For library-specific APIs (VueUse, PrimeVue, etc.), read project dependencies and existing components before inventing new patterns.
