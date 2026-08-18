# regexFinder

`regexFinder` is a Windows desktop application that converts cash-register text exports into structured CSV files. Extraction rules are defined in YAML, so receipt boundaries, blocks, fields, and aggregation behavior can be adapted without changing the application code.

The application also includes tools for validating generated CSV files, exporting failed receipts, and injecting manually corrected rows back into a complete CSV.

## Features

- Split a text export into receipts with a configurable `Splitter` regex.
- Restrict extraction patterns to repeated receipt blocks.
- Extract single-line or multiline values.
- Aggregate matches with `first`, `last`, `sum`, or `merge`.
- Normalize configured numeric values to invariant decimal notation.
- Export one quoted UTF-8 CSV row per receipt.
- Validate required fields, formulas, hash chains, number sequences, and cumulative totals.
- Export failed CSV rows together with their original TXT receipt blocks.
- Replace corrected rows in a complete CSV without overwriting the master CSV.

## Requirements

To run the application:

- Windows 10 or later.
- .NET 8 Desktop Runtime.

To build the application:

- .NET 8 SDK.
- Optional: Visual Studio 2022 with the .NET desktop development workload.

The project can be compiled on a non-Windows host because `EnableWindowsTargeting` is enabled, but the WinForms application itself runs on Windows.

## Build And Run

Run these commands from the repository root:

```bash
dotnet build regexFinder/regexFinder.sln --configuration Release
dotnet run --project regexFinder/regexFinder.csproj
```

Release output is written to:

```text
regexFinder/bin/Release/net8.0-windows/
```

The application is framework-dependent. Keep the complete output directory together and install the .NET 8 Desktop Runtime on the target machine.

## Quick Start

1. Start the application.
2. Leave `UTF8` selected for UTF-8 input, or clear it to force Windows-1257.
3. Click `Upload cash register bills` and select the source TXT file.
4. Click `Upload Regex commands` and select an extraction YAML file.
5. Click `Transform`.
6. Select the destination CSV path after processing completes.

With `UTF8` selected, the loader first attempts strict UTF-8 decoding and falls back to Windows-1257 only when UTF-8 decoding fails. This is not general encoding detection.

Use [`regexFinder/blueprint_blocks_template.yaml`](regexFinder/blueprint_blocks_template.yaml) as a starting point. The working `regexFinder/blueprint.yaml` is intentionally local and is not tracked by Git.

## Extraction Configuration

An extraction YAML contains optional `blocks` and a `patterns` list. A usable pattern named `Splitter` is required for transformation.

```yaml
blocks:
  - name: body
    startsWith: '^N B\b'
    endsWith: '^N s\b'

patterns:
  - name: Splitter
    regexCommand: '^N H\s+COMPANY\b'
    valueType: string
    combineMethod: first
    multiline: false
    linesCount: 1

  - name: Receipt number
    regexCommand: 'RECEIPT\s+(\d+)'
    valueType: integer
    combineMethod: first

  - name: Final total
    blockName: body
    regexCommand: 'TOTAL\s*:?[ ]*([+-]?[0-9]+(?:[.,][0-9]+)?)'
    valueType: decimal
    combineMethod: last
    multiline: true
    linesCount: 2
```

### Splitter

`Splitter` is a reserved pattern name.

- Every matching source line starts a new receipt.
- The final receipt continues to the end of the file.
- Lines before the first splitter match are ignored.
- The splitter is not included as a CSV column.
- Matching is performed against the original, untrimmed source line.
- Transformation fails if no source line matches the splitter.

### Blocks

A block limits a pattern to a named range inside each receipt.

| Property | Description |
| --- | --- |
| `name` | Block name referenced by `blockName`. |
| `startsWith` | Start boundary, included in the block. |
| `endsWith` | End boundary, included in the block. |

A block may occur multiple times in one receipt. Values from every occurrence are combined according to the pattern's `combineMethod`. If a start boundary is found without an end boundary, the block extends to the end of the current receipt.

Block boundary matching is case-insensitive and uses untrimmed lines. A boundary containing regex metacharacters is treated as a regex; otherwise it is treated as an exact whole-line value.

### Patterns

| Property | Description |
| --- | --- |
| `name` | CSV column name. Use a unique, nonempty value. |
| `regexCommand` | .NET regular expression used to find values. |
| `blockName` | Optional block restriction. Without it, the entire receipt is searched. |
| `valueType` | `decimal` and `integer` replace decimal commas with dots. Other values are not parsed or validated. |
| `combineMethod` | `first`, `last`, `sum`, or `merge`. Defaults to `first`. |
| `multiline` | Enables sliding multiline windows when `linesCount` is greater than one. |
| `linesCount` | Maximum number of lines in each multiline window. |
| `distinctValues` | For `sum`, removes equal parsed numbers before aggregation. |

Pattern matching is case-sensitive unless the regex contains an inline option such as `(?i)`. Normal pattern matching trims each source line first.

If a regex contains capture groups, capture group 1 becomes the extracted value. Additional capture groups are ignored. Without a capture group, the complete match is used.

Unknown YAML properties are ignored. Check property names carefully because a typo may load successfully but have no effect. Regex compilation uses a two-second timeout.

### Combining Matches

| Method | Result |
| --- | --- |
| `first` | First nonempty match. |
| `last` | Last nonempty match. Useful when a final total follows intermediate totals. |
| `sum` | Sum of parsed numeric matches, formatted with two decimal places. Unparseable matches are skipped. |
| `merge` | Exact distinct text matches joined with `; `. |

For negative adjustments, the regex must capture the sign:

```yaml
regexCommand: 'AMOUNT\s+([+-]?[0-9]+(?:[.,][0-9]+)?)'
combineMethod: sum
```

Use `distinctValues` only when the same numeric value is repeated in separate sections of one report. Do not use it for independent payments because two payments with the same amount would collapse into one value.

### Multiline Matching

Multiline patterns join trimmed lines with one space. A window starts at every line but never crosses the selected block or receipt boundary. A match is counted only in the window where its full match starts on the first source line, preventing duplicates from overlapping windows.

## CSV Output

- Column order follows YAML pattern order, excluding `Splitter`.
- Every header and value is double-quoted.
- Embedded quotes are doubled.
- The delimiter is a comma.
- Output encoding is UTF-8.
- Numeric `sum` output uses a decimal dot and exactly two decimal places.

## CSV Validation Tests

Click `Tests` to open the `CSV Validation Tests` window. Validation operates on the selected CSV; it does not rerun extraction or compare the CSV with a newly generated result.

Load the extraction YAML before opening the test window because field selectors are populated from YAML pattern names. Load the original TXT before opening the window if original receipt blocks must be included in failure exports.

Checks can be saved to and loaded from YAML. Existing checks cannot be edited in place; delete and recreate a check to change it.

### Required Field

Fails every row where the selected field is empty. This check does not filter receipt types or canceled receipts.

### Compare Fields

Checks a formula in this form:

```text
left = sum(added fields) - sum(subtracted fields)
```

Configuration uses:

- `Field to compare` for the left side.
- `+ Add field` for added terms.
- `- Remove field` for subtracted terms.
- `Receipt types to use` for the rows that participate.
- `Tolerance` for the permitted absolute difference.

At least one receipt type must be selected. Receipt types are read from the fixed `Ceka tips` column and compared case-insensitively. Rows of all other types are skipped.

Canceled receipts are skipped by default. Empty formula fields are ignored and listed in failure details. At least one numeric added field is required; a subtraction-only formula is not evaluated.

### Hash Chain

Sorts rows by `Order by` and checks:

```text
previous row[Previous hash] = current row[Current hash field]
```

Values are compared case-insensitively. Canceled rows are not skipped.

### Number Sequence

Sorts rows by `Order by` and checks:

```text
current value = previous value + Sequence step
```

UI-created checks use a tolerance of `0.01`. Canceled rows are not skipped.

### Grand Total Check

Reconciles transaction amounts against checkpoint report rows in original CSV order.

Configure:

- `Amount / grand total field` for both transaction amounts and checkpoint totals.
- `Receipt type field` for the column containing receipt types.
- `Transaction receipt types` for rows included in arithmetic.
- `Checkpoint receipt types` for rows that close reconciliation intervals.
- `Exclude when nonzero` for an optional monetary exclusion field.
- `Tolerance` for the permitted absolute difference.

The check sums selected transactions before each checkpoint. In the default cumulative mode, each interval is added to previous intervals and compared with the current checkpoint value. Rows after the final checkpoint are not reconciled.

Canceled transaction and checkpoint rows are skipped by default. At least one checkpoint type is required.

### Canceled Receipts

`Compare fields` and `Grand total check` skip canceled receipts by default. The default marker column is `IsCancelled`. A nonempty text marker or a nonzero numeric marker means that the row is canceled.

`Required field`, `Hash chain`, and `Number sequence` do not skip canceled rows.

## Failure Export

After running checks, click `Export failures`. For each check with failures, the application creates:

```text
<check-name>.csv
<check-name>.txt
```

The CSV contains complete original rows selected by `Ceka numurs`. Grand Total failures include keys from the entire interval associated with the failed checkpoint, including rows excluded from arithmetic.

TXT reconstruction requires all of the following to be loaded before opening `Tests`:

- The original TXT source.
- A compiled `Splitter` pattern.
- A compiled `Ceka numurs` pattern.

If these inputs are unavailable, the TXT file may be empty. Failures without a usable `Ceka numurs` key may produce a header-only CSV.

## Corrected CSV Injection

Use `Inject corrected CSV` after manually editing an exported failure CSV.

1. Select the complete original CSV in `CSV to check`.
2. Click `Inject corrected CSV`.
3. Select the corrected CSV.
4. Choose a new output path.

Rows are matched by the fixed `Ceka numurs` key. Keys are trimmed and compared case-insensitively. A corrected row replaces the complete matching master row; unchanged rows remain in their original order.

Before writing, the application verifies:

- Headers match exactly in name, order, and case.
- Every row has a nonempty key.
- Keys are unique within each file.
- Every corrected key exists in the master CSV.
- The output path is different from the master path.

The master CSV cannot be selected as the output file.

## Important Constraints

- `Compare fields` requires the exact column name `Ceka tips`.
- Failure export and corrected-row injection require `Ceka numurs`.
- CSV header and field lookup is case-sensitive.
- The CSV parser does not support embedded newlines inside quoted fields.
- Pattern regexes are case-sensitive; block boundary regexes are case-insensitive.
- Only capture group 1 is extracted.
- `date`, `time`, and other `valueType` labels are not schema validation.
- Invalid numeric matches in `sum` are silently skipped.
- Pattern names should be unique and nonempty.
- There is no visible Cancel button. Closing the main form requests cancellation between receipts, and partial rows may already have been processed.

## Repository Layout

```text
README.md
regexFinder/
  blueprint_blocks_template.yaml
  regexFinder.csproj
  regexFinder.sln
  *.cs
```

Local source data, working YAML, generated CSV files, build output, packages, logs, and Visual Studio user files are excluded by `.gitignore` and must not be committed.

There is currently no automated test project in the solution.
