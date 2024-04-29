import { test, expect } from '@playwright/test';

test('find Dad Joke home page', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/Dad/);
});

test('find Dad Joke search page', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('link', { name: 'Search' }).click();
  await expect(page.getByRole('heading', { name: 'Search the Dad-A-Base' })).toBeVisible();
});


test('search and find some chicken jokes', async ({ page }) => {
  await page.goto('/Search');
  await page.locator('#inputText').fill('chicken');
  await page.getByRole('button', { name: 'Search' }).click();
  await page.waitForSelector('li');
  const listItems = await page.$$eval('li', items => items.length);
  expect(listItems).toBeGreaterThan(0);
  console.log('Found ' + listItems + ' chicken jokes!');
});
