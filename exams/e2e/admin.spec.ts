import { test, expect } from '@playwright/test';
import { loginAsAdmin, logout, waitForToast, getMuiSelect } from './helpers';

const TEACHER_USERNAME = `teacher_e2e_${Date.now()}`;
const STUDENT_USERNAME = `student_e2e_${Date.now()}`;
const QUIZ_TITLE = `Admin E2E Quiz ${Date.now()}`;

test.describe('Admin Flow', () => {
  test.describe('1. Login & Navigation', () => {
    test('admin can login and sees correct sidebar', async ({ page }) => {
      await loginAsAdmin(page);
      await expect(page).toHaveURL(/\/users/);
      await expect(page.getByRole('link', { name: 'Users' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Quizzes' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Exams' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Audit Logs' })).toBeVisible();
    });
  });

  test.describe('2. User Management', () => {
    test.beforeEach(async ({ page }) => {
      await loginAsAdmin(page);
      await page.getByRole('link', { name: 'Users' }).click();
      await expect(page).toHaveURL(/\/users/);
    });

    test('users page shows correct tabs and columns', async ({ page }) => {
      await expect(page.getByRole('tab', { name: 'All' })).toBeVisible();
      await expect(page.getByRole('tab', { name: 'Teachers' })).toBeVisible();
      await expect(page.getByRole('tab', { name: 'Students' })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /username/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /roles/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /created/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /actions/i })).toBeVisible();
    });

    test('create teacher', async ({ page }) => {
      await page.getByRole('button', { name: /New User/i }).click();
      await page.getByRole('menuitem', { name: 'New Teacher' }).click();
      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByRole('dialog').getByRole('heading')).toContainText(/New Teacher|Create User/i);
      await page.getByLabel('Username').fill(TEACHER_USERNAME);
      await page.getByLabel('Password').fill('password123');
      await expect(page.getByLabel('Role')).toHaveValue('Teacher');
      await page.getByRole('button', { name: 'Save' }).click();
      await waitForToast(page);
      await page.getByRole('tab', { name: 'Teachers' }).click();
      await expect(page.getByText(TEACHER_USERNAME)).toBeVisible();
    });

    test('create student', async ({ page }) => {
      await page.getByRole('button', { name: /New User/i }).click();
      await page.getByRole('menuitem', { name: 'New Student' }).click();
      await expect(page.getByRole('dialog')).toBeVisible();
      await page.getByLabel('Username').fill(STUDENT_USERNAME);
      await page.getByLabel('Password').fill('password123');
      await expect(page.getByLabel('Role')).toHaveValue('Student');
      await page.getByRole('button', { name: 'Save' }).click();
      await waitForToast(page);
      await page.getByRole('tab', { name: 'Students' }).click();
      await expect(page.getByText(STUDENT_USERNAME)).toBeVisible();
    });

    test('edit user', async ({ page }) => {
      const editButtons = page.getByRole('button', { name: 'Edit' });
      await editButtons.first().waitFor({ state: 'visible' });
      await editButtons.first().click();
      await expect(page.getByRole('dialog')).toBeVisible();
      const usernameField = page.getByLabel('Username');
      const currentValue = await usernameField.inputValue();
      const newValue = currentValue + '_edited';
      await usernameField.clear();
      await usernameField.fill(newValue);
      await page.getByRole('button', { name: 'Save' }).click();
      await waitForToast(page);
      await expect(page.getByText(newValue)).toBeVisible();
    });
  });

  test.describe('3. Quiz Management', () => {
    test.beforeEach(async ({ page }) => {
      await loginAsAdmin(page);
      await page.getByRole('link', { name: 'Quizzes' }).click();
      await expect(page).toHaveURL(/\/quizzes/);
    });

    test('quizzes page shows correct columns', async ({ page }) => {
      await expect(page.getByRole('columnheader', { name: /title/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /questions/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /duration/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /created/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /actions/i })).toBeVisible();
    });

    test('create quiz and add questions', async ({ page }) => {
      await page.getByRole('button', { name: /New Quiz/i }).click();
      await expect(page).toHaveURL(/\/quizzes\/new/);

      await page.getByLabel('Title').fill(QUIZ_TITLE);
      await page.getByLabel('Description').fill('Test quiz created by admin');
      await page.getByLabel(/Duration \(hrs\)/i).fill('1');
      await page.getByLabel(/Duration \(mins\)/i).fill('30');

      // Examiner field (Admin-only) - MUI Select
      const examinerSelect = getMuiSelect(page, /Examiner/i);
      await expect(examinerSelect).toBeVisible();

      await page.getByRole('button', { name: /Save Quiz/i }).click();
      await waitForToast(page);
      await expect(page).toHaveURL(/\/quizzes\/\d+\/edit/);

      // Add Single Choice question
      await page.getByRole('button', { name: /Add Question/i }).click();
      const dialog2 = page.getByRole('dialog');
      await dialog2.getByLabel('Question Prompt').fill('What is 2 + 2?');

      // MUI Select for Question Type
      const typeSelect = getMuiSelect(page, /Question Type/i);
      await typeSelect.click();
      await page.getByRole('option', { name: 'Single Choice' }).click();

      await dialog2.getByLabel('Score').fill('5');

      // Add options
      const addOptionBtn = page.getByRole('button', { name: /Add Option/i });
      await addOptionBtn.click();
      const optionInputs = page.getByPlaceholder(/Option/i);
      await optionInputs.nth(0).fill('3');
      await addOptionBtn.click();
      await optionInputs.nth(1).fill('4');
      await addOptionBtn.click();
      await optionInputs.nth(2).fill('5');
      await addOptionBtn.click();
      await optionInputs.nth(3).fill('6');

      await page.getByRole('button', { name: /Save Question/i }).click();
      await waitForToast(page);
    });
  });

  test.describe('4. Exam Management', () => {
    test.beforeEach(async ({ page }) => {
      await loginAsAdmin(page);
      await page.getByRole('link', { name: 'Exams' }).click();
      await expect(page).toHaveURL(/\/exams/);
    });

    test('exams page shows correct columns and filter', async ({ page }) => {
      await expect(page.getByRole('columnheader', { name: /student/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /quiz/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /status/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /score/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /started/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /actions/i })).toBeVisible();
      // MUI Select for status filter
      await expect(getMuiSelect(page, /Filter by status/i)).toBeVisible();
    });

    test('assign exam to student', async ({ page }) => {
      await page.getByRole('button', { name: /Assign Exam/i }).click();
      const dialog = page.getByRole('dialog');
      await expect(dialog).toBeVisible();

      // Select student (MUI Select) - skip disabled placeholder
      const studentSelect = getMuiSelect(dialog, /Student/i);
      await studentSelect.click();
      await page.locator('[role="option"]:not([aria-disabled="true"])').first().click();

      // Select quiz (MUI Select) - skip disabled placeholder
      const quizSelect = getMuiSelect(dialog, /Quiz/i);
      await quizSelect.click();
      await page.locator('[role="option"]:not([aria-disabled="true"])').first().click();

      await dialog.getByRole('button', { name: 'Assign' }).click();
      await waitForToast(page);
    });
  });

  test.describe('5. Audit Logs', () => {
    test('audit logs page shows correct columns', async ({ page }) => {
      await loginAsAdmin(page);
      await page.getByRole('link', { name: 'Audit Logs' }).click();
      await expect(page).toHaveURL(/\/audit-logs/);
      await expect(page.getByRole('columnheader', { name: /user/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /service/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /entity/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /date/i })).toBeVisible();
    });
  });

  test.describe('6. Change Password', () => {
    test('admin can access change password page', async ({ page }) => {
      await loginAsAdmin(page);
      await page.getByRole('button', { name: /admin/i }).click();
      await page.getByRole('button', { name: /Change Password/i }).click();
      await expect(page).toHaveURL(/\/settings\/password/);
    });
  });

  test.describe('7. Logout', () => {
    test('admin can logout and is redirected to login', async ({ page }) => {
      await loginAsAdmin(page);
      await logout(page);
      await expect(page).toHaveURL(/\/login/);
    });

    test('cannot access protected routes after logout', async ({ page }) => {
      await loginAsAdmin(page);
      await logout(page);
      await page.goto('/users');
      await expect(page).toHaveURL(/\/login/);
    });
  });
});
