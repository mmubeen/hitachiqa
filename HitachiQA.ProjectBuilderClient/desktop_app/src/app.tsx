import React from 'react';
import Form from './components/form.component'
import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import { Chip, IconButton, Typography } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close';
import MinimizeIcon from '@mui/icons-material/Minimize';
import MarkdownDisplay from './components/mdviewer.component'
import { DefaultProvider } from './ContextProviders/DefaultContext';
import { SetupProvider } from './ContextProviders/SetupContext';
import { InvocationResult } from 'node-powershell/dist';

declare global {
  interface Window {
    electronAPI: {
      openFile: () => any;
      openDevTools: () => any;
      runScript: (script: string) => Promise<InvocationResult>;
      closeApp: () => any;
      minimizeApp: () => any;
      getReadme: () => Promise<string>;
      validateCommandLineToolInstalled: (toolName: string) => Promise<boolean>;
      loadDefaults: () => any;
      onPowerShellOutput: (callback: (data: string) => void) => void;
      onPowerShellError: (callback: (data: string) => void) => void;
      onPowerShellDone: (callback: (code: number) => void) => void;
      openBrowser: (url: string)=>void;
    };
  }
}


const handleCloseApp = () => {
  window.electronAPI.closeApp();
}
const handleMinimizeApp = () => {
  window.electronAPI.minimizeApp();
}

const openDevTools = () => {
  window.electronAPI.openDevTools();

}

const root = createRoot(document.getElementById('root'));

root.render(
  <>
    <div style={{ position: 'absolute', top: 5, left: 5 }}>
      <Chip label="Open DevTools" color="default" size="small" onClick={openDevTools}></Chip>
    </div>
    <div style={{ position: 'absolute', top: 0, right: 0 }}>
      <IconButton onClick={handleMinimizeApp}>
        <MinimizeIcon />
      </IconButton>
      <IconButton onClick={handleCloseApp} color="error">
        <CloseIcon />
      </IconButton>
    </div>
    <StrictMode>
      <SetupProvider>
        <DefaultProvider>
          <Form />
          <MarkdownDisplay />
        </DefaultProvider>
      </SetupProvider>
    </StrictMode>
  </>


); 