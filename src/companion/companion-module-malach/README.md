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