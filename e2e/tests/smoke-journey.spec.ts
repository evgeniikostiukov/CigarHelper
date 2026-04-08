import { test, expect } from '@playwright/test';

/**
 * Полный smoke: аутентификация + ключевые экраны.
 * - По умолчанию: регистрация нового пользователя через UI (уникальный email/username).
 * - Опционально: `E2E_EMAIL` + `E2E_PASSWORD` — только вход (без регистрации).
 */
test.describe('Smoke journey', () => {
  test('регистрация или вход, хьюмидоры, форма, сигары, обзоры, база сигар', async ({
    page,
  }) => {
    test.setTimeout(120_000);

    const envEmail = process.env.E2E_EMAIL?.trim();
    const envPassword = process.env.E2E_PASSWORD?.trim();
    const suffix = `${Date.now()}_${Math.random().toString(36).slice(2, 8)}`;
    const username = `e2e_u_${suffix}`;
    const email = envEmail || `e2e_${suffix}@example.test`;
    const password = envPassword || 'E2e_test1';

    await page.goto('/login');
    await expect(page.getByTestId('login')).toBeVisible();

    if (!envEmail || !envPassword) {
      await page.getByTestId('login-toggle-mode').click();
      await expect(page.getByRole('heading', { name: 'Регистрация' })).toBeVisible();
      await page.getByTestId('login-username').fill(username);
      await page.getByTestId('login-email').fill(email);
      await page.getByTestId('login-password').locator('input').fill(password);
      await page.getByTestId('login-confirm-password').locator('input').fill(password);
    } else {
      await page.getByTestId('login-email').fill(email);
      await page.getByTestId('login-password').locator('input').fill(password);
    }

    await page.getByTestId('login-submit').click();
    await expect(page).not.toHaveURL(/\/login/, { timeout: 30_000 });
    await expect(page.getByTestId('app')).toBeVisible();

    await page.locator('.p-menubar').getByText('Сводка', { exact: true }).click();
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.getByTestId('dashboard')).toBeVisible({ timeout: 30_000 });
    await expect(page.getByTestId('dashboard-content')).toBeVisible({ timeout: 30_000 });

    await page.locator('.p-menubar').getByText('Хьюмидоры', { exact: true }).click();
    await expect(page).toHaveURL(/\/humidors$/);
    await expect(page.getByTestId('humidor-list')).toBeVisible({ timeout: 30_000 });

    await page.getByTestId('humidor-list-add').click();
    await expect(page).toHaveURL(/\/humidors\/new/);
    await expect(page.getByTestId('humidor-form')).toBeVisible({ timeout: 15_000 });
    await page.getByTestId('humidor-form-back').click();
    await expect(page.getByTestId('humidor-list')).toBeVisible({ timeout: 15_000 });

    await page.locator('.p-menubar').getByText('Мои сигары', { exact: true }).click();
    await expect(page).toHaveURL(/\/cigars$/);
    await expect(page.getByTestId('cigar-list')).toBeVisible({ timeout: 30_000 });

    await page.locator('.p-menubar').getByText('Обзоры', { exact: true }).click();
    await expect(page).toHaveURL(/\/reviews$/);
    await expect(page.getByTestId('review-list')).toBeVisible({ timeout: 30_000 });
    await expect(page.getByTestId('review-list-filters')).toBeVisible();

    await page.goto('/cigar-bases');
    await expect(page.getByTestId('cigar-bases')).toBeVisible({ timeout: 15_000 });
    await expect(
      page.locator(
        '[data-testid="cigar-bases-grid"], [data-testid="cigar-bases-empty"], [data-testid="cigar-bases-error"]',
      ),
    ).toBeVisible({ timeout: 45_000 });
  });
});
