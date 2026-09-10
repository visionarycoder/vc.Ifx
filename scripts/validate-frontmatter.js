const assert = require('node:assert');
const fs = require('node:fs');
const path = require('node:path');

const ALLOWED_DOC_TYPES = new Set([
    'readme',
    'guide',
    'reference',
    'runbook',
    'plan',
    'report',
    'policy',
    'skill',
    'prompt',
    'instruction',
    'archive'
]);

const ALLOWED_STATUSES = new Set([
    'draft',
    'active',
    'deprecated',
    'archived'
]);

const REQUIRED_PROPERTIES = [
    'title',
    'doc_type',
    'status',
    'last_updated'
];

const EXCLUDED_DIRECTORY_NAMES = new Set([
    'node_modules',
    'bin',
    'obj'
]);

function stripByteOrderMark(text) {
    return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function extractFrontMatterBlock(markdown) {
    const content = stripByteOrderMark(markdown);
    const match = content.match(/^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/);

    return match ? match[1] : null;
}

function parseScalar(value) {
    const trimmedValue = value.trim();

    if (
        trimmedValue.length >= 2
        && ((trimmedValue.startsWith('"') && trimmedValue.endsWith('"'))
            || (trimmedValue.startsWith('\'') && trimmedValue.endsWith('\'')))
    ) {
        return trimmedValue.slice(1, -1);
    }

    return trimmedValue;
}

function parseFrontMatter(frontMatterBlock) {
    const metadata = {};
    let currentListKey = null;

    for (const rawLine of frontMatterBlock.split(/\r?\n/)) {
        const line = rawLine.trimEnd();

        if (/^\s*$/.test(line) || /^\s*#/.test(line)) {
            continue;
        }

        const listMatch = line.match(/^\s*-\s+(.*)$/);
        if (listMatch && currentListKey !== null) {
            if (!Array.isArray(metadata[currentListKey])) {
                metadata[currentListKey] = [];
            }

            metadata[currentListKey].push(parseScalar(listMatch[1]));
            continue;
        }

        const propertyMatch = line.match(/^([A-Za-z0-9_]+):(?:\s*(.*))?$/);
        if (propertyMatch) {
            const key = propertyMatch[1];
            const rawValue = propertyMatch[2] ?? '';

            metadata[key] = rawValue === '' ? '' : parseScalar(rawValue);
            currentListKey = key;
            continue;
        }

        currentListKey = null;
    }

    return metadata;
}

function isNonEmptyString(value) {
    return typeof value === 'string' && value.trim().length > 0;
}

function isValidCalendarDate(dateText) {
    if (!/^\d{4}-\d{2}-\d{2}$/.test(dateText)) {
        return false;
    }

    const [yearText, monthText, dayText] = dateText.split('-');
    const year = Number(yearText);
    const month = Number(monthText);
    const day = Number(dayText);

    if (!Number.isInteger(year) || !Number.isInteger(month) || !Number.isInteger(day)) {
        return false;
    }

    const candidate = new Date(Date.UTC(year, month - 1, day));

    return candidate.getUTCFullYear() === year
        && candidate.getUTCMonth() === month - 1
        && candidate.getUTCDate() === day;
}

function validateMetadata(metadata, violations) {
    for (const propertyName of REQUIRED_PROPERTIES) {
        if (!isNonEmptyString(metadata[propertyName])) {
            violations.push(`missing required property: ${propertyName}`);
        }
    }

    if (isNonEmptyString(metadata.doc_type) && !ALLOWED_DOC_TYPES.has(metadata.doc_type)) {
        violations.push(`invalid doc_type value: ${metadata.doc_type}`);
    }

    if (isNonEmptyString(metadata.status) && !ALLOWED_STATUSES.has(metadata.status)) {
        violations.push(`invalid status value: ${metadata.status}`);
    }

    if (isNonEmptyString(metadata.last_updated) && !isValidCalendarDate(metadata.last_updated)) {
        violations.push(`invalid last_updated value: ${metadata.last_updated}`);
    }
}

function validateMarkdownContent(markdown) {
    const frontMatterBlock = extractFrontMatterBlock(markdown);

    if (frontMatterBlock === null) {
        return ['missing front matter'];
    }

    const metadata = parseFrontMatter(frontMatterBlock);
    const violations = [];
    validateMetadata(metadata, violations);

    return violations;
}

function isExcludedPathSegment(name) {
    return EXCLUDED_DIRECTORY_NAMES.has(name.toLowerCase());
}

function collectMarkdownFiles(rootDirectory) {
    const markdownFiles = [];

    if (!fs.existsSync(rootDirectory)) {
        return markdownFiles;
    }

    const stack = [rootDirectory];

    while (stack.length > 0) {
        const currentDirectory = stack.pop();
        const directoryEntries = fs.readdirSync(currentDirectory, { withFileTypes: true });

        for (const entry of directoryEntries) {
            if (isExcludedPathSegment(entry.name)) {
                continue;
            }

            const fullPath = path.join(currentDirectory, entry.name);

            if (entry.isDirectory()) {
                stack.push(fullPath);
                continue;
            }

            if (entry.isFile() && entry.name.toLowerCase().endsWith('.md')) {
                markdownFiles.push(fullPath);
            }
        }
    }

    markdownFiles.sort((left, right) => left.localeCompare(right));
    return markdownFiles;
}

function validateRepository(repoRoot) {
    const rootsToScan = [
        path.join(repoRoot, '.github'),
        path.join(repoRoot, 'docs')
    ];

    const files = rootsToScan.flatMap(collectMarkdownFiles);
    const violationsByFile = [];

    for (const filePath of files) {
        const relativePath = path.relative(repoRoot, filePath);

        try {
            const content = fs.readFileSync(filePath, 'utf8');
            const violations = validateMarkdownContent(content);

            if (violations.length > 0) {
                violationsByFile.push({
                    filePath: relativePath,
                    violations
                });
            }
        }
        catch (error) {
            violationsByFile.push({
                filePath: relativePath,
                violations: [`unable to read file: ${error.message}`]
            });
        }
    }

    return {
        filesScanned: files.length,
        violationsByFile
    };
}

function printValidationReport(result) {
    const totalViolations = result.violationsByFile.reduce(
        (count, entry) => count + entry.violations.length,
        0
    );

    console.log('Front matter validation report');
    console.log(`Files scanned: ${result.filesScanned}`);

    if (result.violationsByFile.length === 0) {
        console.log('Violations: none');
        console.log(`Summary: 0 violations across ${result.filesScanned} files.`);
        return 0;
    }

    console.log('Violations:');
    for (const entry of result.violationsByFile) {
        console.log(`- ${entry.filePath}`);
        for (const violation of entry.violations) {
            console.log(`  - ${violation}`);
        }
    }

    console.log(
        `Summary: ${result.violationsByFile.length} file(s) with ${totalViolations} violation(s).`
    );

    return 1;
}

function buildSelfTestCases() {
    return [
        {
            name: 'valid front matter passes',
            markdown: [
                '---',
                'title: Front Matter Standard',
                'doc_type: policy',
                'status: active',
                'last_updated: 2026-08-29',
                '---',
                '# Front Matter Standard'
            ].join('\n'),
            expectedViolations: []
        },
        {
            name: 'missing front matter is flagged',
            markdown: '# No Front Matter',
            expectedViolations: ['missing front matter']
        },
        {
            name: 'missing title is flagged',
            markdown: [
                '---',
                'doc_type: policy',
                'status: active',
                'last_updated: 2026-08-29',
                '---',
                '# Missing Title'
            ].join('\n'),
            expectedViolations: ['missing required property: title']
        },
        {
            name: 'missing doc_type is flagged',
            markdown: [
                '---',
                'title: Missing Doc Type',
                'status: active',
                'last_updated: 2026-08-29',
                '---',
                '# Missing Doc Type'
            ].join('\n'),
            expectedViolations: ['missing required property: doc_type']
        },
        {
            name: 'missing status is flagged',
            markdown: [
                '---',
                'title: Missing Status',
                'doc_type: policy',
                'last_updated: 2026-08-29',
                '---',
                '# Missing Status'
            ].join('\n'),
            expectedViolations: ['missing required property: status']
        },
        {
            name: 'missing last_updated is flagged',
            markdown: [
                '---',
                'title: Missing Last Updated',
                'doc_type: policy',
                'status: active',
                '---',
                '# Missing Last Updated'
            ].join('\n'),
            expectedViolations: ['missing required property: last_updated']
        },
        {
            name: 'invalid doc_type is flagged',
            markdown: [
                '---',
                'title: Invalid Doc Type',
                'doc_type: memo',
                'status: active',
                'last_updated: 2026-08-29',
                '---',
                '# Invalid Doc Type'
            ].join('\n'),
            expectedViolations: ['invalid doc_type value: memo']
        },
        {
            name: 'invalid status is flagged',
            markdown: [
                '---',
                'title: Invalid Status',
                'doc_type: policy',
                'status: enabled',
                'last_updated: 2026-08-29',
                '---',
                '# Invalid Status'
            ].join('\n'),
            expectedViolations: ['invalid status value: enabled']
        },
        {
            name: 'malformed last_updated format is flagged',
            markdown: [
                '---',
                'title: Invalid Date Format',
                'doc_type: policy',
                'status: active',
                'last_updated: 08-29-2026',
                '---',
                '# Invalid Date Format'
            ].join('\n'),
            expectedViolations: ['invalid last_updated value: 08-29-2026']
        },
        {
            name: 'non calendar last_updated is flagged',
            markdown: [
                '---',
                'title: Invalid Calendar Date',
                'doc_type: policy',
                'status: active',
                'last_updated: 2026-02-30',
                '---',
                '# Invalid Calendar Date'
            ].join('\n'),
            expectedViolations: ['invalid last_updated value: 2026-02-30']
        },
        {
            name: 'valid optional extra fields pass',
            markdown: [
                '---',
                'title: Optional Metadata',
                'doc_type: guide',
                'status: draft',
                'last_updated: 2026-09-10',
                'tags:',
                '  - docs',
                '  - metadata',
                'owner: Platform',
                'summary: Preserves optional fields.',
                '---',
                '# Optional Metadata'
            ].join('\n'),
            expectedViolations: []
        }
    ];
}

function runSelfTest() {
    const testCases = buildSelfTestCases();
    let passedCases = 0;

    console.log('Front matter validator self-test');

    for (const testCase of testCases) {
        try {
            const actualViolations = validateMarkdownContent(testCase.markdown);
            assert.deepStrictEqual(actualViolations, testCase.expectedViolations);
            passedCases += 1;
            console.log(`[PASS] ${testCase.name}`);
        }
        catch (error) {
            console.error(`[FAIL] ${testCase.name}`);
            console.error(`  ${error.message}`);
        }
    }

    const failedCases = testCases.length - passedCases;
    console.log(`Summary: ${passedCases} passed, ${failedCases} failed.`);

    return failedCases === 0 ? 0 : 1;
}

function main() {
    const argumentsSet = new Set(process.argv.slice(2));
    const isSelfTestMode = argumentsSet.has('--self-test') || argumentsSet.has('self-test');

    return isSelfTestMode
        ? runSelfTest()
        : printValidationReport(validateRepository(process.cwd()));
}

try {
    process.exitCode = main();
}
catch (error) {
    console.error(`Front matter validator failed: ${error.message}`);
    process.exitCode = 1;
}
