import { test, expect } from '@playwright/test';

test.describe('Homepage E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should load homepage successfully', async ({ page }) => {
    await expect(page).toHaveTitle(/SavePoint/i);
  });

  test('should display navigation bar', async ({ page }) => {
    const navbar = page.locator('nav');
    await expect(navbar).toBeVisible();
  });

  test('should show popular games section', async ({ page }) => {
    const popularSection = page.getByText(/popular/i).first();
    await expect(popularSection).toBeVisible();
  });

  test('should navigate to video games page', async ({ page }) => {
    await page.getByRole('link', { name: /games/i }).click();
    await expect(page).toHaveURL(/.*videogames/);
  });
});
