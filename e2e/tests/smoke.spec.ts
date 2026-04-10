import { test, expect } from '@playwright/test';

test.describe('Smoke', () => {
  test('главная открывается и рендерит корневое приложение', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByTestId('app')).toBeVisible();
    await expect(page.getByTestId('app-header')).toBeVisible();
  });
});
