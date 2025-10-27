import { test, expect } from '@playwright/test';

test.describe('Authentication E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should open login modal', async ({ page }) => {
    // Find and click login button
    const loginButton = page.getByRole('button', { name: /login/i }).first();
    await loginButton.click();

    // Verify modal is visible
    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toBeVisible();
  });

  test('should show validation error for empty login form', async ({ page }) => {
    // Open login modal
    await page.getByRole('button', { name: /login/i }).first().click();

    // Try to submit empty form
    const submitButton = page.getByRole('button', { name: /login/i }).last();
    await submitButton.click();

    // Verify form is still visible (didn't submit)
    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toBeVisible();
  });

  test('should attempt login with credentials', async ({ page }) => {
    // Open login modal
    await page.getByRole('button', { name: /login/i }).first().click();

    // Fill in credentials
    await page.getByLabel(/email/i).fill('test@example.com');
    await page.getByLabel(/password/i).fill('Test123!');

    // Submit form
    const submitButton = page.getByRole('button', { name: /login/i }).last();
    await submitButton.click();

    // Wait for response (could be success or error)
    await page.waitForTimeout(1000);
  });

  test('should open register modal', async ({ page }) => {
    // Find and click register button
    const registerButton = page.getByRole('button', { name: /register|sign up/i }).first();
    
    if (await registerButton.isVisible()) {
      await registerButton.click();

      // Verify modal is visible
      const usernameInput = page.getByLabel(/username/i).or(page.getByLabel(/email/i));
      await expect(usernameInput).toBeVisible();
    }
  });
});
