import path from 'path';
import { execSync } from 'child_process';

let currentBranch = execSync(`git branch --show-current`).toString().trim();
currentBranch = ['main'].includes(currentBranch) ? 'main' : 'beta';
const outDirPath = path.join(__dirname, '../dist');

execSync(`npx wrangler pages deploy ${outDirPath} --project-name chikexams --branch ${currentBranch}`, {
  stdio: 'inherit',
});
