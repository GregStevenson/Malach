import {
  InstanceBase,
  InstanceStatus,
  runEntrypoint,
  CompanionActionDefinitions,
  SomeCompanionConfigField,
} from '@companion-module/base'

interface Config {
  host: string
  port: number
  token: string
}

class MalachInstance extends InstanceBase<Config> {
  // store current config so TS knows it exists
  private config?: Config

  public async init(): Promise<void> {
    this.updateStatus(InstanceStatus.Ok)
    this.setActionDefinitions(this.getActions())
  }

    // --- Add inside class MalachInstance ---

  // Common lists
// --- Alphanumeric key lists ---
private readonly lowercaseAlphaKeys: { id: string; label: string }[] =
  Array.from({ length: 26 }, (_, i) => String.fromCharCode(97 + i)).map((c) => ({ id: c, label: c }))

private readonly uppercaseAlphaKeys: { id: string; label: string }[] =
  Array.from({ length: 26 }, (_, i) => String.fromCharCode(65 + i)).map((c) => ({ id: c, label: c }))

private readonly numericKeys: { id: string; label: string }[] =
  Array.from({ length: 10 }, (_, i) => String(i)).map((n) => ({ id: n, label: n }))

// Combine into commonAlphaNumericKeys (ID and label are identical)
private readonly commonAlphaNumericKeys: { id: string; label: string }[] = [
  ...this.lowercaseAlphaKeys,
  ...this.uppercaseAlphaKeys,
  ...this.numericKeys,
]

// --- Special keys (with display labels) ---
private readonly commonSpecialKeys: { id: string; label: string }[] = (() => {
  const ids = [
    'enter',
    'tab',
    'escape',
    'space',
    'backspace',
    'delete',
    'up',
    'down',
    'left',
    'right',
    'home',
    'end',
    'pageup',
    'pagedown',
    'insert',
    'printscreen',
    'pause',
    'scrolllock',
  ]

  const labelFor = (id: string): string => {
    switch (id) {
      case 'up':
        return 'Arrow Up'
      case 'down':
        return 'Arrow Down'
      case 'left':
        return 'Arrow Left'
      case 'right':
        return 'Arrow Right'
      case 'pageup':
        return 'Page Up'
      case 'pagedown':
        return 'Page Down'
      default:
        return id.charAt(0).toUpperCase() + id.slice(1) // e.g., escape -> Escape
    }
  }

  const base = ids.map((id) => ({ id, label: labelFor(id) }))
  const fkeys = Array.from({ length: 24 }, (_, i) => {
    const n = i + 1
    return { id: `f${n}`, label: `F${n}` }
  })

  return [...base, ...fkeys]
})()

// --- Modifier choices (unchanged IDs, initial-cap labels already set) ---
private readonly modifierChoices = [
  { id: 'ctrl', label: 'Ctrl' },
  { id: 'shift', label: 'Shift' },
  { id: 'alt', label: 'Alt' },
  { id: 'command', label: 'Command/Win' },
]


  // For now: just log the JSON you would send to the server
  private SendJson(json: string): void {
    this.log('info', `SendJson: ${json}`)
  }


  public getConfigFields(): SomeCompanionConfigField[] {
    return [
      { type: 'textinput', id: 'host', label: 'Host', default: '127.0.0.1', width: 6 },
      { type: 'number', id: 'port', label: 'Port', default: 5123, min: 1, max: 65535, width: 3 },
      { type: 'textinput', id: 'token', label: 'Bearer Token', default: 'dev-token', width: 12 },
    ]
  }

  public async configUpdated(newConfig: Config): Promise<void> {
    this.config = newConfig
  }

  private get baseUrl(): string {
    const host = this.config?.host ?? '127.0.0.1'
    const port = this.config?.port ?? 5123
    return `http://${host}:${port}`
  }

  private get authHeader(): string {
    return `Bearer ${this.config?.token ?? ''}`
  }

  private getActions(): CompanionActionDefinitions {
    return {
      // --- existing actions you already had ---
      launch_notepad: {
        name: 'Malach: Launch Notepad',
        options: [],
        callback: async () => {
          const url = `${this.baseUrl}/v1/launch`
          try {
            const res = await fetch(url, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                Authorization: this.authHeader,
              },
              body: JSON.stringify({ exe: 'notepad.exe', args: '' }),
            })
            if (!res.ok) throw new Error(`HTTP ${res.status}`)
            this.log('info', 'Malach requested Notepad launch')
          } catch (e: any) {
            this.log('error', `Malach request failed: ${e?.message ?? e}`)
            this.updateStatus(InstanceStatus.UnknownError, e?.message ?? 'Request failed')
          }
        },
      },

      open_path: {
        name: 'Malach: Open Path',
        options: [{ type: 'textinput', id: 'path', label: 'Path to open', default: 'C:\\' }],
        callback: async (opts) => {
          const path = String((opts as any)?.path ?? '')
          const url = `${this.baseUrl}/v1/open`
          try {
            const res = await fetch(url, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                Authorization: this.authHeader,
              },
              body: JSON.stringify({ path }),
            })
            if (!res.ok) throw new Error(`HTTP ${res.status}`)
            this.log('info', `Malach requested open: ${path}`)
          } catch (e: any) {
            this.log('error', `Open failed: ${e?.message ?? e}`)
            this.updateStatus(InstanceStatus.UnknownError, e?.message ?? 'Request failed')
          }
        },
      },

      // --- VicReo-style actions ---

singleKey: {
  name: 'Single key',
  options: [
    {
      type: 'textinput',
      id: 'singleKey',
      label: 'Single key to send',
      tooltip: 'One alphanumeric character (A–Z, 0–9)',
      // UI validation: turns the field red unless it matches exactly one alphanumeric char
      regex: '/^[A-Za-z0–9]$/',
      required: true,
      useVariables: false,
    },
  ],
  callback: (opts) => {
    const raw = String((opts as any)?.options?.singleKey ?? '')
    const isValid = /^[A-Za-z0–9]$/.test(raw)

    // Preserve user’s case exactly; empty if invalid
    const key = isValid ? raw : ''

    if (!isValid) {
      this.log('warn', `Malach: Single key invalid "${raw}" — sending empty key`)
    } else {
      this.log('info', `Malach: Single key valid "${raw}"`)
    }

    const payload = { key, type: 'press', password: '' }
    this.SendJson(JSON.stringify(payload))
  },
},


specialKey: {
  name: 'Special key',
  options: [
    {
      type: 'dropdown',
      id: 'specialKey',
      label: 'Special key to send',
      tooltip: 'Select a special key (e.g., Enter, Escape, Arrow Up, F5, etc.)',
      choices: [...this.commonSpecialKeys].sort((a, b) =>
        a.label.localeCompare(b.label)
      ),
      default: 'enter',     
    },
  ],
  callback: (opts) => {
    const key = String((opts as any)?.options?.specialKey ?? '').trim()
    const isValid = key.length > 0
    if (!isValid) {
      this.log('warn', 'Malach: Special key invalid or empty — sending empty key')
    } else {
      this.log('info', `Malach: Special key selected "${key}"`)
    }

    const payload = { key: isValid ? key : '', type: 'pressSpecial', password: '' }
    this.SendJson(JSON.stringify(payload))
  },
},


combination: {
  name: 'Modified Key',
  options: [
    {
      type: 'dropdown',
      id: 'mod1',
      label: 'Modifier',
      choices: this.modifierChoices,
      default: 'ctrl',      
    },
    {
      type: 'checkbox',
      id: 'useSpecial',
      label: 'Special key',
      default: false,
    },

    // Text input (same rules as your Single key) when NOT using special
    {
      type: 'textinput',
      id: 'keyText',
      label: 'Single key to send',
      tooltip: 'One alphabetic character (A–Z only)',
      regex: '/^[A-Za-z]$/',
      required: true,
      useVariables: false,
      isVisibleExpression: '$(options:useSpecial) == false',
    },

    // Special key dropdown when using special
    {
      type: 'dropdown',
      id: 'keySpecial',
      label: 'Special key to send',
      tooltip: 'Select a special key (e.g., Enter, Escape, Arrow Up, F5, …)',
      choices: [...this.commonSpecialKeys].sort((a, b) => a.label.localeCompare(b.label)),
      default: 'enter',
      isVisibleExpression: '$(options:useSpecial) == true',
    },
  ],

  callback: (opts) => {
    const o = (opts as any)?.options ?? {}
    const mod = String(o.mod1 ?? 'ctrl')
    const useSpecial = o.useSpecial === true || o.useSpecial === 'true'

    let key = ''
    if (useSpecial) {
      key = String(o.keySpecial ?? '').trim()
    } else {
      const raw = String(o.keyText ?? '')
      const isValid = /^[A-Za-z]$/.test(raw)
      if (!isValid) {
        this.log('warn', `Combination Key: invalid single key "${raw}" — sending empty key`)
        key = ''
      } else {
        key = raw // preserve user case
      }
    }

    const payload = { key, type: 'combination', modifiers: [mod], password: '' }
    this.SendJson(JSON.stringify(payload))
  },
},
trio: {
  name: 'Doubly Modified Key',
  options: [
    {
      type: 'dropdown',
      id: 'mod1',
      label: 'Modifier 1',
      choices: this.modifierChoices,
      default: 'ctrl',
    },
    {
      type: 'dropdown',
      id: 'mod2',
      label: 'Modifier 2',
      choices: this.modifierChoices,
      default: 'shift',
    },
    {
      type: 'checkbox',
      id: 'useSpecial',
      label: 'Special key',
      default: false,
    },

    // Text input (same rules as Single key) when NOT using special
    {
      type: 'textinput',
      id: 'keyText',
      label: 'Single key to send',
      tooltip: 'One alphabetic character (A–Z only)',
      regex: '/^[A-Za-z]$/',
      useVariables: false,
      isVisibleExpression: '$(options:useSpecial) == false',
    },

    // Special key dropdown when using special
    {
      type: 'dropdown',
      id: 'keySpecial',
      label: 'Special key to send',
      tooltip: 'Select a special key (e.g., Enter, Escape, Arrow Up, F5, …)',
      choices: [...this.commonSpecialKeys].sort((a, b) => a.label.localeCompare(b.label)),
      default: 'enter',
      isVisibleExpression: '$(options:useSpecial) == true',
    },
  ],

  callback: (opts) => {
    const o = (opts as any)?.options ?? {}

    const modifiers = Array.from(
      new Set([String(o.mod1 ?? ''), String(o.mod2 ?? '')].filter(Boolean))
    )

    let key = ''
    if (o.useSpecial === true || o.useSpecial === 'true') {
      key = String(o.keySpecial ?? '').trim()
    } else {
      const raw = String(o.keyText ?? '')
      key = /^[A-Za-z]$/.test(raw) ? raw : ''
      if (!key) this.log('warn', `Doubly Modified Key: invalid single key "${raw}" — sending empty key`)
    }

    const payload = { key, type: 'trio', modifiers, password: '' }
    this.SendJson(JSON.stringify(payload))
  },
},

quartet: {
  name: 'Triply Modified Key',
  options: [
    {
      type: 'dropdown',
      id: 'mod1',
      label: 'Modifier 1',
      choices: this.modifierChoices,
      default: 'ctrl',
    },
    {
      type: 'dropdown',
      id: 'mod2',
      label: 'Modifier 2',
      choices: this.modifierChoices,
      default: 'shift',
    },
    {
      type: 'dropdown',
      id: 'mod3',
      label: 'Modifier 3',
      choices: this.modifierChoices,
      default: 'alt',
    },
    {
      type: 'checkbox',
      id: 'useSpecial',
      label: 'Special key',
      default: false,
    },

    // Text input (same rules as Single key) when NOT using special
    {
      type: 'textinput',
      id: 'keyText',
      label: 'Single key to send',
      tooltip: 'One alphabetic character (A–Z only)',
      regex: '/^[A-Za-z]$/',
      useVariables: false,
      isVisibleExpression: '$(options:useSpecial) == false',
    },

    // Special key dropdown when using special
    {
      type: 'dropdown',
      id: 'keySpecial',
      label: 'Special key to send',
      tooltip: 'Select a special key (e.g., Enter, Escape, Arrow Up, F5, …)',
      choices: [...this.commonSpecialKeys].sort((a, b) => a.label.localeCompare(b.label)),
      default: 'enter',
      isVisibleExpression: '$(options:useSpecial) == true',
    },
  ],

  callback: (opts) => {
    const o = (opts as any)?.options ?? {}

    const modifiers = Array.from(
      new Set([String(o.mod1 ?? ''), String(o.mod2 ?? ''), String(o.mod3 ?? '')].filter(Boolean))
    )

    let key = ''
    if (o.useSpecial === true || o.useSpecial === 'true') {
      key = String(o.keySpecial ?? '').trim()
    } else {
      const raw = String(o.keyText ?? '')
      key = /^[A-Za-z]$/.test(raw) ? raw : ''
      if (!key) this.log('warn', `Triply Modified Key: invalid single key "${raw}" — sending empty key`)
    }

    const payload = { key, type: 'quartet', modifiers, password: '' }
    this.SendJson(JSON.stringify(payload))
  },
},


      keyDown: {
        name: 'VicReo: Key Down (type=down)',
        options: [
          { type: 'checkbox', id: 'useCustom', label: 'Use custom key', default: false },
          {
            type: 'dropdown', id: 'key', label: 'Key',
            choices: [...this.commonAlphaNumericKeys, ...this.commonSpecialKeys], default: 'a',
            isVisibleExpression: '$(options:useCustom) == false',
          },
          {
            type: 'textinput', id: 'keyCustom', label: 'Custom key',
            isVisibleExpression: '$(options:useCustom) == true',
          },
        ],
        callback: (opts) => {
          const o = opts as any
          const key = (o.useCustom ? o.keyCustom : o.key) ?? ''
          const payload = { key, type: 'down', password: '' }
          this.SendJson(JSON.stringify(payload))
        },
      },

      keyUp: {
        name: 'VicReo: Key Up (type=up)',
        options: [
          { type: 'checkbox', id: 'useCustom', label: 'Use custom key', default: false },
          {
            type: 'dropdown', id: 'key', label: 'Key',
            choices: [...this.commonAlphaNumericKeys, ...this.commonSpecialKeys], default: 'a',
            isVisibleExpression: '$(options:useCustom) == false',
          },
          {
            type: 'textinput', id: 'keyCustom', label: 'Custom key',
            isVisibleExpression: '$(options:useCustom) == true',
          },
        ],
        callback: (opts) => {
          const o = opts as any
          const key = (o.useCustom ? o.keyCustom : o.key) ?? ''
          const payload = { key, type: 'up', password: '' }
          this.SendJson(JSON.stringify(payload))
        },
      },

      msg: {
        name: 'VicReo: Type String (type=string)',
        options: [{ type: 'textinput', id: 'msg', label: 'Message', default: '' }],
        callback: (opts) => {
          const o = opts as any
          const payload = { type: 'string', msg: String(o.msg ?? ''), password: '' }
          this.SendJson(JSON.stringify(payload))
        },
      },
    }
  }


  public async destroy(): Promise<void> {
    // cleanup if needed later
  }
}

runEntrypoint(MalachInstance, [])
