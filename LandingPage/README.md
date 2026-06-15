# React + TypeScript + Vite

This project is a React landing page built with Vite and TypeScript, using [Biome](https://biomejs.dev/) for linting and formatting.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Linting & Formatting

This project uses [Biome](https://biomejs.dev/) instead of ESLint/Prettier. Configuration is in `biome.json`.

```sh
# Lint source files
npm run lint

# Check formatting and lint
npm run check

# Auto-fix issues
npx biome check --fix ./src
```
