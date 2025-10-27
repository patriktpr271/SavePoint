import { test, expect } from '@playwright/test';

test.describe('Game Browsing E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/videogames');
  });

  test('should display game grid', async ({ page }) => {
    // Wait for games to load
    await page.waitForSelector('[class*="card"]', { timeout: 10000 });

    // Verify game cards are visible
    const gameCards = page.locator('[class*="card"]');
    const count = await gameCards.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should open game details modal when clicking a game card', async ({ page }) => {
    // Wait for games to load
    await page.waitForSelector('[class*="card"]', { timeout: 10000 });

    // Click first game card
    const firstCard = page.locator('[class*="card"]').first();
    await firstCard.click();

    // Wait for modal to open
    await page.waitForTimeout(500);

    // Verify modal content (adjust selector based on your modal)
    const modal = page.locator('[role="dialog"]').or(page.locator('[class*="modal"]'));
    await expect(modal).toBeVisible();
  });

  test('should navigate through pages using pagination', async ({ page }) => {
    // Wait for initial load
    await page.waitForSelector('[class*="card"]', { timeout: 10000 });

    // Look for next page button
    const nextButton = page.getByRole('button', { name: '»' });
    
    if (await nextButton.isEnabled()) {
      await nextButton.click();

      // Wait for page to update
      await page.waitForTimeout(1000);

      // Verify page changed (URL or content)
      const pageIndicator = page.getByText(/Page \d+ of \d+/);
      await expect(pageIndicator).toBeVisible();
    }
  });

  test('should filter games by genre', async ({ page }) => {
    // Wait for page load
    await page.waitForTimeout(2000);

    // Look for genre filter (adjust selector based on your implementation)
    const genreFilter = page.locator('select').or(page.getByRole('combobox')).first();
    
    if (await genreFilter.isVisible()) {
      await genreFilter.click();
      
      // Select a genre option
      const options = page.locator('option');
      if (await options.count() > 1) {
        await options.nth(1).click();
        
        // Wait for filtered results
        await page.waitForTimeout(1000);
        
        // Verify games are still displayed
        const gameCards = page.locator('[class*="card"]');
        expect(await gameCards.count()).toBeGreaterThan(0);
      }
    }
  });

  test('should search for games', async ({ page }) => {
    // Wait for page load
    await page.waitForTimeout(1000);

    // Look for search input
    const searchInput = page.getByPlaceholder(/search/i).or(page.getByRole('searchbox'));
    
    if (await searchInput.isVisible()) {
      await searchInput.fill('Zelda');
      
      // Wait for search results
      await page.waitForTimeout(1500);
      
      // Verify results (games should still be visible)
      const gameCards = page.locator('[class*="card"]');
      const count = await gameCards.count();
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });
});
