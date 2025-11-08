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
  private readonly commonAlphaKeys = 'abcdefghijklmnopqrstuvwxyz'.split('').map((k) => ({ id: k, label: k.toUpperCase() }))
  private readonly commonSpecialKeys = [
    'enter','tab','escape','space','backspace','delete',
    'up','down','left','right','home','end','pageup','pagedown',
    'f1','f2','f3','f4','f5','f6','f7','f8','f9','f10','f11','f12',
  ].map((k) => ({ id: k, label: k }))

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
        name: 'VicReo: Special Key (type=pressSpecial)',
        options: [
          { type: 'checkbox', id: 'useCustom', label: 'Use custom special key', default: false },
          {
            type: 'dropdown', id: 'specialKey', label: 'Special key',
            choices: this.commonSpecialKeys, default: 'enter',
            isVisibleExpression: '$(options:useCustom) == false',
          },
          {
            type: 'textinput', id: 'specialKeyCustom', label: 'Custom special key',
            isVisibleExpression: '$(options:useCustom) == true',
          },
        ],
        callback: (opts) => {
          const o = opts as any
          const key = (o.useCustom ? o.specialKeyCustom : o.specialKey) ?? ''
          const payload = { key, type: 'pressSpecial', password: '' }
          this.SendJson(JSON.stringify(payload))
        },
      },

      combination: {
        name: 'VicReo: Combination (type=combination)',
        options: [
          { type: 'dropdown', id: 'mod1', label: 'Modifier', choices: this.modifierChoices, default: 'ctrl'},
          { type: 'checkbox', id: 'useCustom', label: 'Use custom key', default: false},
          {
            type: 'dropdown', id: 'key', label: 'Key',
            choices: [...this.commonAlphaKeys, ...this.commonSpecialKeys], default: 'c',
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
          const modifiers = [o.mod1].filter(Boolean)
          const payload = { key, type: 'combination', modifiers, password: '' }
          this.SendJson(JSON.stringify(payload))
        },
      },

      trio: {
        name: 'VicReo: Trio (type=trio)',
        options: [
          { type: 'dropdown', id: 'mod1', label: 'Modifier 1', choices: this.modifierChoices, default: 'ctrl' },
          { type: 'dropdown', id: 'mod2', label: 'Modifier 2', choices: this.modifierChoices, default: 'shift' },
          { type: 'checkbox', id: 'useCustom', label: 'Use custom key', default: false },
          {
            type: 'dropdown', id: 'key', label: 'Key',
            choices: [...this.commonAlphaKeys, ...this.commonSpecialKeys], default: 'a',
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
          const modifiers = [o.mod1, o.mod2].filter(Boolean)
          const payload = { key, type: 'trio', modifiers, password: '' }
          this.SendJson(JSON.stringify(payload))
        },
      },

      quartet: {
        name: 'VicReo: Quartet (type=quartet)',
        options: [
          { type: 'dropdown', id: 'mod1', label: 'Modifier 1', choices: this.modifierChoices, default: 'ctrl' },
          { type: 'dropdown', id: 'mod2', label: 'Modifier 2', choices: this.modifierChoices, default: 'shift' },
          { type: 'dropdown', id: 'mod3', label: 'Modifier 3', choices: this.modifierChoices, default: 'alt' },
          { type: 'checkbox', id: 'useCustom', label: 'Use custom key', default: false },
          {
            type: 'dropdown', id: 'key', label: 'Key',
            choices: [...this.commonAlphaKeys, ...this.commonSpecialKeys], default: 'a',
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
          const modifiers = [o.mod1, o.mod2, o.mod3].filter(Boolean)
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
            choices: [...this.commonAlphaKeys, ...this.commonSpecialKeys], default: 'a',
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
            choices: [...this.commonAlphaKeys, ...this.commonSpecialKeys], default: 'a',
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
