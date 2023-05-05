import { InvocationResult } from "node-powershell/dist";

const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('electronAPI', {
  openFile: () => ipcRenderer.invoke('dialog:openFile'),
  openDevTools: () => ipcRenderer.invoke('devTools:open'),
  runScript: (script: string) => ipcRenderer.invoke('script:run', script),
  runBuildScript: (script: string)=> ipcRenderer.invoke('script:runBuild', script),
  closeApp: () => ipcRenderer.invoke('app:close'),
  minimizeApp: () => ipcRenderer.invoke('app:minimize'),
  getReadme: () => ipcRenderer.invoke('file:readme'),
  validateCommandLineToolInstalled: (toolName: string)=>ipcRenderer.invoke('validate:toolInstalled', toolName)
})

