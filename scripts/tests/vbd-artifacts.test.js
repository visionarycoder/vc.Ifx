#!/usr/bin/env node
'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');
const { test, before, after } = require('node:test');

const repositoryRoot = path.resolve(__dirname, '..', '..');
const cliPath = path.join(repositoryRoot, 'scripts', 'vbd-artifacts.js');
const sandboxRoot = path.join(repositoryRoot, '.sandbox');

let testRoot = '';

before(() => {
  fs.mkdirSync(sandboxRoot, { recursive: true });
  testRoot = fs.mkdtempSync(path.join(sandboxRoot, 'vbd-artifacts-test-'));
});

after(() => {
  if (!testRoot) {
    return;
  }

  fs.rmSync(testRoot, { recursive: true, force: true });
});

test('init creates the standard seven directories', () => {
  const analysisRoot = path.join(testRoot, 'init-tree');
  const result = runCli(['init', relativeToRepository(analysisRoot)]);

  assert.equal(result.status, 0, result.stderr || result.stdout);

  const expectedDirectories = [
    'use-cases',
    'decisions',
    'contracts',
    'estimates',
    'checkpoints',
    'simulations',
    'portfolio',
  ];

  for (const directoryName of expectedDirectories) {
    assert.equal(fs.existsSync(path.join(analysisRoot, directoryName)), true, directoryName);
  }
});

test('validate passes on an empty initialized tree', () => {
  const analysisRoot = path.join(testRoot, 'empty-tree');
  runCli(['init', relativeToRepository(analysisRoot)]);

  const result = runCli(['validate', relativeToRepository(analysisRoot)]);

  assert.equal(result.status, 0, result.stderr || result.stdout);
  assert.match(result.stdout, /"filesScanned":0/);
  assert.match(result.stdout, /"errors":0/);
});

test('validate reports JSON parse errors as blocking failures', () => {
  const analysisRoot = path.join(testRoot, 'parse-error-tree');
  runCli(['init', relativeToRepository(analysisRoot)]);
  fs.writeFileSync(path.join(analysisRoot, 'use-cases', 'broken.json'), '{"id":', 'utf8');

  const result = runCli(['validate', relativeToRepository(analysisRoot)]);

  assert.equal(result.status, 1, result.stdout);
  assert.match(result.stdout, /Parse error:/);
});

test('validate reports duplicate ids as blocking failures', () => {
  const analysisRoot = path.join(testRoot, 'duplicate-id-tree');
  runCli(['init', relativeToRepository(analysisRoot)]);

  writeJson(path.join(analysisRoot, 'use-cases', 'uc-1.json'), {
    id: 'UC-Shared-1',
    title: 'One',
    actor: 'User',
    trigger: 'Trigger',
    preconditions: [],
    outcome: 'Outcome',
    mainFlow: ['A'],
    alternateFlows: [],
    failureFlows: [],
    supportingSymbols: [],
    gaps: [],
    conflicts: [],
    confidence: 'high',
  });

  writeJson(path.join(analysisRoot, 'decisions', 'dup.json'), {
    id: 'UC-Shared-1',
  });

  const result = runCli(['validate', relativeToRepository(analysisRoot)]);

  assert.equal(result.status, 1, result.stdout);
  assert.match(result.stdout, /Duplicate id 'UC-Shared-1'/);
});

test('validate reports dependency cycles as blocking failures', () => {
  const analysisRoot = path.join(testRoot, 'cycle-tree');
  runCli(['init', relativeToRepository(analysisRoot)]);

  writeTask(path.join(analysisRoot, 'portfolio', 'task-a.json'), {
    id: 'TASK-A-1',
    dependencies: ['TASK-B-1'],
  });

  writeTask(path.join(analysisRoot, 'portfolio', 'task-b.json'), {
    id: 'TASK-B-1',
    dependencies: ['TASK-A-1'],
  });

  const result = runCli(['validate', relativeToRepository(analysisRoot)]);

  assert.equal(result.status, 1, result.stdout);
  assert.match(result.stdout, /Dependency cycle: TASK-A-1 -> TASK-B-1 -> TASK-A-1/);
});

test('validate reports unresolved references as warnings without failing', () => {
  const analysisRoot = path.join(testRoot, 'warning-tree');
  runCli(['init', relativeToRepository(analysisRoot)]);

  writeTask(path.join(analysisRoot, 'portfolio', 'task-a.json'), {
    id: 'TASK-Warn-1',
    dependencies: ['TASK-Missing-1'],
  });

  const result = runCli(['validate', relativeToRepository(analysisRoot)]);

  assert.equal(result.status, 0, result.stdout);
  assert.match(result.stdout, /WARN Unresolved reference 'TASK-Missing-1'/);
  assert.match(result.stdout, /"warnings":1/);
});

test('checkpoint writes a deterministic checkpoint file with summary fields', () => {
  const analysisRoot = path.join(testRoot, 'checkpoint-tree');
  runCli(['init', relativeToRepository(analysisRoot)]);

  writeTask(path.join(analysisRoot, 'portfolio', 'task-a.json'), {
    id: 'TASK-Ready-1',
    dependencies: [],
  });

  const result = runCli(
    ['checkpoint', relativeToRepository(analysisRoot), 'build', '2'],
    {
      VBD_CHECKPOINT_CLOCK: '2026-09-10T08:53:26.830Z',
    },
  );

  assert.equal(result.status, 0, result.stdout);

  const checkpointPath = path.join(analysisRoot, 'checkpoints', 'build-02.json');
  assert.equal(fs.existsSync(checkpointPath), true);

  const checkpoint = JSON.parse(fs.readFileSync(checkpointPath, 'utf8'));
  assert.equal(checkpoint.phase, 'build');
  assert.equal(checkpoint.seq, 2);
  assert.equal(checkpoint.timestamp, '2026-09-10T08:53:26.830Z');
  assert.deepEqual(checkpoint.validationSummary, {
    command: 'validate',
    root: windowsPath(analysisRoot),
    filesScanned: 1,
    errors: 0,
    warnings: 0,
    sourceChecksSkipped: false,
  });
  assert.deepEqual(checkpoint.nextReadyTasks, ['TASK-Ready-1']);
});

function writeTask(filePath, overrides) {
  const task = {
    id: 'TASK-Example-1',
    kind: 'construction-task',
    componentId: 'CMP-Example',
    title: 'Example task',
    projectType: 'Ifx',
    useCases: [],
    symbols: [],
    gaps: [],
    dependencies: [],
    outputs: ['src/output.txt'],
    validation: ['Run validation'],
    completionCriteria: ['Done'],
    tokenBudget: 1,
    status: 'pending',
    ...overrides,
  };

  writeJson(filePath, task);
}

function writeJson(filePath, value) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, 'utf8');
}

function runCli(argumentsList, extraEnvironment = {}) {
  return spawnSync(process.execPath, [cliPath, ...argumentsList], {
    cwd: repositoryRoot,
    encoding: 'utf8',
    env: {
      ...process.env,
      ...extraEnvironment,
    },
  });
}

function relativeToRepository(targetPath) {
  return path.relative(repositoryRoot, targetPath) || '.';
}

function windowsPath(targetPath) {
  return targetPath.replace(/\//g, '\\');
}
