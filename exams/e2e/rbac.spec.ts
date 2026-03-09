import { test, expect } from '@playwright/test';
import { loginAsStudent, loginAsTeacher, login } from './helpers';

test.describe('Cross-Role Scenarios', () => {
  test.describe('Role-Based Access Control', () => {
    test('student cannot access /users', async ({ page }) => {
      await loginAsStudent(page);
      await page.goto('/users');
      await expect(page).toHaveURL(/\/login|\/my-exams/);
    });

    test('teacher cannot access /audit-logs', async ({ page }) => {
      await loginAsTeacher(page);
      await page.goto('/audit-logs');
      await expect(page).toHaveURL(/\/login|\/quizzes/);
    });

    test('student cannot access /quizzes', async ({ page }) => {
      await loginAsStudent(page);
      await page.goto('/quizzes');
      await expect(page).toHaveURL(/\/login|\/my-exams/);
    });
  });

  test.describe('Error Handling', () => {
    test('login with wrong credentials shows error', async ({ page }) => {
      await page.goto('/login');
      await page.getByPlaceholder('Enter your username').fill('wronguser');
      await page.getByPlaceholder('Enter your password').fill('wrongpass');
      await page.getByRole('button', { name: 'Log In' }).click();
      await expect(page.getByText(/invalid.*username.*password|incorrect/i)).toBeVisible({ timeout: 10000 });
    });
  });
});
