import React, { createContext, useContext, useEffect, useState, useRef } from 'react';
import { CircularProgress, Button, Stack } from '@mui/material';

interface DefaultContextType {
  dotnetFrameworkOptions: string[];
  driverOptions: string[];
  loadingDefaults: boolean;
  reloadDefaults: () => void;
}

const DefaultContext = createContext<DefaultContextType | undefined>(undefined);

export const useDefaults = (): DefaultContextType => {
  const context = useContext(DefaultContext);
  if (!context) {
    throw new Error('useDefaults must be used within a DefaultProvider');
  }
  return context;
};

export const DefaultProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [dotnetFrameworkOptions, setDotnetFrameworkOptions] = useState<string[]>([]);
  const [driverOptions, setDriverOptions] = useState<string[]>([]);
  const [loadingDefaults, setLoadingDefaults] = useState<boolean>(false);
  const [error, setError] = useState<boolean>(false);

  // Track if we've already attempted the initial load to prevent multiple calls on mount
  const hasAttemptedInitialLoad = useRef(false);

  const loadDefaults = async () => {
    console.log('Invoking loadDefaults...');
    
    // Reset states
    setLoadingDefaults(true);
    setError(false);

    try {
      const defaults = await window.electronAPI.loadDefaults();
      console.log('Defaults loaded successfully:', defaults);

      const targetFrameworks = defaults["default.targetFrameworkOptions"];
      const drivers = defaults["default.driverOptions"];

      if (!targetFrameworks || !drivers) {
        throw new Error('Invalid or empty defaults returned');
      }

      setDotnetFrameworkOptions(targetFrameworks);
      setDriverOptions(drivers);
    } catch (err) {
      console.error('Error loading defaults:', err);
      setError(true);
    } finally {
      setLoadingDefaults(false);
    }
  };

  useEffect(() => {
    // Only run loadDefaults once on mount
    if (!hasAttemptedInitialLoad.current) {
      hasAttemptedInitialLoad.current = true;
      loadDefaults();
    }
  }, []);

  // Render states
  if (loadingDefaults) {
    return (
      <Stack spacing={2} alignItems="center">
        <CircularProgress />
        <p>Loading defaults...</p>
      </Stack>
    );
  }

  if (error) {
    return (
      <Stack spacing={2} alignItems="center">
        <p>Error loading defaults. Please try again.</p>
        <Button variant="contained" color="primary" onClick={() => loadDefaults}>
          Retry
        </Button>
      </Stack>
    );
  }

  // If we reached here, we have defaults loaded or at least no error and no loading
  return (
    <DefaultContext.Provider
      value={{
        dotnetFrameworkOptions,
        driverOptions,
        loadingDefaults,
        reloadDefaults: loadDefaults,
      }}
    >
      {children}
    </DefaultContext.Provider>
  );
};
