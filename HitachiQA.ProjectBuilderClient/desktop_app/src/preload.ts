const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('electronAPI', {
  openFile: () => ipcRenderer.invoke('dialog:openFile'),
  openDevTools: () => ipcRenderer.invoke('devTools:open'),
  runScript: (script: string) => ipcRenderer.invoke('script:run', script),
  closeApp: () => ipcRenderer.invoke('app:close'),
  minimizeApp: () => ipcRenderer.invoke('app:minimize'),
  getReadme: () => ipcRenderer.invoke('file:readme'),
  validateCommandLineToolInstalled: (toolName: string) => ipcRenderer.invoke('validate:toolInstalled', toolName),
  loadDefaults: () => ipcRenderer.invoke('info:loadDefaults'),
  onPowerShellOutput: (callback: (data: string) => void) => {
    ipcRenderer.on('powershell-output', (_, data) => callback(data));
  },
  onPowerShellError: (callback: (data: string) => void) => {
    ipcRenderer.on('powershell-error', (_, data) => callback(data));
  },
  onPowerShellDone: (callback: (code: number) => void) => {
    ipcRenderer.on('powershell-done', (_, code) => callback(code));
  },
  openBrowser: (url: string) => ipcRenderer.invoke('open-browser', url),  
});

