import { test, expect } from '@playwright/test';
import { loginAsStudent, logout, waitForToast } from './helpers';

test.describe('Student Flow', () => {
  test.describe('1. Login & Navigation', () => {
    test('student can login and sees correct sidebar', async ({ page }) => {
      await loginAsStudent(page);
      await expect(page).toHaveURL(/\/my-exams/);
      await expect(page.getByRole('link', { name: 'My Exams' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Users' })).not.toBeVisible();
      await expect(page.getByRole('link', { name: 'Quizzes' })).not.toBeVisible();
      await expect(page.getByRole('link', { name: 'Exams' })).not.toBeVisible();
      await expect(page.getByRole('link', { name: 'Audit Logs' })).not.toBeVisible();
    });
  });

  test.describe('2. My Exams Dashboard', () => {
    test.beforeEach(async ({ page }) => {
      await loginAsStudent(page);
      await expect(page).toHaveURL(/\/my-exams/);
    });

    test('my exams page shows pending and history tabs', async ({ page }) => {
      await expect(page.getByRole('tab', { name: 'Pending' })).toBeVisible();
      await expect(page.getByRole('tab', { name: 'History' })).toBeVisible();
    });

    test('pending tab shows correct columns', async ({ page }) => {
      await page.getByRole('tab', { name: 'Pending' }).click();
      await expect(page.getByRole('columnheader', { name: /quiz/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /assigned/i })).toBeVisible();
    });

    test('history tab shows correct columns', async ({ page }) => {
      await page.getByRole('tab', { name: 'History' }).click();
      await expect(page.getByRole('columnheader', { name: /quiz/i })).toBeVisible();
    });
  });

  test.describe('3. Take Exam', () => {
    test('student can start an exam', async ({ page }) => {
      await loginAsStudent(page);
      await page.getByRole('tab', { name: 'Pending' }).click();

      const startBtn = page.getByRole('button', { name: /Start Exam/i }).first();
      const continueBtn = page.getByRole('button', { name: /Continue/i }).first();

      const hasStart = await startBtn.isVisible().catch(() => false);
      const hasContinue = await continueBtn.isVisible().catch(() => false);

      if (!hasStart && !hasContinue) {
        test.skip(true, 'No pending exams available to take');
        return;
      }

      if (hasStart) {
        await startBtn.click();
      } else {
        await continueBtn.click();
      }

      await expect(page).toHaveURL(/\/exams\/\d+\/take/);
      // Verify exam elements
      await expect(page.getByText(/Question \d+ of \d+/i)).toBeVisible();
    });

    test('student can navigate exam questions', async ({ page }) => {
      await loginAsStudent(page);
      await page.getByRole('tab', { name: 'Pending' }).click();

      const startBtn = page.getByRole('button', { name: /Start Exam|Continue/i }).first();
      if (!await startBtn.isVisible().catch(() => false)) {
        test.skip(true, 'No pending exams available');
        return;
      }
      await startBtn.click();
      await expect(page).toHaveURL(/\/exams\/\d+\/take/);

      // Try to navigate to next if available
      const nextBtn = page.getByRole('button', { name: /Next/i });
      if (await nextBtn.isVisible()) {
        await nextBtn.click();
      }

      // Try previous
      const prevBtn = page.getByRole('button', { name: /Previous/i });
      if (await prevBtn.isVisible()) {
        await prevBtn.click();
      }
    });
  });

  test.describe('4. Exam Review (History)', () => {
    test('student can view history tab', async ({ page }) => {
      await loginAsStudent(page);
      await page.getByRole('tab', { name: 'History' }).click();
      // Either shows exams or empty state
      const examRows = page.getByRole('row');
      // At minimum header row visible
      await expect(examRows.first()).toBeVisible();
    });
  });

  test.describe('5. Change Password', () => {
    test('student can access change password page', async ({ page }) => {
      await loginAsStudent(page);
      await page.getByRole('button', { name: /student1/i }).click();
      await page.getByRole('button', { name: /Change Password/i }).click();
      await expect(page).toHaveURL(/\/settings\/password/);
    });
  });

  test.describe('6. Logout', () => {
    test('student can logout and redirect to login', async ({ page }) => {
      await loginAsStudent(page);
      await logout(page);
    });

    test('student cannot access protected routes after logout', async ({ page }) => {
      await loginAsStudent(page);
      await logout(page);
      await page.goto('/my-exams');
      await expect(page).toHaveURL(/\/login/);
    });
  });
});
