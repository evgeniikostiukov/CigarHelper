import { test, expect, type Page } from '@playwright/test';

/**
 * Очередь мутаций в Service Worker (202 + toast) проверяется только на **production preview**:
 * в `vite dev` PWA выключен (`devOptions.enabled: false`), контроллера SW нет.
 *
 * Запуск:
 * 1. API: `dotnet run --project CigarHelper.API/CigarHelper.Api.csproj` (порт 5184).
 * 2. Фронт: `cd CigarHelper.Web && npm run build && npm run preview` → http://localhost:4173
 * 3. `PLAYWRIGHT_BASE_URL=http://localhost:4173 npx playwright test tests/pwa-offline-queue.spec.ts`
 *
 * Либо явно: `E2E_PWA_OFFLINE=1` с любым `PLAYWRIGHT_BASE_URL` на preview-хосте.
 */
function shouldRunPwaOfflineE2E(): boolean {
  if (process.env.E2E_PWA_OFFLINE === '1') return true;
  const u = process.env.PLAYWRIGHT_BASE_URL ?? '';
  return /:(4173)(\/|$)/.test(u);
}

async function ensureServiceWorkerControlsPage(page: Page): Promise<void> {
  for (let attempt = 0; attempt < 4; attempt++) {
    const ok = await page.evaluate(() => !!navigator.serviceWorker?.controller);
    if (ok) return;
    await page.reload({ waitUntil: 'load' });
    await new Promise((r) => setTimeout(r, 500));
  }
  throw new Error(
    'Service Worker не захватил страницу после нескольких reload. Нужен `npm run build && npm run preview`, не `vite dev`.',
  );
}

test.describe('PWA offline mutation queue (preview)', () => {
  test.beforeEach(() => {
    test.skip(
      !shouldRunPwaOfflineE2E(),
      'Нужен preview с SW: PLAYWRIGHT_BASE_URL на порт 4173 или E2E_PWA_OFFLINE=1. См. комментарий в файле.',
    );
  });

  test('POST в очереди офлайн: toast «Действие в очереди»', async ({ page, context }) => {
    test.setTimeout(90_000);

    const envEmail = process.env.E2E_EMAIL?.trim();
    const envPassword = process.env.E2E_PASSWORD?.trim();
    const suffix = `${Date.now()}_${Math.random().toString(36).slice(2, 8)}`;
    const username = `e2e_pwa_${suffix}`;
    const email = envEmail || `e2e_pwa_${suffix}@example.test`;
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

    await ensureServiceWorkerControlsPage(page);

    await page.locator('.p-menubar').getByText('Хьюмидоры', { exact: true }).click();
    await expect(page).toHaveURL(/\/humidors$/);
    await page.getByTestId('humidor-list-add').click();
    await expect(page).toHaveURL(/\/humidors\/new/);
    await expect(page.getByTestId('humidor-form')).toBeVisible({ timeout: 15_000 });

    await page.getByTestId('humidor-form-name').fill(`Offline queue ${suffix}`);

    await context.setOffline(true);

    await page.getByTestId('humidor-form-submit').click();

    await expect(page.getByText('Действие в очереди')).toBeVisible({ timeout: 15_000 });
    await expect(page).toHaveURL(/\/humidors$/, { timeout: 15_000 });

    await context.setOffline(false);
  });
});
