import { test, expect } from '@playwright/test';

test('find Dad Joke home page', async ({ page, baseURL }) => {
  console.log('Test: Open Dad Joke website');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/');
  await expect(page).toHaveTitle(/Dad/);
});

test('find Dad Joke search page', async ({ page, baseURL }) => {
  console.log('Test: Find Search page');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/');
  await page.getByRole('link', { name: 'Search' }).click();
  await expect(page.getByRole('heading', { name: 'Search the Dad-A-Base' })).toBeVisible();
});


test('search and find some chicken jokes', async ({ page, baseURL }) => {
  console.log('Test: Search Dad Jokes');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/Search');
  await page.locator('#inputText').fill('chicken');
  await page.getByRole('button', { name: 'Search' }).click();
  await page.waitForSelector('li');
  const listItems = await page.$$eval('li', items => items.length);
  expect(listItems).toBeGreaterThan(0);
  console.log('Found ' + listItems + ' chicken jokes!');
});
