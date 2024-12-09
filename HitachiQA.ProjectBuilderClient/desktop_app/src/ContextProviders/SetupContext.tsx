import React, { createContext, useContext, useEffect, useState, useRef } from 'react';
import { CircularProgress, Button, Stack, TextField, Box, Typography } from '@mui/material';
import { InvocationResult } from 'node-powershell/dist';

interface SetupContextType {
    environmentReady: boolean;
    reloadSetup: () => void;
}

const SetupContext = createContext<SetupContextType | undefined>(undefined);

export const useSetup = (): SetupContextType => {
    const context = useContext(SetupContext);
    if (!context) {
        throw new Error('useSetup must be used within a SetupProvider');
    }
    return context;
};

const OutputDisplay: React.FC<{ output: string; error: string }> = ({ output, error }) => {
    const reverseText = (text: string) => {
        return text.split('\n').filter(Boolean).reverse().join('\n'); // Split by lines, reverse, and join back
    };

    return (
        <div style={{ display: 'flex', width: '90%' }}>
            <pre
                style={{
                    flex: 1,
                    textAlign: 'left',
                    whiteSpace: 'pre-wrap',
                    maxHeight: '400px',
                    overflowY: 'auto',
                    marginRight: '10px',
                    background: '#f9f9f9',
                    padding: '10px',
                    border: '1px solid #ccc',
                }}
            >
                {reverseText(output)}
            </pre>
            <pre
                style={{
                    flex: 1,
                    textAlign: 'left',
                    whiteSpace: 'pre-wrap',
                    color: 'red',
                    maxHeight: '400px',
                    overflowY: 'auto',
                    background: '#fff5f5',
                    padding: '10px',
                    border: '1px solid #ccc',
                }}
            >
                {reverseText(error)}
            </pre>
        </div>
    );
};


export const SetupProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [environmentReady, setEnvironmentReady] = useState(false);
    const [loadingSetup, setLoadingSetup] = useState(true);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [runCommandInput, setRunCommandInput] = useState<string>('');
    const [currentOutput, setCurrentOutput] = useState<string>('');
    const [currentError, setCurrentError] = useState<string>('');
    const [deviceCode, setDeviceCode] = useState<string | null>(null);
    const [deviceLoginURL, setDeviceLoginURL] = useState<string | null>(null);
    const listenersRegistered = useRef(false);
    const appendOutput = (data: string) => {
        setCurrentOutput((prev) => prev + data + '\n');
    
        const codeMatch = data.match(/enter the code ([A-Z0-9]+) to authenticate/i);
        const urlMatch = data.match(/open the page (https?:\/\/\S+)/i);
    
        if (codeMatch) setDeviceCode(codeMatch[1]);
        if (urlMatch) setDeviceLoginURL(urlMatch[1]);
    };
    const appendError = (data: string) => setCurrentError((prev) => prev + data + '\n');
    const clearOutput = () => {
        setCurrentOutput('');
        setCurrentError('');
    };

    const runScript = async (command: string): Promise<InvocationResult | null> => {
        try {
            const result: InvocationResult = await window.electronAPI.runScript(command);
            return result;
        } catch (error: any) {
            appendError(error.message || 'An unknown error occurred while running the script.');
            return null;
        }
        finally{
            setDeviceCode(null)
        }
    };

    const openLoginURL = () => {
        if (deviceLoginURL) window.electronAPI.openBrowser(deviceLoginURL);
    };

    const checkDotnet = async (): Promise<boolean> => {
        setErrorMessage(null);
        const result = await runScript('dotnet --version');
        if (!result) {
            setErrorMessage(
                '`dotnet` is not installed. Please install Visual Studio (Community, Professional, or Enterprise Edition) and restart this application.'
            );
            return false;
        }
        return true;
    };

    const installCredProvider = async (): Promise<boolean> => {
        setErrorMessage(null);
        const result = await runScript(
            `iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/microsoft/artifacts-credprovider/master/helpers/installcredprovider.ps1'))`
        );

        if (!result) {
            appendError('[SetupContext] Failed to install Azure Artifacts Credential Provider.');
            setErrorMessage(
                `Failed to install Azure Artifacts Credential Provider. Ensure you have a stable internet connection and retry.`
            );
            return false;
        }

        appendOutput('[SetupContext] Azure Artifacts Credential Provider installed successfully.');
        return true;
    };

    const checkHitachiQaBuilder = async (): Promise<boolean> => {
        setErrorMessage(null);

        const result = await runScript('hitachiqabuilder');
        if (result) {
            return true;
        }

        appendError('[SetupContext] `hitachiqabuilder` tool is not installed. Attempting to install Azure Artifacts Credential Provider.');
        const credProviderInstalled = await installCredProvider();
        if (!credProviderInstalled) {
            return false;
        }

        setErrorMessage(
            '`hitachiqabuilder` tool is not installed. Azure Artifacts Credential Provider has been installed. Please run the following command to install `hitachiqabuilder` manually:'
        );
        setRunCommandInput(
            'dotnet tool install --global --add-source "https://tfs-hisol-crm.pkgs.visualstudio.com/_packaging/HitachiFeed/nuget/v3/index.json" hsl.hitachiqa.builder --interactive --prerelease'
        );

        return false;
    };

    const checkEnvironment = async () => {
        setLoadingSetup(true);
        clearOutput();
        setErrorMessage(null);

        const dotnetOk = await checkDotnet();
        if (!dotnetOk) {
            setLoadingSetup(false);
            return;
        }

        const builderOk = await checkHitachiQaBuilder();
        if (!builderOk) {
            setLoadingSetup(false);
            return;
        }

        setEnvironmentReady(true);
        setLoadingSetup(false);
    };

    const runEditedCommand = async () => {
        if (!runCommandInput) return;
        setLoadingSetup(true);
        setErrorMessage(null);

        const result = await runScript(runCommandInput);
        if (!result) {
            setErrorMessage('Command execution failed. Please check the output above or logs for details.');
        } else {
            await checkEnvironment();
        }

        setLoadingSetup(false);
    };

    let count = 0;
    useEffect(() => {
        if (!listenersRegistered.current) {
            window.electronAPI.onPowerShellOutput(appendOutput);
            window.electronAPI.onPowerShellError(appendError);
            listenersRegistered.current = true;
        }

        checkEnvironment()
    }, []);

    if (loadingSetup) {
        return (
            <Stack spacing={2} alignItems="center">
                <CircularProgress />
                <p>Checking environment setup...</p>
                {deviceCode && (
                    <Box sx={{ p: 2, border: '1px solid #ccc', borderRadius: '4px', mt: 2, backgroundColor: '#f9f9f9' }}>
                        <Typography variant="h6">Device Login Required</Typography>
                        <Typography>Open the following URL in your browser:</Typography>
                        <Typography color="primary" sx={{ mt: 1 }}>{deviceLoginURL}</Typography>
                        <Button variant="contained" color="primary" onClick={openLoginURL} sx={{ mt: 1 }}>
                            Open in Browser
                        </Button>
                        <Typography variant="h6" sx={{ mt: 2 }}>Authentication Code:</Typography>
                        <Typography variant="h4" color="secondary">{deviceCode}</Typography>
                    </Box>
                )}
                <OutputDisplay output={currentOutput} error={currentError} />
            </Stack>
        );
    }

    if (!environmentReady) {
        return (
            <Stack spacing={2} alignItems="center">
                {runCommandInput ? (
                    <>
                        <TextField
                            label="Command to Run"
                            variant="outlined"
                            value={runCommandInput}
                            onChange={(e) => setRunCommandInput(e.target.value)}
                            style={{ width: '100%' }}
                        />
                        <Button variant="contained" color="primary" onClick={runEditedCommand}>
                            Run Command
                        </Button>
                    </>
                ) : (
                    <Button variant="contained" color="primary" onClick={checkEnvironment}>
                        Retry Checking Environment
                    </Button>
                )}
                {errorMessage && <pre style={{ textAlign: 'left', whiteSpace: 'pre-wrap', color: 'red' }}>{errorMessage}</pre>}
                <OutputDisplay output={currentOutput} error={currentError} />
            </Stack>
        );
    }

    return (
        <SetupContext.Provider value={{ environmentReady, reloadSetup: checkEnvironment }}>
            {children}
        </SetupContext.Provider>
    );
};
