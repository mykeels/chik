import { Page, expect, Locator } from '@playwright/test';

export async function login(page: Page, username: string, password: string) {
  await page.goto('/login');
  await page.getByPlaceholder('Enter your username').fill(username);
  await page.getByPlaceholder('Enter your password').fill(password);
  await page.getByRole('button', { name: 'Log In' }).click();
}

export async function logout(page: Page) {
  await page.getByRole('button', { name: 'Logout' }).click();
  await expect(page).toHaveURL(/\/login/);
}

export async function loginAsAdmin(page: Page) {
  await login(page, 'admin', 'admin123');
  await expect(page).toHaveURL(/\/users/);
}

export async function loginAsTeacher(page: Page) {
  await login(page, 'teacher1', 'teacher123');
  await expect(page).toHaveURL(/\/quizzes/);
}

export async function loginAsStudent(page: Page) {
  await login(page, 'student1', 'student123');
  await expect(page).toHaveURL(/\/my-exams/);
}

export async function waitForToast(page: Page, text?: string) {
  const toast = page.locator('.Toastify__toast');
  await toast.first().waitFor({ state: 'visible', timeout: 10000 });
  if (text) {
    await expect(toast.first()).toContainText(text);
  }
}

/**
 * Get a MUI Select combobox by its label text.
 * MUI's aria-labelledby association isn't always resolved by Playwright's name filter,
 * so we find the FormControl wrapper containing the label text.
 */
export function getMuiSelect(container: Page | Locator, labelText: string | RegExp) {
  const c = 'locator' in container ? container : (container as Page);
  return c.locator('.MuiFormControl-root').filter({ hasText: labelText }).locator('[role="combobox"]');
}
