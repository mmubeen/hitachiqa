import React from 'react';
import Form from './components/form.component'
import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import {Chip, IconButton,Typography } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close';
import { Close } from '@mui/icons-material';
import MinimizeIcon from '@mui/icons-material/Minimize';
import MarkdownDisplay from './components/mdviewer.component'

declare global {
  interface Window {
    electronAPI: {
      openFile: () => any;
      openDevTools: ()=>any;
      runScript: (script: string) => any;
      runBuildScript: (script: string) => any;
      closeApp: () => any;
      minimizeApp: () => any;
      getReadme: () => Promise<string>,
      validateCommandLineToolInstalled: (toolName: string)=> Promise<boolean>
    }
  }
} 
const handleCloseApp = ()=>{
  window.electronAPI.closeApp();
}
const handleMinimizeApp = ()=>{
  window.electronAPI.minimizeApp();
}

const openDevTools= ()=>{
  window.electronAPI.openDevTools();
  
}

const root = createRoot(document.getElementById('root'));

root.render(
  <StrictMode>
    <div style={{position:"absolute", top:5, left:5 }}>
      <Chip label="Open DevTools" color="default" size="small" onClick={openDevTools}></Chip>
    </div>
    <div style={{position:"absolute", top:0, right:0 }}>
      <IconButton onClick={handleMinimizeApp}>
        <MinimizeIcon></MinimizeIcon>
      </IconButton >
      <IconButton onClick={handleCloseApp} color="error">
        <CloseIcon></CloseIcon>
      </IconButton >
    </div>
    <Form />
    <MarkdownDisplay/>
  </StrictMode>
); 