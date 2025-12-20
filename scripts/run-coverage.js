const { spawnSync } = require("child_process");
const path = require("path");

function run(cmd, args, opts) {
  const res = spawnSync(cmd, args, Object.assign({ stdio: "inherit" }, opts));
  return res.status;
}

const repoRoot = path.resolve(__dirname, "..");
const psScript = path.join(repoRoot, "scripts", "coverage.ps1");
const shScript = path.join(repoRoot, "scripts", "coverage.sh");

if (process.platform === "win32") {
  // Try PowerShell Core / pwsh first, then fallback to powershell
  let code = run("pwsh", ["-File", psScript]);
  if (code !== 0) {
    code = run("powershell", ["-File", psScript]);
  }
  if (code !== 0) process.exit(code);
  process.exit(0);
} else {
  // Unix-like: run bash script
  const code = run("bash", [shScript]);
  process.exit(code);
}
