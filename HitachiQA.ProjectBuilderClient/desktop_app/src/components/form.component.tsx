import * as React from 'react';
import { useState } from 'react';
import { TextField, FormControl, InputLabel, Select, MenuItem, Button, Box, SelectProps, ListItemButton, ListItemText, FormHelperText, Chip, Stack, CircularProgress, Typography } from '@mui/material';
import LoadingButton from '@mui/lab/LoadingButton';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import {Done, Error, Replay} from '@mui/icons-material';


interface FormData {
  projectName: string;
  host: string;
  dotnetFramework: string;
  outputFolder: string;
  outputFolderError: boolean;
  result: string,
  framework: string
}
interface PostBuildEventStatus {
  build:"default"|"success"|"error",
  playwright: "default"|"success"|"error"
}
const dotnetFrameworkOptions = ['net6.0', 'net7.0'];
const frameworkOptions = ['Playwright', 'Selenium'];

const Form: React.FC = () => {
  const [formData, setFormData] = useState<FormData>({
    projectName: '',
    host: 'https://www.hitachi.us',
    dotnetFramework: dotnetFrameworkOptions[0],
    outputFolder: '',
    outputFolderError: false,
    result: '',
    framework: "Selenium"
  });
  const [postBuildEvents, setPostBuildEvents] = useState<PostBuildEventStatus>({
    build:"default",
    playwright:"default"
  })
  const [loading, setLoading] = useState<boolean>(false);
  const [openDialog, setOpenDialog] = useState<boolean>(false);
  const [validations, setValidations] = useState([]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement  | { name?: string; value: unknown }>| SelectProps<string>) => {

    const { name, value } = 'target' in e ? e.target:e;
    setFormData((prevFormData) => ({ ...prevFormData, [name as string]: value }));

  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if(!formData.outputFolder)
    {
        setFormData((prevFormData) => ({ ...prevFormData, "outputFolderError": true }));
        return;
    }
    onSubmit(formData);
    
  };

  const onSubmit = async (formData: FormData)=>{
    console.log(formData) 
    setLoading(true);

    let hadErrors = await validateTools()
    if(hadErrors)
    {
      setLoading(false);
      return;
    }
    let args = ` -projectName ${formData.projectName} -dotnetFramework ${formData.dotnetFramework} -targetHost ${formData.host} -outputFolder "${formData.outputFolder}" -framework ${formData.framework}`
    let result = await window.electronAPI.runBuildScript("/assets/createSolution.ps1"+args)
                .finally(()=>{
                  setLoading(false);
                })
            
    setFormData((prevFormData) => ({ ...prevFormData, "result": 'raw' in result? result.raw:result }));

    if("hadErrors" in result)
    {
      if(!result.hadErrors)
      {
        handleOpenDialog();
        runInstallPlaywright();
      }
    }
    console.log("result: ")
    console.log(result)
  }

  const handleOpenDialog = () => {
    setOpenDialog(true); 
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
  };


  const openDir = async ()=>{
    await window.electronAPI.openFile().then((filepath:string|undefined)=> 
    {
      if(filepath)
      {
        setFormData((prevFormData) => ({ ...prevFormData, "outputFolder": filepath, "outputFolderError": false }));
      }
      else
      {
        setFormData((prevFormData) => ({ ...prevFormData, "outputFolderError": true }));
      }
    })



  }

  const validateTools= async () =>{
    setValidations([]);
    let dotnet = await window.electronAPI.validateCommandLineToolInstalled("dotnet")
    //let pwsh = await window.electronAPI.validateCommandLineToolInstalled("pwsh")

    if(!dotnet)
    {
      setValidations((prevState)=>[...prevState, {key: "dotnet", value: "dotnet command line tool not found: please install visual studio 2022 before running this tool"} ]);
    }
    // if(!pwsh)
    // {
    //   setValidations((prevState)=>[...prevState, {key: "pwsh", value: "pwsh command line tool not found: to install, please run \`dotnet tool install --global PowerShell\`"} ]);
    // }

    if(!dotnet)
    {
      return true;
    }
    return false;

  }
  const openSolution= async()=>{
    var openSolutionResult = await window.electronAPI.runScript(`start "${formData.outputFolder}/${formData.projectName}/${formData.projectName}.sln"`);
    
  }
  const runInstallPlaywright= async()=>{
    setLoading(true);
    setPostBuildEvents((prevFormData) => ({ ...prevFormData, build: "default"}));
    setPostBuildEvents((prevFormData) => ({ ...prevFormData, playwright: "default"}));

    let playwrightPath = `"${formData.outputFolder}/${formData.projectName}/bin/Debug/${formData.dotnetFramework}/playwright.ps1"`

    var buildResult = await window.electronAPI.runScript(`dotnet build "${formData.outputFolder}/${formData.projectName}/${formData.projectName}.csproj"`);

    var testPlaywrightPath = await window.electronAPI.runScript(`if(Test-Path ${playwrightPath}){echo "True"} else {echo "False"}`);
    console.log("testPlaywrightPath")
    console.log(testPlaywrightPath)
    if(!hasError(buildResult) && !hasError(testPlaywrightPath) && (testPlaywrightPath.raw as string).toLowerCase()==="true"){

      setPostBuildEvents((prevFormData) => ({ ...prevFormData, build: "success"}));

      var playwrightResult = await window.electronAPI.runScript(`powershell -File ${playwrightPath} install --with-deps`);
      if(!hasError(playwrightResult)){
        setPostBuildEvents((prevFormData) => ({ ...prevFormData, playwright: "success"}));
      }
      else {
        setPostBuildEvents((prevFormData) => ({ ...prevFormData, playwright: "error"}));
      }
    }
    else{
      setPostBuildEvents((prevFormData) => ({ ...prevFormData, build: "error"}));
      setPostBuildEvents((prevFormData) => ({ ...prevFormData, playwright: "error"}));

    }
    setLoading(false);
  }

  const getPostBuildIcon = (outcome: string)=>{
    switch(outcome)
    {
      case "success":
        return (<Done style={{left:170, position:"relative"}}/>)
      case "error":
        return (<Replay cursor="pointer" style={{left:170, position:"relative"}} onClick={runInstallPlaywright}/>)
      default: 
        return (<CircularProgress color="primary" size={20} style={{left:170, position:"relative"}}/>)
    }
  }

  function hasError(result: any)
  {
    console.log(result);
    if("hadErrors" in result)
    {
      return result.hadErrors;
    }
    return true;
  }
  return (
    <div>
    <Box component="form" onSubmit={handleSubmit} sx={{ margin: 2 }}>
      <TextField
        required
        label="Project Name"
        name="projectName"
        value={formData.projectName}
        onChange={handleChange}
        sx={{ m: 1, mb: 2}}
        inputProps={{pattern:'^[a-zA-Z0-9_-]*$'}}
        error={formData.projectName.length>0 && !/^[\w-]+$/.test(formData.projectName)}
        helperText={
          formData.projectName.length>0 && !/^[\w-]+$/.test(formData.projectName)
            ? 'Project name can only contain letters, numbers, dashes, and underscores'
            : ''
        }
      />
      <TextField
        required
        label="Host"
        type="url"
        name="host"
        value={formData.host}
        onChange={handleChange}
        sx={{m: 1, mb: 2 }}
        error={formData.host!=="" && !/^(https?:\/\/).*/.test(formData.host)}
        helperText={formData.host!=="" && !/^(https?:\/\/).*/.test(formData.host)? "make the URL it starts with http:// or https://":""}
      />
      <FormControl required sx={{ m: 1, mb: 2 }}>
        <InputLabel id="dotnet-framework-label">Dotnet Framework</InputLabel>
        <Select
          labelId="dotnet-framework-label"
          id="dotnet-framework-select"
          name="dotnetFramework"
          value={formData.dotnetFramework}
          label="Dotnet Framework"
          onChange={handleChange}
          style={{minWidth:"150px"}}
        >
          {dotnetFrameworkOptions.map((option) => (
            <MenuItem key={option} value={option}>
              {option}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <FormControl required sx={{ m: 1, mb: 2 }}>
        <InputLabel id="framework-label">Framework</InputLabel>
        <Select
          labelId="framework-label"
          id="framework-select"
          name="framework"
          value={formData.framework}
          label="Framework"
          onChange={handleChange}
          style={{minWidth:"150px"}}
        >
          {frameworkOptions.map((option) => (
            <MenuItem key={option} value={option}>
              {option}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <FormControl required fullWidth sx={{ m: 1, mb: 2 }}>
        <TextField
          required
          fullWidth
          label="Output Folder"
          type="text"
          name="outputFolder"
          value={formData.outputFolder}
          onChange={handleChange}
          sx={{ mb: 2 }}
          InputProps={{ readOnly: true }}        
          error={formData.outputFolderError}
          helperText ={formData.outputFolderError?"Please select a location to create the project":""}
        />
        <Button variant="outlined" component="label" onClick={openDir}>
          Select output folder
        </Button>
      </FormControl>
      <FormControl required sx={{ m: 1, mb: 2 }}>
      <LoadingButton
        
        loading = {loading}
        loadingPosition="end"
        type="submit"
        variant="contained"
        style={{minWidth: "200px"}}
        
      >Build Project</LoadingButton>
      </FormControl>
      <FormControl error={true} fullWidth >
        {validations.map(val=> 
          <FormHelperText key={val.key} id={val.key}>{val.value}</FormHelperText>
        )}
      </FormControl>
      <FormControl fullWidth sx={{m: 1}}>
      <TextField
        multiline
        rows={10}
        fullWidth
        InputProps={{ readOnly: true }}  
        value={formData.result}
      >

      </TextField>  
      </FormControl>
      
    </Box>
    <Dialog
        open={openDialog}
        onClose={handleCloseDialog}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">
          {`Successfully Built ${formData.projectName}`}
        </DialogTitle>
        <DialogContent>
          <Stack direction="column" spacing={1} style={{maxWidth:"200px"}}>
            <Chip label="Code generation" color="success" size="small" style={{justifyContent:'left'}} icon={getPostBuildIcon("success")}/>
            <Chip label="Selenium Installation" color="success" size="small" style={{justifyContent:'left'}} icon={getPostBuildIcon("success")}/>
            <Chip label="Dotnet build" color={postBuildEvents.build} icon={getPostBuildIcon(postBuildEvents.build)} size="small" style={{justifyContent:'left'}}/>
            {postBuildEvents.build==="error" && 
              <div>
                <Typography color="error" fontSize={10} variant="caption" display="block" style={{width:"280%"}}>Error might be authentication to HitachiQA feed, please open visual studio and build to enter credentials</Typography>
                <Button color="success" size="small" onClick={openSolution}>Open Visual Studio</Button>
              </div>
                
            }
            <Chip label="Playwright installation" color={postBuildEvents.playwright} icon={getPostBuildIcon(postBuildEvents.playwright)} size="small" style={{justifyContent:'left'}}/>
          </Stack>

        </DialogContent>
        <DialogContent>
          <h4>Next Steps:</h4>
          <DialogContentText id="alert-dialog-description">
            - Install specflow for Visual Studio 2022 extension
          </DialogContentText>
          <DialogContentText id="alert-dialog-description">
            - Build and run your first test <br />
           note: you might be asked to authenticate, use your Hitachi credentials
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog} autoFocus>
            Ok
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
};

export default Form;
