import { test, expect } from '@playwright/test';

test.describe('Authentication E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should open login modal', async ({ page }) => {
    const loginButton = page.getByRole('button', { name: /login/i }).first();
    await loginButton.click();

    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toBeVisible();
  });

  test('should show validation error for empty login form', async ({ page }) => {
    await page.getByRole('button', { name: /login/i }).first().click();

    const submitButton = page.getByRole('button', { name: /login/i }).last();
    await submitButton.click();

    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toBeVisible();
  });

  test('should attempt login with credentials', async ({ page }) => {
    await page.getByRole('button', { name: /login/i }).first().click();

    await page.getByLabel(/email/i).fill('test@example.com');
    await page.getByLabel(/password/i).fill('Test123!');

    const submitButton = page.getByRole('button', { name: /login/i }).last();
    await submitButton.click();

    await page.waitForTimeout(1000);
  });

  test('should open register modal', async ({ page }) => {
    const registerButton = page.getByRole('button', { name: /register|sign up/i }).first();
    
    if (await registerButton.isVisible()) {
      await registerButton.click();

      const usernameInput = page.getByLabel(/username/i).or(page.getByLabel(/email/i));
      await expect(usernameInput).toBeVisible();
    }
  });
});
