const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const SOLUTION_FILE_NAME = 'vc.Ifx.slnx';
const CHECK_MODE = '--check';
const SELF_TEST_MODE = '--self-test';

function toSolutionPath(filePath) {
  return filePath.split(path.sep).join('/');
}

function ensureDirectory(directoryPath) {
  fs.mkdirSync(directoryPath, { recursive: true });
}

function writeTextFile(filePath, content) {
  ensureDirectory(path.dirname(filePath));
  fs.writeFileSync(filePath, content, 'utf8');
}

function listMarkdownFiles(rootDirectory, relativeDirectory, excludedRelativePrefix) {
  const startDirectory = path.join(rootDirectory, relativeDirectory);
  if (!fs.existsSync(startDirectory)) {
    return [];
  }

  const excludedPrefix = excludedRelativePrefix
    ? `${toSolutionPath(excludedRelativePrefix).replace(/\/+$/, '')}/`
    : null;
  const pendingDirectories = [startDirectory];
  const markdownFiles = [];

  while (pendingDirectories.length > 0) {
    const currentDirectory = pendingDirectories.pop();
    const entries = fs.readdirSync(currentDirectory, { withFileTypes: true });

    for (const entry of entries) {
      const absolutePath = path.join(currentDirectory, entry.name);
      const relativePath = toSolutionPath(path.relative(rootDirectory, absolutePath));

      if (excludedPrefix !== null && relativePath.startsWith(excludedPrefix)) {
        continue;
      }

      if (entry.isDirectory()) {
        pendingDirectories.push(absolutePath);
        continue;
      }

      if (entry.isFile() && entry.name.toLowerCase().endsWith('.md')) {
        markdownFiles.push(relativePath);
      }
    }
  }

  markdownFiles.sort();
  return markdownFiles;
}

function getTrackedMarkdownFiles(rootDirectory) {
  const githubMarkdownFiles = listMarkdownFiles(rootDirectory, '.github', '.github/workflows');
  const copilotMarkdownFiles = listMarkdownFiles(rootDirectory, '.copilot', null);
  return [...githubMarkdownFiles, ...copilotMarkdownFiles].sort();
}

function getSolutionFileEntries(solutionContents) {
  const filePathExpression = /<File\s+Path="([^"]+)"\s*\/>/g;
  const fileEntries = new Set();
  let match = filePathExpression.exec(solutionContents);

  while (match !== null) {
    fileEntries.add(match[1].replace(/\\/g, '/'));
    match = filePathExpression.exec(solutionContents);
  }

  return fileEntries;
}

function getMissingMarkdownEntries(rootDirectory, solutionFilePath) {
  if (!fs.existsSync(solutionFilePath)) {
    throw new Error(`Solution file not found: ${solutionFilePath}`);
  }

  const trackedMarkdownFiles = getTrackedMarkdownFiles(rootDirectory);
  const solutionContents = fs.readFileSync(solutionFilePath, 'utf8');
  const solutionEntries = getSolutionFileEntries(solutionContents);

  return trackedMarkdownFiles.filter((filePath) => !solutionEntries.has(filePath));
}

function printMissingEntries(missingEntries) {
  if (missingEntries.length === 0) {
    console.log(`All tracked markdown files are present in ${SOLUTION_FILE_NAME}.`);
    return;
  }

  console.log(`Missing markdown entries in ${SOLUTION_FILE_NAME}:`);
  for (const entry of missingEntries) {
    console.log(entry);
  }
}

function createFixtureSolution(fileEntries) {
  const lines = [
    '<Solution>',
    '  <Folder Name="/.github/">',
    ...fileEntries.map((entry) => `    <File Path="${entry}" />`),
    '  </Folder>',
    '</Solution>',
    '',
  ];

  return lines.join('\n');
}

function recreateDirectory(directoryPath) {
  fs.rmSync(directoryPath, { recursive: true, force: true });
  fs.mkdirSync(directoryPath, { recursive: true });
}

function runSelfTestCase(baseDirectory, caseName, arrange, assertCase) {
  const caseDirectory = path.join(baseDirectory, caseName);
  recreateDirectory(caseDirectory);
  arrange(caseDirectory);
  assertCase(caseDirectory);
}

function runSelfTests() {
  const fixtureRoot = path.join(process.cwd(), '.sandbox', 'solution-markdown-sync-self-test');
  const results = [];

  fs.rmSync(fixtureRoot, { recursive: true, force: true });
  fs.mkdirSync(fixtureRoot, { recursive: true });

  try {
    runSelfTestCase(
      fixtureRoot,
      'missing-entry-detected',
      (caseDirectory) => {
        writeTextFile(path.join(caseDirectory, '.github', 'skills', 'fake-skill', 'SKILL.md'), '# Skill\n');
        writeTextFile(path.join(caseDirectory, '.github', 'prompts', 'README.md'), '# Prompt README\n');
        writeTextFile(path.join(caseDirectory, '.github', 'workflows', 'ignored.md'), '# Ignored\n');
        writeTextFile(path.join(caseDirectory, '.copilot', 'notes.md'), '# Notes\n');
        writeTextFile(
          path.join(caseDirectory, 'fake.slnx'),
          createFixtureSolution(['.github/prompts/README.md']),
        );
      },
      (caseDirectory) => {
        const missingEntries = getMissingMarkdownEntries(caseDirectory, path.join(caseDirectory, 'fake.slnx'));
        assert.deepEqual(missingEntries, ['.copilot/notes.md', '.github/skills/fake-skill/SKILL.md']);
      },
    );
    results.push('PASS missing-entry-detected');

    runSelfTestCase(
      fixtureRoot,
      'all-present',
      (caseDirectory) => {
        writeTextFile(path.join(caseDirectory, '.github', 'skills', 'fake-skill', 'SKILL.md'), '# Skill\n');
        writeTextFile(path.join(caseDirectory, '.github', 'prompts', 'README.md'), '# Prompt README\n');
        writeTextFile(path.join(caseDirectory, '.github', 'ISSUE-TEMPLATE', 'case.md'), '# Template\n');
        writeTextFile(path.join(caseDirectory, '.copilot', 'notes.md'), '# Notes\n');
        writeTextFile(
          path.join(caseDirectory, 'fake.slnx'),
          createFixtureSolution([
            '.github/ISSUE-TEMPLATE/case.md',
            '.github/prompts/README.md',
            '.github/skills/fake-skill/SKILL.md',
            '.copilot/notes.md',
          ]),
        );
      },
      (caseDirectory) => {
        const missingEntries = getMissingMarkdownEntries(caseDirectory, path.join(caseDirectory, 'fake.slnx'));
        assert.deepEqual(missingEntries, []);
      },
    );
    results.push('PASS all-present');

    runSelfTestCase(
      fixtureRoot,
      'solution-parser-normalizes-separators',
      (caseDirectory) => {
        writeTextFile(path.join(caseDirectory, '.github', 'instructions', 'sample.instructions.md'), '# Instruction\n');
        writeTextFile(
          path.join(caseDirectory, 'fake.slnx'),
          createFixtureSolution(['.github\\instructions\\sample.instructions.md']),
        );
      },
      (caseDirectory) => {
        const missingEntries = getMissingMarkdownEntries(caseDirectory, path.join(caseDirectory, 'fake.slnx'));
        assert.deepEqual(missingEntries, []);
      },
    );
    results.push('PASS solution-parser-normalizes-separators');

    console.log(`Self-test passed: ${results.length}/${results.length} cases.`);
    for (const result of results) {
      console.log(result);
    }
  } finally {
    fs.rmSync(fixtureRoot, { recursive: true, force: true });
  }
}

function getMode(argv) {
  const supportedModes = new Set([CHECK_MODE, SELF_TEST_MODE]);
  const matchingModes = argv.filter((argument) => supportedModes.has(argument));

  if (matchingModes.length !== 1 || argv.length !== 1) {
    const supportedModeList = [...supportedModes].join(', ');
    throw new Error(`Expected exactly one mode: ${supportedModeList}`);
  }

  return matchingModes[0];
}

function main() {
  const mode = getMode(process.argv.slice(2));

  if (mode === CHECK_MODE) {
    const repoRoot = process.cwd();
    const solutionFilePath = path.join(repoRoot, SOLUTION_FILE_NAME);
    const missingEntries = getMissingMarkdownEntries(repoRoot, solutionFilePath);
    printMissingEntries(missingEntries);
    process.exitCode = missingEntries.length === 0 ? 0 : 1;
    return;
  }

  runSelfTests();
  process.exitCode = 0;
}

try {
  main();
} catch (error) {
  const message = error instanceof Error ? error.message : String(error);
  const stack = error instanceof Error && error.stack ? error.stack : message;
  console.error(`sync-solution-markdown failed: ${message}`);
  if (stack !== message) {
    console.error(stack);
  }
  process.exitCode = 1;
}
