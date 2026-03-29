// @ts-check

import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';
import prettierConfig from 'eslint-plugin-prettier/recommended';
import globals from 'globals';
import parserVue from 'vue-eslint-parser';

export default tseslint.config(
  eslint.configs.recommended,

  {
    // config with just ignores is the replacement for `.eslintignore`
    ignores: [
      '**/build/**',
      '**/dist/**',
      'node_modules/',
      '.prettierrc.json',
      'eslint.config.js',
      'src/vite-env.d.ts',
      'src/dotnet/**',
      'components.d.ts',
      'postcss.config.js',
      'vitest.config.ts',
    ],
  },
  {
    rules: {
      '@typescript-eslint/no-console': 'off',
      'no-debugger': 'warn',
      'no-unused-vars': 'off',
      '@typescript-eslint/no-useless-escape': 'off',
      '@typescript-eslint/no-unused-vars': [
        'warn',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_' },
      ],
      'no-unused-labels': 'warn',
      '@typescript-eslint/no-explicit-any': 'off',
      // '@typescript-eslint/no-implicit-any': 'warn',
    },
    files: ['**/*.ts'],
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: {
        project: true,
      },
      globals: {
        ...globals.node,
        ...globals.browser,
      },
    },
    plugins: {
      '@typescript-eslint': tseslint.plugin,
    },
  },
  prettierConfig,
  {
    files: ['**/*.vue'],
    languageOptions: {
      parser: parserVue,
      parserOptions: {
        parser: '@typescript-eslint/parser',
      },
      globals: {
        ...globals.node,
        ...globals.browser,
      },
    },
  },
  {
    // disable type-aware linting on JS files
    files: ['**/*.js'],
    ...tseslint.configs.disableTypeChecked,
  },
);
