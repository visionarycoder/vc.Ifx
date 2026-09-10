#!/usr/bin/env node
'use strict';

const fs = require('node:fs');
const path = require('node:path');

const STANDARD_DIRECTORIES = Object.freeze([
  'use-cases',
  'decisions',
  'contracts',
  'estimates',
  'checkpoints',
  'simulations',
  'portfolio',
]);

const SCHEMA_DIRECTORY = path.resolve(
  __dirname,
  '..',
  '.github',
  'skills',
  'vbd-phased-modernization',
  'assets',
  'schemas',
);

const DIRECTORY_KIND_MAP = Object.freeze({
  'use-cases': 'use-case',
  decisions: 'decision-record',
  contracts: 'operational-contract',
  estimates: 'candidate-estimate',
  checkpoints: 'construction-checkpoint',
  simulations: 'change-simulation-result',
  portfolio: 'portfolio-record',
});

const REFERENCE_FIELD_PATTERN = /(references|dependsOn|dependencies)/i;
const DEPENDENCY_FIELD_PATTERN = /^(dependsOn|dependencies)$/i;
const ID_LIKE_PATTERN = /^(?:[A-Z]{2,10}-[A-Za-z0-9][A-Za-z0-9-]*|\w[\w-]*-\d+)$/;

const CONFIDENCE_WEIGHTS = Object.freeze({
  high: 100,
  medium: 70,
  low: 40,
});

function main() {
  const args = process.argv.slice(2);
  const command = args[0];

  if (!command) {
    printUsage();
    process.exitCode = 1;
    return;
  }

  try {
    switch (command) {
      case 'init':
        runInitCommand(args.slice(1));
        break;
      case 'validate':
        runValidateCommand(args.slice(1));
        break;
      case 'graph':
        runGraphCommand(args.slice(1));
        break;
      case 'plan':
        runPlanCommand(args.slice(1));
        break;
      case 'score':
        runScoreCommand(args.slice(1));
        break;
      case 'dashboard':
        runDashboardCommand(args.slice(1));
        break;
      case 'checkpoint':
        runCheckpointCommand(args.slice(1));
        break;
      default:
        logError(`Unknown command '${command}'.`);
        printUsage();
        process.exitCode = 1;
        break;
    }
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    logError(message);
    process.exitCode = 1;
  }
}

function runInitCommand(args) {
  const [rootArgument] = args;
  requireArgument(rootArgument, 'init <root>');

  const rootPath = resolveInputPath(rootArgument);
  const created = [];
  const existing = [];

  fs.mkdirSync(rootPath, { recursive: true });

  for (const directory of STANDARD_DIRECTORIES) {
    const directoryPath = path.join(rootPath, directory);

    if (fs.existsSync(directoryPath)) {
      existing.push(toDisplayPath(directoryPath));
      continue;
    }

    fs.mkdirSync(directoryPath, { recursive: true });
    created.push(toDisplayPath(directoryPath));
  }

  logSummary({
    command: 'init',
    root: toDisplayPath(rootPath),
    createdCount: created.length,
    existingCount: existing.length,
    directories: STANDARD_DIRECTORIES,
  });
}

function runValidateCommand(args) {
  const parsed = parseValidateArguments(args);
  const result = validateArtifacts(parsed.rootPath, parsed.sourceRootPath);
  emitValidationResult(result);
  process.exitCode = result.errorCount > 0 ? 1 : 0;
}

function runGraphCommand(args) {
  const [rootArgument, outputArgument] = args;
  requireArgument(rootArgument, 'graph <root> <out.md>');
  requireArgument(outputArgument, 'graph <root> <out.md>');

  const rootPath = resolveInputPath(rootArgument);
  const outputPath = resolveInputPath(outputArgument);
  const context = analyzeArtifacts(rootPath);
  const graphMarkdown = renderGraphMarkdown(rootPath, context);

  ensureParentDirectory(outputPath);
  fs.writeFileSync(outputPath, graphMarkdown, 'utf8');

  logSummary({
    command: 'graph',
    root: toDisplayPath(rootPath),
    output: toDisplayPath(outputPath),
    nodes: context.artifacts.length,
    edges: context.referenceEdges.length,
    orphanEdges: context.unresolvedReferences.length,
  });
}

function runPlanCommand(args) {
  const [rootArgument, outputArgument] = args;
  requireArgument(rootArgument, 'plan <root> [out.md]');

  const rootPath = resolveInputPath(rootArgument);
  const context = analyzeArtifacts(rootPath);
  const plan = computePlan(context);
  const markdown = renderPlanMarkdown(rootPath, plan);

  if (outputArgument) {
    const outputPath = resolveInputPath(outputArgument);
    ensureParentDirectory(outputPath);
    fs.writeFileSync(outputPath, markdown, 'utf8');
    logSummary({
      command: 'plan',
      root: toDisplayPath(rootPath),
      output: toDisplayPath(outputPath),
      waves: plan.waves.length,
      blockedByMissingDependencies: plan.blockedByMissingDependencies.length,
    });
    return;
  }

  process.stdout.write(markdown);
}

function runScoreCommand(args) {
  const [rootArgument, outputArgument] = args;
  requireArgument(rootArgument, 'score <root> [out.md]');

  const rootPath = resolveInputPath(rootArgument);
  const context = analyzeArtifacts(rootPath);
  const report = computeScoreReport(context);
  const markdown = renderScoreMarkdown(rootPath, report);

  if (outputArgument) {
    const outputPath = resolveInputPath(outputArgument);
    ensureParentDirectory(outputPath);
    fs.writeFileSync(outputPath, markdown, 'utf8');
    logSummary({
      command: 'score',
      root: toDisplayPath(rootPath),
      output: toDisplayPath(outputPath),
      scoredArtifacts: report.entries.length,
    });
    return;
  }

  process.stdout.write(markdown);
}

function runDashboardCommand(args) {
  const [rootArgument, outputArgument] = args;
  requireArgument(rootArgument, 'dashboard <root> <out.md>');
  requireArgument(outputArgument, 'dashboard <root> <out.md>');

  const rootPath = resolveInputPath(rootArgument);
  const outputPath = resolveInputPath(outputArgument);
  const context = analyzeArtifacts(rootPath);
  const markdown = renderDashboardMarkdown(rootPath, context);

  ensureParentDirectory(outputPath);
  fs.writeFileSync(outputPath, markdown, 'utf8');

  logSummary({
    command: 'dashboard',
    root: toDisplayPath(rootPath),
    output: toDisplayPath(outputPath),
    artifacts: context.artifacts.length,
  });
}

function runCheckpointCommand(args) {
  const [rootArgument, phase, sequenceArgument] = args;
  requireArgument(rootArgument, 'checkpoint <root> <phase> <seq>');
  requireArgument(phase, 'checkpoint <root> <phase> <seq>');
  requireArgument(sequenceArgument, 'checkpoint <root> <phase> <seq>');

  const rootPath = resolveInputPath(rootArgument);
  const sequence = parseSequence(sequenceArgument);
  const validateResult = validateArtifacts(rootPath, null);
  const context = analyzeArtifacts(rootPath);
  const plan = computePlan(context);
  const timestamp = getCheckpointTimestamp();
  const fileName = `${phase}-${String(sequence).padStart(2, '0')}.json`;
  const outputPath = path.join(rootPath, 'checkpoints', fileName);

  fs.mkdirSync(path.dirname(outputPath), { recursive: true });

  const checkpoint = {
    id: `CP-${sanitizeToken(phase)}-${sequence}`,
    kind: 'construction-checkpoint',
    phase,
    seq: sequence,
    timestamp,
    createdAtUtc: timestamp,
    sourceRevision: process.env.VBD_SOURCE_REVISION || 'unknown',
    evidenceManifestHash: null,
    completedOutputs: [],
    invalidatedOutputs: [],
    unresolvedDecisions: [],
    nextReadyTasks: plan.nextReadyTasks,
    tokenBudget: {
      consumed: 0,
      remaining: null,
    },
    validationEvidence: buildCheckpointValidationEvidence(validateResult),
    validationSummary: buildValidationSummary(validateResult),
  };

  fs.writeFileSync(outputPath, `${JSON.stringify(checkpoint, null, 2)}\n`, 'utf8');
  process.stdout.write(`${toDisplayPath(outputPath)}\n`);
}

function parseValidateArguments(args) {
  const [rootArgument, ...rest] = args;
  requireArgument(rootArgument, 'validate <root> [--source-root <dir>]');

  let sourceRootPath = null;

  for (let index = 0; index < rest.length; index += 1) {
    const current = rest[index];

    if (current === '--source-root') {
      const value = rest[index + 1];
      requireArgument(value, 'validate <root> [--source-root <dir>]');
      sourceRootPath = resolveInputPath(value);
      index += 1;
      continue;
    }

    throw new Error(`Unsupported argument '${current}' for validate.`);
  }

  return {
    rootPath: resolveInputPath(rootArgument),
    sourceRootPath,
  };
}

function validateArtifacts(rootPath, sourceRootPath) {
  if (!fs.existsSync(rootPath)) {
    return {
      command: 'validate',
      root: toDisplayPath(rootPath),
      filesScanned: 0,
      errorCount: 0,
      warningCount: 0,
      errors: [],
      warnings: [],
      info: [`Root '${toDisplayPath(rootPath)}' does not exist. Validation skipped.`],
      artifacts: [],
      dependencyCycles: [],
      duplicateIds: [],
      unresolvedReferences: [],
      sourceChecksSkipped: !sourceRootPath || !fs.existsSync(sourceRootPath),
    };
  }

  const context = analyzeArtifacts(rootPath, { sourceRootPath });
  const errors = [];
  const warnings = [];
  const info = [];

  if (sourceRootPath && !fs.existsSync(sourceRootPath)) {
    info.push(`Source root '${toDisplayPath(sourceRootPath)}' does not exist. Source checks skipped.`);
  }

  for (const issue of context.parseErrors) {
    errors.push(`Parse error: ${toDisplayPath(issue.filePath)} - ${issue.message}`);
  }

  for (const issue of context.missingIds) {
    errors.push(`Missing id: ${toDisplayPath(issue.filePath)} must contain a string id field.`);
  }

  for (const duplicate of context.duplicateIds) {
    errors.push(
      `Duplicate id '${duplicate.id}' in ${duplicate.locations
        .map((location) => toDisplayPath(location))
        .join(', ')}`,
    );
  }

  for (const violation of context.schemaViolations) {
    errors.push(
      `Schema violation (${violation.kind}): ${toDisplayPath(violation.filePath)} - ${violation.message}`,
    );
  }

  for (const issue of context.unresolvedReferences) {
    warnings.push(
      `Unresolved reference '${issue.targetId}' from ${issue.sourceId} (${toDisplayPath(
        issue.filePath,
      )}${issue.fieldPath ? ` :: ${issue.fieldPath}` : ''})`,
    );
  }

  for (const issue of context.sourceWarnings) {
    warnings.push(
      `Source check warning: ${toDisplayPath(issue.filePath)} - ${issue.message}`,
    );
  }

  const dependencyCycles = detectDependencyCycles(context.dependencyEdges);

  for (const cycle of dependencyCycles) {
    errors.push(`Dependency cycle: ${cycle.join(' -> ')}`);
  }

  return {
    command: 'validate',
    root: toDisplayPath(rootPath),
    filesScanned: context.filesScanned,
    errorCount: errors.length,
    warningCount: warnings.length,
    errors,
    warnings,
    info,
    artifacts: context.artifacts,
    dependencyCycles,
    duplicateIds: context.duplicateIds,
    unresolvedReferences: context.unresolvedReferences,
    sourceChecksSkipped: Boolean(sourceRootPath && !fs.existsSync(sourceRootPath)),
  };
}

function emitValidationResult(result) {
  for (const message of result.info) {
    logInfo(message);
  }

  for (const message of result.warnings) {
    logWarning(message);
  }

  for (const message of result.errors) {
    logError(message);
  }

  logSummary(buildValidationSummary(result));
}

function analyzeArtifacts(rootPath, options = {}) {
  const sourceRootPath = options.sourceRootPath || null;
  const schemas = loadSchemas();
  const jsonFilePaths = listJsonFiles(rootPath);
  const artifacts = [];
  const parseErrors = [];
  const missingIds = [];
  const schemaViolations = [];
  const sourceWarnings = [];
  const idToArtifacts = new Map();

  for (const filePath of jsonFilePaths) {
    let rawContent;

    try {
      rawContent = fs.readFileSync(filePath, 'utf8');
    } catch (error) {
      parseErrors.push({
        filePath,
        message: error instanceof Error ? error.message : String(error),
      });
      continue;
    }

    let json;

    try {
      json = JSON.parse(rawContent);
    } catch (error) {
      parseErrors.push({
        filePath,
        message: error instanceof Error ? error.message : String(error),
      });
      continue;
    }

    if (!json || typeof json !== 'object' || typeof json.id !== 'string' || json.id.trim() === '') {
      missingIds.push({ filePath });
      continue;
    }

    const relativePath = path.relative(rootPath, filePath);
    const directoryName = getImmediateDirectoryName(relativePath);
    const kind = resolveArtifactKind(json, directoryName, schemas);
    const artifact = {
      id: json.id,
      kind,
      filePath,
      relativePath,
      directoryName,
      status: typeof json.status === 'string' ? json.status : 'unknown',
      data: json,
    };

    artifacts.push(artifact);

    if (!idToArtifacts.has(artifact.id)) {
      idToArtifacts.set(artifact.id, []);
    }

    idToArtifacts.get(artifact.id).push(artifact);

    if (kind && schemas.has(kind)) {
      const schema = schemas.get(kind);
      const violations = validateAgainstSchema(json, schema, schemas, '$');

      for (const violation of violations) {
        schemaViolations.push({
          filePath,
          kind,
          message: violation,
        });
      }
    }

    if (
      sourceRootPath &&
      fs.existsSync(sourceRootPath) &&
      json.source &&
      typeof json.source === 'object' &&
      typeof json.source.path === 'string' &&
      json.source.path.trim() !== ''
    ) {
      const candidatePath = path.resolve(sourceRootPath, json.source.path);

      if (!fs.existsSync(candidatePath)) {
        sourceWarnings.push({
          filePath,
          message: `Referenced source path '${json.source.path}' does not exist under '${toDisplayPath(
            sourceRootPath,
          )}'.`,
        });
      }
    }
  }

  const duplicateIds = [];

  for (const [id, matchingArtifacts] of idToArtifacts.entries()) {
    if (matchingArtifacts.length > 1) {
      duplicateIds.push({
        id,
        locations: matchingArtifacts.map((artifact) => artifact.filePath),
      });
    }
  }

  const knownIds = new Set(idToArtifacts.keys());
  const referenceEdges = [];
  const dependencyEdges = [];
  const unresolvedReferences = [];

  for (const artifact of artifacts) {
    const extraction = extractArtifactReferences(artifact.data);

    for (const reference of extraction.references) {
      const edge = {
        sourceId: artifact.id,
        targetId: reference.targetId,
        filePath: artifact.filePath,
        fieldPath: reference.fieldPath,
        relationship: reference.relationship,
      };

      referenceEdges.push(edge);

      if (!knownIds.has(reference.targetId)) {
        unresolvedReferences.push(edge);
      }
    }

    for (const dependency of extraction.dependencies) {
      const edge = {
        sourceId: artifact.id,
        targetId: dependency.targetId,
        filePath: artifact.filePath,
        fieldPath: dependency.fieldPath,
      };

      dependencyEdges.push(edge);
    }
  }

  return {
    rootPath,
    filesScanned: jsonFilePaths.length,
    artifacts,
    parseErrors,
    missingIds,
    duplicateIds,
    schemaViolations,
    unresolvedReferences,
    referenceEdges,
    dependencyEdges,
    sourceWarnings,
  };
}

function loadSchemas() {
  const schemas = new Map();

  if (!fs.existsSync(SCHEMA_DIRECTORY)) {
    return schemas;
  }

  const schemaFileNames = fs
    .readdirSync(SCHEMA_DIRECTORY, { withFileTypes: true })
    .filter((entry) => entry.isFile() && entry.name.endsWith('.schema.json'))
    .map((entry) => entry.name)
    .sort();

  for (const fileName of schemaFileNames) {
    const fullPath = path.join(SCHEMA_DIRECTORY, fileName);
    const content = fs.readFileSync(fullPath, 'utf8');
    const schema = JSON.parse(content);
    const kind = fileName.replace(/\.schema\.json$/i, '');
    schemas.set(kind, schema);
  }

  return schemas;
}

function listJsonFiles(rootPath) {
  if (!fs.existsSync(rootPath)) {
    return [];
  }

  const filePaths = [];
  const stack = [rootPath];

  while (stack.length > 0) {
    const currentPath = stack.pop();
    const entries = fs.readdirSync(currentPath, { withFileTypes: true });

    for (const entry of entries) {
      const fullPath = path.join(currentPath, entry.name);

      if (entry.isDirectory()) {
        stack.push(fullPath);
        continue;
      }

      if (entry.isFile() && entry.name.endsWith('.json')) {
        filePaths.push(fullPath);
      }
    }
  }

  filePaths.sort((left, right) => left.localeCompare(right));
  return filePaths;
}

function resolveArtifactKind(json, directoryName, schemas) {
  if (typeof json.kind === 'string' && schemas.has(json.kind)) {
    return json.kind;
  }

  if (DIRECTORY_KIND_MAP[directoryName] && schemas.has(DIRECTORY_KIND_MAP[directoryName])) {
    return DIRECTORY_KIND_MAP[directoryName];
  }

  return directoryName && DIRECTORY_KIND_MAP[directoryName]
    ? DIRECTORY_KIND_MAP[directoryName]
    : 'unknown';
}

function validateAgainstSchema(value, schema, schemaRegistry, pointer) {
  if (!schema || typeof schema !== 'object') {
    return [];
  }

  if (typeof schema.$ref === 'string') {
    const referencedSchema = resolveSchemaReference(schema.$ref, schema, schemaRegistry);
    if (!referencedSchema) {
      return [`${pointer} has unresolved schema reference '${schema.$ref}'.`];
    }

    return validateAgainstSchema(value, referencedSchema, schemaRegistry, pointer);
  }

  const violations = [];

  if (Object.prototype.hasOwnProperty.call(schema, 'const') && value !== schema.const) {
    violations.push(`${pointer} must equal ${JSON.stringify(schema.const)}.`);
    return violations;
  }

  if (schema.enum && Array.isArray(schema.enum) && !schema.enum.includes(value)) {
    violations.push(`${pointer} must be one of ${schema.enum.map((item) => JSON.stringify(item)).join(', ')}.`);
    return violations;
  }

  if (schema.type) {
    const allowedTypes = Array.isArray(schema.type) ? schema.type : [schema.type];
    const matchingType = allowedTypes.find((typeName) => matchesSchemaType(value, typeName));

    if (!matchingType) {
      violations.push(`${pointer} must be ${allowedTypes.join(' or ')}.`);
      return violations;
    }
  }

  if (typeof value === 'string') {
    if (typeof schema.minLength === 'number' && value.length < schema.minLength) {
      violations.push(`${pointer} must have length >= ${schema.minLength}.`);
    }

    if (typeof schema.pattern === 'string') {
      const expression = new RegExp(schema.pattern);
      if (!expression.test(value)) {
        violations.push(`${pointer} must match pattern ${schema.pattern}.`);
      }
    }

    if (schema.format === 'date-time' && Number.isNaN(Date.parse(value))) {
      violations.push(`${pointer} must be a valid date-time.`);
    }
  }

  if (typeof value === 'number') {
    if (typeof schema.minimum === 'number' && value < schema.minimum) {
      violations.push(`${pointer} must be >= ${schema.minimum}.`);
    }

    if (typeof schema.maximum === 'number' && value > schema.maximum) {
      violations.push(`${pointer} must be <= ${schema.maximum}.`);
    }
  }

  if (schema.type === 'integer' && !Number.isInteger(value)) {
    violations.push(`${pointer} must be an integer.`);
  }

  if (Array.isArray(value)) {
    if (typeof schema.minItems === 'number' && value.length < schema.minItems) {
      violations.push(`${pointer} must contain at least ${schema.minItems} item(s).`);
    }

    if (schema.uniqueItems === true) {
      const uniqueCount = new Set(value.map((item) => JSON.stringify(item))).size;
      if (uniqueCount !== value.length) {
        violations.push(`${pointer} must contain unique items.`);
      }
    }

    if (schema.items) {
      for (let index = 0; index < value.length; index += 1) {
        violations.push(
          ...validateAgainstSchema(
            value[index],
            schema.items,
            schemaRegistry,
            `${pointer}[${index}]`,
          ),
        );
      }
    }
  }

  if (value && typeof value === 'object' && !Array.isArray(value)) {
    if (Array.isArray(schema.required)) {
      for (const propertyName of schema.required) {
        if (!Object.prototype.hasOwnProperty.call(value, propertyName)) {
          violations.push(`${pointer} must include required property '${propertyName}'.`);
        }
      }
    }

    if (schema.properties && typeof schema.properties === 'object') {
      for (const [propertyName, propertySchema] of Object.entries(schema.properties)) {
        if (!Object.prototype.hasOwnProperty.call(value, propertyName)) {
          continue;
        }

        violations.push(
          ...validateAgainstSchema(
            value[propertyName],
            propertySchema,
            schemaRegistry,
            `${pointer}.${propertyName}`,
          ),
        );
      }
    }

    if (
      schema.additionalProperties &&
      typeof schema.additionalProperties === 'object' &&
      schema.properties &&
      typeof schema.properties === 'object'
    ) {
      for (const [propertyName, propertyValue] of Object.entries(value)) {
        if (Object.prototype.hasOwnProperty.call(schema.properties, propertyName)) {
          continue;
        }

        violations.push(
          ...validateAgainstSchema(
            propertyValue,
            schema.additionalProperties,
            schemaRegistry,
            `${pointer}.${propertyName}`,
          ),
        );
      }
    }
  }

  return violations;
}

function resolveSchemaReference(reference, schema, schemaRegistry) {
  if (reference.startsWith('#/')) {
    return resolveLocalReference(schema, reference);
  }

  const byFileName = reference
    .split('/')
    .pop()
    ?.replace(/\.schema\.json$/i, '')
    .replace(/\.json$/i, '');

  if (byFileName && schemaRegistry.has(byFileName)) {
    return schemaRegistry.get(byFileName);
  }

  return null;
}

function resolveLocalReference(schema, reference) {
  const segments = reference
    .replace(/^#\//, '')
    .split('/')
    .map((segment) => segment.replace(/~1/g, '/').replace(/~0/g, '~'));

  let current = schema;

  for (const segment of segments) {
    if (!current || typeof current !== 'object' || !Object.prototype.hasOwnProperty.call(current, segment)) {
      return null;
    }

    current = current[segment];
  }

  return current;
}

function matchesSchemaType(value, typeName) {
  switch (typeName) {
    case 'string':
      return typeof value === 'string';
    case 'number':
      return typeof value === 'number' && Number.isFinite(value);
    case 'integer':
      return Number.isInteger(value);
    case 'boolean':
      return typeof value === 'boolean';
    case 'array':
      return Array.isArray(value);
    case 'object':
      return value !== null && typeof value === 'object' && !Array.isArray(value);
    case 'null':
      return value === null;
    default:
      return true;
  }
}

function extractArtifactReferences(value) {
  const references = [];
  const dependencies = [];

  walkValue(value, '$', null, (currentValue, currentPath, keyName) => {
    if (typeof currentValue === 'string') {
      if (keyName === 'id') {
        return;
      }

      if (looksLikeArtifactId(currentValue) && looksLikeReferenceField(keyName)) {
        references.push({
          fieldPath: currentPath,
          targetId: currentValue,
          relationship: keyName || 'reference',
        });
      }

      return;
    }

    if (!Array.isArray(currentValue)) {
      return;
    }

    const candidateValues = currentValue.filter((item) => typeof item === 'string' && looksLikeArtifactId(item));

    if (candidateValues.length === 0) {
      return;
    }

    const isReferenceArray =
      looksLikeReferenceField(keyName) ||
      REFERENCE_FIELD_PATTERN.test(keyName || '') ||
      candidateValues.length > 0;

    if (!isReferenceArray) {
      return;
    }

    for (const targetId of candidateValues) {
      const reference = {
        fieldPath: currentPath,
        targetId,
        relationship: keyName || 'reference',
      };

      references.push(reference);

      if (DEPENDENCY_FIELD_PATTERN.test(keyName || '')) {
        dependencies.push(reference);
      }
    }
  });

  return { references, dependencies };
}

function walkValue(value, currentPath, keyName, visitor) {
  visitor(value, currentPath, keyName);

  if (Array.isArray(value)) {
    for (let index = 0; index < value.length; index += 1) {
      walkValue(value[index], `${currentPath}[${index}]`, keyName, visitor);
    }
    return;
  }

  if (!value || typeof value !== 'object') {
    return;
  }

  for (const [childKey, childValue] of Object.entries(value)) {
    walkValue(childValue, `${currentPath}.${childKey}`, childKey, visitor);
  }
}

function looksLikeReferenceField(fieldName) {
  if (!fieldName) {
    return false;
  }

  return (
    REFERENCE_FIELD_PATTERN.test(fieldName) ||
    /(Ids?|Cases?|Symbols?|Tasks?|WaveId|CandidateId|ComponentId|Manifest|callers|callees)/i.test(
      fieldName,
    )
  );
}

function looksLikeArtifactId(value) {
  if (typeof value !== 'string') {
    return false;
  }

  return ID_LIKE_PATTERN.test(value) && /-/.test(value);
}

function detectDependencyCycles(edges) {
  const adjacency = new Map();

  for (const edge of edges) {
    if (!adjacency.has(edge.sourceId)) {
      adjacency.set(edge.sourceId, []);
    }

    adjacency.get(edge.sourceId).push(edge.targetId);

    if (!adjacency.has(edge.targetId)) {
      adjacency.set(edge.targetId, []);
    }
  }

  const visited = new Set();
  const active = new Set();
  const stack = [];
  const cycles = [];
  const seenCycleKeys = new Set();

  function visit(nodeId) {
    if (active.has(nodeId)) {
      const startIndex = stack.indexOf(nodeId);
      const cycle = stack.slice(startIndex).concat(nodeId);
      const key = normalizeCycleKey(cycle);

      if (!seenCycleKeys.has(key)) {
        seenCycleKeys.add(key);
        cycles.push(cycle);
      }

      return;
    }

    if (visited.has(nodeId)) {
      return;
    }

    visited.add(nodeId);
    active.add(nodeId);
    stack.push(nodeId);

    const neighbors = adjacency.get(nodeId) || [];
    for (const neighbor of neighbors) {
      visit(neighbor);
    }

    stack.pop();
    active.delete(nodeId);
  }

  for (const nodeId of adjacency.keys()) {
    visit(nodeId);
  }

  return cycles;
}

function normalizeCycleKey(cycle) {
  const body = cycle.slice(0, -1);

  if (body.length === 0) {
    return '';
  }

  let best = null;

  for (let index = 0; index < body.length; index += 1) {
    const rotated = body.slice(index).concat(body.slice(0, index));
    const candidate = rotated.concat(rotated[0]).join('>');
    if (best === null || candidate < best) {
      best = candidate;
    }
  }

  return best || cycle.join('>');
}

function computePlan(context) {
  const nodes = new Map();
  const resolvedEdges = [];
  const unresolvedDependencies = [];

  for (const artifact of context.artifacts) {
    const extraction = extractArtifactReferences(artifact.data);
    const dependencyIds = extraction.dependencies.map((item) => item.targetId);

    if (dependencyIds.length === 0 && artifact.kind !== 'construction-task' && artifact.kind !== 'cutover-wave') {
      continue;
    }

    if (!nodes.has(artifact.id)) {
      nodes.set(artifact.id, {
        artifact,
        dependencies: new Set(),
        dependents: new Set(),
      });
    }

    for (const dependencyId of dependencyIds) {
      if (!nodes.has(artifact.id)) {
        nodes.set(artifact.id, {
          artifact,
          dependencies: new Set(),
          dependents: new Set(),
        });
      }

      if (context.artifacts.some((candidate) => candidate.id === dependencyId)) {
        resolvedEdges.push([artifact.id, dependencyId]);
        nodes.get(artifact.id).dependencies.add(dependencyId);
        continue;
      }

      unresolvedDependencies.push({
        sourceId: artifact.id,
        targetId: dependencyId,
      });
    }
  }

  for (const [sourceId, targetId] of resolvedEdges) {
    if (!nodes.has(targetId)) {
      const targetArtifact = context.artifacts.find((artifact) => artifact.id === targetId);
      if (targetArtifact) {
        nodes.set(targetId, {
          artifact: targetArtifact,
          dependencies: new Set(),
          dependents: new Set(),
        });
      }
    }

    if (nodes.has(targetId)) {
      nodes.get(targetId).dependents.add(sourceId);
    }
  }

  const remaining = new Map();

  for (const [id, node] of nodes.entries()) {
    remaining.set(id, {
      artifact: node.artifact,
      dependencies: new Set(node.dependencies),
      dependents: new Set(node.dependents),
      missingDependencies: unresolvedDependencies
        .filter((item) => item.sourceId === id)
        .map((item) => item.targetId),
    });
  }

  const waves = [];

  while (remaining.size > 0) {
    const readyIds = [...remaining.entries()]
      .filter(([, node]) => node.dependencies.size === 0 && node.missingDependencies.length === 0)
      .map(([id]) => id)
      .sort((left, right) => left.localeCompare(right));

    if (readyIds.length === 0) {
      break;
    }

    waves.push(readyIds);

    for (const readyId of readyIds) {
      remaining.delete(readyId);
    }

    for (const node of remaining.values()) {
      for (const readyId of readyIds) {
        node.dependencies.delete(readyId);
      }
    }
  }

  const blockedByMissingDependencies = [...remaining.entries()]
    .map(([id, node]) => ({
      id,
      missingDependencies: node.missingDependencies,
      remainingDependencies: [...node.dependencies].sort((left, right) => left.localeCompare(right)),
    }))
    .sort((left, right) => left.id.localeCompare(right.id));

  const nextReadyTasks = waves[0]
    ? waves[0].filter((id) => id.startsWith('TASK-'))
    : [];

  return {
    waves,
    blockedByMissingDependencies,
    nextReadyTasks,
  };
}

function computeScoreReport(context) {
  const entries = context.artifacts
    .filter((artifact) => artifact.kind === 'candidate-estimate')
    .map((artifact) => scoreCandidateEstimate(artifact))
    .sort((left, right) => right.score - left.score || left.id.localeCompare(right.id));

  return {
    weights: {
      confidence: '40%',
      effort: '35%',
      dependencyLoad: '25%',
    },
    entries,
  };
}

function scoreCandidateEstimate(artifact) {
  const data = artifact.data;
  const confidenceScore = CONFIDENCE_WEIGHTS[data.confidence] ?? 50;
  const expectedHours = getExpectedRangeValue(data.personHours);
  const expectedCalendarDays = getExpectedRangeValue(data.calendarDays);
  const dependencyCount = Array.isArray(data.dependencies) ? data.dependencies.length : 0;
  const implementationTokens = getExpectedRangeValue(data.implementationTokens);

  const effortBase = Math.max(
    0,
    100 - expectedHours * 2 - expectedCalendarDays * 3 - implementationTokens / 400,
  );
  const dependencyScore = Math.max(0, 100 - dependencyCount * 15);
  const weightedScore = Math.round(confidenceScore * 0.4 + effortBase * 0.35 + dependencyScore * 0.25);

  return {
    id: artifact.id,
    candidateId: data.candidateId,
    score: weightedScore,
    rationale: `confidence=${data.confidence}; expectedHours=${expectedHours}; dependencies=${dependencyCount}`,
  };
}

function renderGraphMarkdown(rootPath, context) {
  const lines = [];
  lines.push(`# VBD Graph`);
  lines.push('');
  lines.push(`Root: \`${toDisplayPath(rootPath)}\``);
  lines.push('');
  lines.push('```mermaid');
  lines.push('graph TD');

  const declaredNodes = new Set();
  const orphanNodes = new Set();

  for (const artifact of context.artifacts) {
    lines.push(`  ${toMermaidId(artifact.id)}["${escapeMermaidLabel(artifact.id)}"]`);
    declaredNodes.add(artifact.id);
  }

  for (const edge of context.referenceEdges) {
    const sourceNode = toMermaidId(edge.sourceId);
    const targetNode = toMermaidId(edge.targetId);

    if (!declaredNodes.has(edge.targetId)) {
      lines.push(`  ${targetNode}["${escapeMermaidLabel(`${edge.targetId} (orphan)`)}"]`);
      orphanNodes.add(edge.targetId);
      declaredNodes.add(edge.targetId);
    }

    lines.push(`  ${sourceNode} --> ${targetNode}`);
  }

  for (const orphanId of orphanNodes) {
    lines.push(`  class ${toMermaidId(orphanId)} orphan`);
  }

  if (orphanNodes.size > 0) {
    lines.push('  classDef orphan fill:#fff3cd,stroke:#d97706,stroke-width:2px;');
    lines.push('  %% orphan nodes represent unresolved references');
  }

  lines.push('```');
  lines.push('');

  return `${lines.join('\n')}\n`;
}

function renderPlanMarkdown(rootPath, plan) {
  const lines = [];
  lines.push(`# VBD Plan Waves`);
  lines.push('');
  lines.push(`Root: \`${toDisplayPath(rootPath)}\``);
  lines.push('');

  if (plan.waves.length === 0) {
    lines.push('No dependency waves found.');
  } else {
    for (let index = 0; index < plan.waves.length; index += 1) {
      lines.push(`${index + 1}. Wave ${index + 1}: ${plan.waves[index].join(', ')}`);
    }
  }

  if (plan.blockedByMissingDependencies.length > 0) {
    lines.push('');
    lines.push('Blocked items:');
    for (const blocked of plan.blockedByMissingDependencies) {
      const missingText = blocked.missingDependencies.length > 0
        ? ` missing=[${blocked.missingDependencies.join(', ')}]`
        : '';
      const remainingText = blocked.remainingDependencies.length > 0
        ? ` remaining=[${blocked.remainingDependencies.join(', ')}]`
        : '';
      lines.push(`- ${blocked.id}${missingText}${remainingText}`);
    }
  }

  lines.push('');
  return `${lines.join('\n')}\n`;
}

function renderScoreMarkdown(rootPath, report) {
  const lines = [];
  lines.push('# VBD Advisory Score');
  lines.push('');
  lines.push(`Root: \`${toDisplayPath(rootPath)}\``);
  lines.push('');
  lines.push(
    `Weights: confidence ${report.weights.confidence}, effort ${report.weights.effort}, dependencyLoad ${report.weights.dependencyLoad}.`,
  );
  lines.push('');
  lines.push('| Artifact ID | Candidate | Score | Rationale |');
  lines.push('|---|---|---:|---|');

  if (report.entries.length === 0) {
    lines.push('| _none_ |  | 0 | No candidate-estimate artifacts found. |');
  } else {
    for (const entry of report.entries) {
      lines.push(
        `| ${entry.id} | ${entry.candidateId} | ${entry.score} | ${escapeMarkdownTable(
          entry.rationale,
        )} |`,
      );
    }
  }

  lines.push('');
  return `${lines.join('\n')}\n`;
}

function renderDashboardMarkdown(rootPath, context) {
  const countsByKind = new Map();
  const countsByStatus = new Map();

  for (const artifact of context.artifacts) {
    incrementCount(countsByKind, artifact.kind || 'unknown');
    incrementCount(countsByStatus, artifact.status || 'unknown');
  }

  const lines = [];
  lines.push('# VBD Dashboard');
  lines.push('');
  lines.push(`Root: \`${toDisplayPath(rootPath)}\``);
  lines.push('');
  lines.push(`Artifacts scanned: ${context.artifacts.length}`);
  lines.push('');
  lines.push('## By kind');
  lines.push('');
  lines.push('| Kind | Count |');
  lines.push('|---|---:|');
  appendCountRows(lines, countsByKind);
  lines.push('');
  lines.push('## By status');
  lines.push('');
  lines.push('| Status | Count |');
  lines.push('|---|---:|');
  appendCountRows(lines, countsByStatus);
  lines.push('');

  return `${lines.join('\n')}\n`;
}

function buildCheckpointValidationEvidence(validateResult) {
  return [
    {
      check: 'files-scanned',
      status: 'pass',
      detail: `Scanned ${validateResult.filesScanned} file(s).`,
    },
    {
      check: 'blocking-failures',
      status: validateResult.errorCount === 0 ? 'pass' : 'fail',
      detail: `${validateResult.errorCount} blocking failure(s).`,
    },
    {
      check: 'warnings',
      status: validateResult.warningCount === 0 ? 'pass' : 'warn',
      detail: `${validateResult.warningCount} warning(s).`,
    },
  ];
}

function buildValidationSummary(validateResult) {
  return {
    command: 'validate',
    root: validateResult.root,
    filesScanned: validateResult.filesScanned,
    errors: validateResult.errorCount,
    warnings: validateResult.warningCount,
    sourceChecksSkipped: Boolean(validateResult.sourceChecksSkipped),
  };
}

function incrementCount(map, key) {
  map.set(key, (map.get(key) || 0) + 1);
}

function appendCountRows(lines, counts) {
  const entries = [...counts.entries()].sort(([left], [right]) => left.localeCompare(right));

  if (entries.length === 0) {
    lines.push('| _none_ | 0 |');
    return;
  }

  for (const [key, count] of entries) {
    lines.push(`| ${escapeMarkdownTable(key)} | ${count} |`);
  }
}

function getExpectedRangeValue(value) {
  if (!value || typeof value !== 'object') {
    return 0;
  }

  return typeof value.expected === 'number' ? value.expected : 0;
}

function parseSequence(value) {
  const sequence = Number.parseInt(value, 10);

  if (!Number.isInteger(sequence) || sequence < 0) {
    throw new Error(`Sequence '${value}' must be a non-negative integer.`);
  }

  return sequence;
}

function getCheckpointTimestamp() {
  const injected = process.env.VBD_CHECKPOINT_CLOCK;

  if (typeof injected === 'string' && injected.trim() !== '') {
    if (Number.isNaN(Date.parse(injected))) {
      throw new Error(`VBD_CHECKPOINT_CLOCK '${injected}' is not a valid ISO timestamp.`);
    }

    return injected;
  }

  return new Date().toISOString();
}

function sanitizeToken(value) {
  return String(value)
    .replace(/[^A-Za-z0-9-]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .replace(/-{2,}/g, '-');
}

function getImmediateDirectoryName(relativePath) {
  const normalized = relativePath.split(path.sep).filter((segment) => segment.length > 0);
  return normalized.length > 1 ? normalized[0] : '';
}

function resolveInputPath(inputPath) {
  return path.resolve(process.cwd(), inputPath);
}

function ensureParentDirectory(filePath) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
}

function requireArgument(value, usage) {
  if (typeof value === 'string' && value.trim() !== '') {
    return;
  }

  throw new Error(`Usage: node scripts/vbd-artifacts.js ${usage}`);
}

function toDisplayPath(filePath) {
  return filePath.replace(/\//g, '\\');
}

function toMermaidId(id) {
  return `node_${id.replace(/[^A-Za-z0-9]/g, '_')}`;
}

function escapeMermaidLabel(value) {
  return String(value).replace(/"/g, '\\"');
}

function escapeMarkdownTable(value) {
  return String(value).replace(/\|/g, '\\|');
}

function logInfo(message) {
  process.stdout.write(`INFO ${message}\n`);
}

function logWarning(message) {
  process.stdout.write(`WARN ${message}\n`);
}

function logError(message) {
  process.stdout.write(`ERROR ${message}\n`);
}

function logSummary(payload) {
  process.stdout.write(`SUMMARY ${JSON.stringify(payload)}\n`);
}

function printUsage() {
  const usageLines = [
    'Usage: node scripts/vbd-artifacts.js <command> [...args]',
    'Commands:',
    '  init <root>',
    '  validate <root> [--source-root <dir>]',
    '  graph <root> <out.md>',
    '  plan <root> [out.md]',
    '  score <root> [out.md]',
    '  dashboard <root> <out.md>',
    '  checkpoint <root> <phase> <seq>',
  ];

  process.stdout.write(`${usageLines.join('\n')}\n`);
}

if (require.main === module) {
  main();
}

module.exports = {
  STANDARD_DIRECTORIES,
  analyzeArtifacts,
  buildValidationSummary,
  computePlan,
  computeScoreReport,
  detectDependencyCycles,
  extractArtifactReferences,
  renderDashboardMarkdown,
  renderGraphMarkdown,
  renderPlanMarkdown,
  renderScoreMarkdown,
  validateArtifacts,
};
