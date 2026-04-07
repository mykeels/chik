import { test, expect } from '@playwright/test';
import { loginAsTeacher, logout, waitForToast, getMuiSelect } from './helpers';

const QUIZ_TITLE = `Teacher E2E Quiz ${Date.now()}`;

test.describe('Teacher Flow', () => {
  test.describe('1. Login & Navigation', () => {
    test('teacher can login and sees correct sidebar', async ({ page }) => {
      await loginAsTeacher(page);
      await expect(page).toHaveURL(/\/quizzes/);
      await expect(page.getByRole('link', { name: 'Quizzes' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Exams' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Students' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Users' })).not.toBeVisible();
      await expect(page.getByRole('link', { name: 'Audit Logs' })).not.toBeVisible();
    });
  });

  test.describe('2. Quiz Management', () => {
    test.beforeEach(async ({ page }) => {
      await loginAsTeacher(page);
      await expect(page).toHaveURL(/\/quizzes/);
    });

    test('create quiz without examiner field', async ({ page }) => {
      await page.getByRole('button', { name: /New Quiz/i }).click();
      await expect(page).toHaveURL(/\/quizzes\/new/);

      await page.getByLabel('Title').fill(QUIZ_TITLE);
      await page.getByLabel('Description').fill('Quiz for testing teacher flow');
      await page.getByLabel(/Duration \(hrs\)/i).fill('0');
      await page.getByLabel(/Duration \(mins\)/i).fill('45');

      // Verify NO Examiner field (Admin-only)
      await expect(getMuiSelect(page, /Examiner/i)).not.toBeVisible();

      await page.getByRole('button', { name: /Save Quiz/i }).click();
      await waitForToast(page);
      await expect(page).toHaveURL(/\/quizzes\/\d+\/edit/);
    });

    test('add all question types', async ({ page }) => {
      // Navigate to an existing quiz to add questions
      await page.getByRole('button', { name: /New Quiz/i }).click();
      await page.getByLabel('Title').fill(`Questions Test ${Date.now()}`);
      await page.getByLabel('Description').fill('desc');
      await page.getByRole('button', { name: /Save Quiz/i }).click();
      await waitForToast(page);
      await expect(page).toHaveURL(/\/quizzes\/\d+\/edit/);

      const saveQuestion = async () => {
        await page.getByRole('button', { name: /Save Question/i }).click();
        await waitForToast(page);
        // Wait for the dialog to fully close before proceeding
        await page.getByRole('dialog').waitFor({ state: 'hidden', timeout: 10000 });
      };

      // Add Single Choice
      await page.getByRole('button', { name: /Add Question/i }).click();
      await page.getByLabel('Question Prompt').fill('Capital of France?');
      await getMuiSelect(page, /Question Type/i).click();
      await page.getByRole('option', { name: 'Single Choice' }).click();
      await page.getByLabel('Score').fill('5');
      const addOpt = page.getByRole('button', { name: /Add Option/i });
      await addOpt.click();
      await page.getByPlaceholder(/Option/i).nth(0).fill('London');
      await addOpt.click();
      await page.getByPlaceholder(/Option/i).nth(1).fill('Paris');
      await addOpt.click();
      await page.getByPlaceholder(/Option/i).nth(2).fill('Berlin');
      await addOpt.click();
      await page.getByPlaceholder(/Option/i).nth(3).fill('Madrid');
      await saveQuestion();

      // Add Multiple Choice
      await page.getByRole('button', { name: /Add Question/i }).click();
      await page.getByLabel('Question Prompt').fill('Which are programming languages?');
      await getMuiSelect(page, /Question Type/i).click();
      await page.getByRole('option', { name: 'Multiple Choice' }).click();
      await page.getByLabel('Score').fill('10');
      const addOpt2 = page.getByRole('button', { name: /Add Option/i });
      await addOpt2.click();
      await page.getByPlaceholder(/Option/i).nth(0).fill('Python');
      await addOpt2.click();
      await page.getByPlaceholder(/Option/i).nth(1).fill('HTML');
      await addOpt2.click();
      await page.getByPlaceholder(/Option/i).nth(2).fill('JavaScript');
      await addOpt2.click();
      await page.getByPlaceholder(/Option/i).nth(3).fill('CSS');
      await saveQuestion();

      // Add True/False
      await page.getByRole('button', { name: /Add Question/i }).click();
      await page.getByLabel('Question Prompt').fill('The Earth is flat');
      await getMuiSelect(page, /Question Type/i).click();
      await page.getByRole('option', { name: /True.*False/i }).click();
      await page.getByLabel('Score').fill('5');
      // Verify auto-populated True/False options
      await expect(page.getByText('True')).toBeVisible();
      await expect(page.getByText('False')).toBeVisible();
      await saveQuestion();

      // Add Fill in the Blank
      await page.getByRole('button', { name: /Add Question/i }).click();
      await page.getByLabel('Question Prompt').fill('The chemical symbol for water is ___');
      await getMuiSelect(page, /Question Type/i).click();
      await page.getByRole('option', { name: /Fill in the Blank/i }).click();
      await page.getByLabel('Score').fill('5');
      await saveQuestion();

      // Add Short Answer
      await page.getByRole('button', { name: /Add Question/i }).click();
      await page.getByLabel('Question Prompt').fill('What is the capital of Japan?');
      await getMuiSelect(page, /Question Type/i).click();
      await page.getByRole('option', { name: /Short Answer/i }).click();
      await page.getByLabel('Score').fill('5');
      await saveQuestion();

      // Add Essay
      await page.getByRole('button', { name: /Add Question/i }).click();
      await page.getByLabel('Question Prompt').fill('Describe the water cycle');
      await getMuiSelect(page, /Question Type/i).click();
      await page.getByRole('option', { name: 'Essay' }).click();
      await page.getByLabel('Score').fill('20');
      await saveQuestion();
    });
  });

  test.describe('3. Student Management', () => {
    test('teacher can see students page', async ({ page }) => {
      await loginAsTeacher(page);
      await page.getByRole('link', { name: 'Students' }).click();
      await expect(page).toHaveURL(/\/users/);
      // Should only see students tab or filtered view
      await expect(page.getByRole('tab', { name: 'Students' })).toBeVisible();
    });
  });

  test.describe('4. Exam Management', () => {
    test.beforeEach(async ({ page }) => {
      await loginAsTeacher(page);
      await page.getByRole('link', { name: 'Exams' }).click();
      await expect(page).toHaveURL(/\/exams/);
    });

    test('teacher can assign exam to student', async ({ page }) => {
      await page.getByRole('button', { name: /Assign Exam/i }).click();
      const dialog = page.getByRole('dialog');
      await expect(dialog).toBeVisible();

      const studentSelect = getMuiSelect(dialog, /Students/i);
      await studentSelect.click();
      const enabledStudents = page.locator('[role="option"]:not([aria-disabled="true"])');
      if (await enabledStudents.count() > 0) {
        await enabledStudents.first().click();
        await page.keyboard.press('Escape');
        const quizSelect = getMuiSelect(dialog, /Quiz/i);
        await quizSelect.click();
        const enabledQuizzes = page.locator('[role="option"]:not([aria-disabled="true"])');
        if (await enabledQuizzes.count() > 0) {
          await enabledQuizzes.first().click();
          await dialog.getByRole('button', { name: 'Assign' }).click();
          await waitForToast(page);
        } else {
          await dialog.getByRole('button', { name: /Cancel/i }).click();
        }
      } else {
        await dialog.getByRole('button', { name: /Cancel/i }).click();
      }
    });
  });

  test.describe('5. Change Password', () => {
    test('teacher can access change password page', async ({ page }) => {
      await loginAsTeacher(page);
      await page.getByRole('button', { name: /teacher1/i }).click();
      await page.getByRole('button', { name: /Change Password/i }).click();
      await expect(page).toHaveURL(/\/settings\/password/);
    });
  });

  test.describe('6. Logout', () => {
    test('teacher can logout', async ({ page }) => {
      await loginAsTeacher(page);
      await logout(page);
    });
  });
});
