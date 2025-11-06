# === SETTINGS ===
$ModuleRoot = "D:\!!!!!CompanionConnections\Malach\src\companion\companion-module-malach"
$ErrorActionPreference = "Stop"

Write-Host "Scaffolding Malach Companion module at:`n$ModuleRoot" -ForegroundColor Cyan

# === CREATE FOLDERS ===
New-Item -ItemType Directory -Force -Path $ModuleRoot | Out-Null
New-Item -ItemType Directory -Force -Path "$ModuleRoot\src" | Out-Null

# === package.json ===
@'
{
  "name": "companion-module-malach",
  "version": "0.0.1",
  "private": true,
  "license": "MIT",
  "type": "module",
  "main": "dist/index.js",
  "scripts": {
    "build": "tsc -p tsconfig.build.json",
    "dev": "tsc -w -p tsconfig.build.json",
    "manifest": "companion-generate-manifest",
    "package": "companion-module-build"
  },
  "dependencies": {
    "@companion-module/base": "^1.13.4"
  },
  "devDependencies": {
    "@companion-module/tools": "^2.4.2",
    "typescript": "^5.5.4",
    "@types/node": "^20.14.2"
  }
}
'@ | Set-Content -Encoding UTF8 "$ModuleRoot\package.json"

# === tsconfig.json ===
@'
{
  "compilerOptions": {
    "target": "ES2021",
    "module": "NodeNext",
    "moduleResolution": "NodeNext",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "types": ["node"],
    "lib": ["ES2021", "DOM"]
  }
}
'@ | Set-Content -Encoding UTF8 "$ModuleRoot\tsconfig.json"

# === tsconfig.build.json ===
@'
{
  "extends": "./tsconfig.json",
  "compilerOptions": {
    "outDir": "dist",
    "rootDir": "src"
  },
  "include": ["src"]
}
'@ | Set-Content -Encoding UTF8 "$ModuleRoot\tsconfig.build.json"

# === README.md ===
@'
# Malach — Bitfocus Companion Module

This module talks to the local **Malach** helper (HTTP API at `http://127.0.0.1:5123`) instead of spawning processes inside Companion.

## Config
- Host: 127.0.0.1
- Port: 5123
- Bearer Token: dev-token (match your Malach.HttpHost)

## Dev
```bash
yarn install
yarn build
yarn manifest
'@ | Set-Content -Encoding UTF8 "$ModuleRoot\README.md"