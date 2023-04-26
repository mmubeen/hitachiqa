import { InvocationResult } from "node-powershell/dist";

const { contextBridge, ipcRenderer } = require('electron')

contextBridge.exposeInMainWorld('electronAPI', {
  openFile: () => ipcRenderer.invoke('dialog:openFile'),
  runScript: (script: string) => ipcRenderer.invoke('script:run', script),
  closeApp: () => ipcRenderer.invoke('app:close'),
  minimizeApp: () => ipcRenderer.invoke('app:minimize'),
  getReadme: () => ipcRenderer.invoke('file:readme')

})

