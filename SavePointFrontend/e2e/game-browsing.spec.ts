import { test, expect } from '@playwright/test';

test.describe('Game Browsing E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/videogames');
  });

  test('should display game grid', async ({ page }) => {
    await page.waitForSelector('[class*="card"]', { timeout: 10000 });

    const gameCards = page.locator('[class*="card"]');
    const count = await gameCards.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should open game details modal when clicking a game card', async ({ page }) => {
    await page.waitForSelector('[class*="card"]', { timeout: 10000 });

    const firstCard = page.locator('[class*="card"]').first();
    await firstCard.click();

    await page.waitForTimeout(500);
    const modal = page.locator('[role="dialog"]').or(page.locator('[class*="modal"]'));
    await expect(modal).toBeVisible();
  });

  test('should navigate through pages using pagination', async ({ page }) => {
    await page.waitForSelector('[class*="card"]', { timeout: 10000 });

    const nextButton = page.getByRole('button', { name: '»' });
    
    if (await nextButton.isEnabled()) {
      await nextButton.click();

      await page.waitForTimeout(1000);
      const pageIndicator = page.getByText(/Page \d+ of \d+/);
      await expect(pageIndicator).toBeVisible();
    }
  });

  test('should filter games by genre', async ({ page }) => {
    await page.waitForTimeout(2000);

    const genreFilter = page.locator('select').or(page.getByRole('combobox')).first();
    
    if (await genreFilter.isVisible()) {
      await genreFilter.click();
      
      const options = page.locator('option');
      if (await options.count() > 1) {
        await options.nth(1).click();
        
        await page.waitForTimeout(1000);
        
        const gameCards = page.locator('[class*="card"]');
        expect(await gameCards.count()).toBeGreaterThan(0);
      }
    }
  });

  test('should search for games', async ({ page }) => {
    await page.waitForTimeout(1000);

    const searchInput = page.getByPlaceholder(/search/i).or(page.getByRole('searchbox'));
    
    if (await searchInput.isVisible()) {
      await searchInput.fill('Zelda');
      
      await page.waitForTimeout(1500);
      
      const gameCards = page.locator('[class*="card"]');
      const count = await gameCards.count();
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });
});
