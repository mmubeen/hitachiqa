import * as React from 'react';
import { useState } from 'react';
import { TextField, FormControl, InputLabel, Select, MenuItem, Button, Box, SelectProps, ListItemButton, ListItemText, FormHelperText, Chip, Stack, CircularProgress, Typography, LinearProgress } from '@mui/material';
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
  const [progress, setProgress] = React.useState(0);
  
  let timer: NodeJS.Timer = null;

  function startProgress(currentProgress: number) {
    
    if(currentProgress===100 && timer!== null)
    {
      clearInterval(timer);
      setProgress(100);
    }
    let newProgress = currentProgress;
    const startTimer = () => {
      timer = setInterval(() => {
        if (newProgress < 90) {
          newProgress += 1;
          setProgress(newProgress);
        }
      }, 300);
    };
  
    startTimer();
    setTimeout(() => {
      clearInterval(timer);
    }, 30000-6000-(currentProgress*600));
  }
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
    startProgress(0);
    let hadErrors = await validateTools()
    if(hadErrors)
    {
      setLoading(false);
      return;
    }
    let args = ` -projectName ${formData.projectName} -dotnetFramework ${formData.dotnetFramework} -targetHost ${formData.host} -outputFolder "${formData.outputFolder}" -framework ${formData.framework}`
    let result = await window.electronAPI.runBuildScript("/assets/createSolution.ps1"+args)
                .finally(()=>{
                  startProgress(100);
                  setLoading(false);
                })
            
    setFormData((prevFormData) => ({ ...prevFormData, "result": 'raw' in result? result.raw:result }));


    if("hadErrors" in result)
    {
      if(!result.hadErrors)
      {
        handleOpenDialog();
        if(!isPlaywrightFilePresent())
        {
          runInstallPlaywright();
        }
        else{
          setPostBuildEvents((prevFormData) => ({ ...prevFormData, build: "success", playwright: "success"}));
        }
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
  const openDevTools= ()=>{
    window.electronAPI.openDevTools();
    
  }
  const runInstallPlaywright= async()=>{
    setLoading(true);
    setPostBuildEvents((prevFormData) => ({ ...prevFormData, build: "default", playwright: "default"}));



    var buildResult = await window.electronAPI.runScript(`dotnet build "${formData.outputFolder}/${formData.projectName}/${formData.projectName}.csproj" --interactive`);

    if(!hasError(buildResult) && isPlaywrightFilePresent()){

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
      setPostBuildEvents((prevFormData) => ({ ...prevFormData, build: "error", playwright: "error"}));
    }
    setLoading(false);
    setProgress(100);

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
  const playwrightPath:string= `"${formData.outputFolder}/${formData.projectName}/bin/Debug/${formData.dotnetFramework}/playwright.ps1"`

  async function isPlaywrightFilePresent()
  {
    var testPlaywrightPath = await window.electronAPI.runScript(`if(Test-Path ${playwrightPath}){echo "True"} else {echo "False"}`);
    if(hasError(testPlaywrightPath)){
        return false;
    }
    console.log("testPlaywrightPath")
    console.log(testPlaywrightPath)
    return (testPlaywrightPath.raw as string).toLowerCase()==="true"
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
      <LinearProgress variant="determinate" value={progress} />
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
                <div style={{ textAlign: "right", position: "absolute", right:"20px" }}>
                  <Chip label="Open Visual Studio" color="primary" size="small" onClick={openSolution}></Chip>
                </div>
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
          <Chip label="Open DevTools" color="default" size="small" onClick={openDevTools}></Chip>
          <Chip label="Open in Visual Studio" color="primary" size="small" onClick={openSolution}></Chip>
          <Button onClick={handleCloseDialog} autoFocus>
            Ok
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
};

export default Form;
