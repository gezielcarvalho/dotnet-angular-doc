#!/usr/bin/env bash
# Run tests with coverage and print a concise summary. Exit non-zero when below threshold.
set -euo pipefail

FAIL_UNDER=${1:-0}

echo "Running tests with coverage..."
dotnet test backend.tests --nologo --collect:"XPlat Code Coverage"

# Pick the newest coverage file to avoid returning an older run's file
COV_FILE=$(find backend.tests/TestResults -type f -name coverage.cobertura.xml -print0 2>/dev/null | xargs -0 ls -1t 2>/dev/null | head -n1 || true)
if [[ -z "${COV_FILE}" ]]; then
  echo "coverage.cobertura.xml not found under backend.tests/TestResults" >&2
  exit 2
fi

LINE_RATE=$(grep -oP 'line-rate="\K[0-9.]+' "${COV_FILE}" | head -n1 || true)
LINES_COVERED=$(grep -oP 'lines-covered="\K[0-9]+' "${COV_FILE}" | head -n1 || true)
LINES_VALID=$(grep -oP 'lines-valid="\K[0-9]+' "${COV_FILE}" | head -n1 || true)

if [[ -z "${LINE_RATE}" ]]; then
  echo "Failed to parse line-rate from coverage XML" >&2
  exit 3
fi

# Ensure consistent numeric parsing regardless of locale
export LC_ALL=C

# Some tools write line-rate as a fraction (0.1765) and some as a percentage (17.65).
# If value is <= 1 treat as fraction and multiply by 100, otherwise treat as already percent.
PERCENT=$(awk -v lr="${LINE_RATE}" 'BEGIN{ if(lr=="" ){print "0.00"; exit} if(lr+0 <= 1) printf "%.2f", (lr+0)*100; else printf "%.2f", (lr+0) }')

echo "Coverage summary:"
echo "  Line coverage : ${PERCENT}% (${LINES_COVERED} / ${LINES_VALID})"

cmp=$(awk -v a="${PERCENT}" -v b="${FAIL_UNDER}" 'BEGIN{print (a<b)}')
if [[ "${cmp}" == "1" ]]; then
  echo "Coverage ${PERCENT}% is below threshold ${FAIL_UNDER}% - failing" >&2
  exit 1
fi

exit 0
