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
    }
  }

  public async destroy(): Promise<void> {
    // cleanup if needed later
  }
}

runEntrypoint(MalachInstance, [])
