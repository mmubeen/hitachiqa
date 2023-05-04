import * as React from 'react';
import { useState } from 'react';
import { TextField, FormControl, InputLabel, Select, MenuItem, Button, Box, SelectProps, ListItemButton, ListItemText } from '@mui/material';
import LoadingButton from '@mui/lab/LoadingButton';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';


interface FormData {
  projectName: string;
  host: string;
  dotnetFramework: string;
  outputFolder: string;
  outputFolderError: boolean;
  result: string,
  framework: string
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
  const [loading, setLoading] = useState<boolean>(false);
  const [openDialog, setOpenDialog] = React.useState(false);

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
    let args = ` -projectName ${formData.projectName} -dotnetFramework ${formData.dotnetFramework} -targetHost ${formData.host} -outputFolder "${formData.outputFolder}" -framework ${formData.framework}`
    let result = await window.electronAPI.runScript("/assets/createSolution.ps1"+args)
                .finally(()=>{
                  setLoading(false);
                })
            
    setFormData((prevFormData) => ({ ...prevFormData, "result": 'raw' in result? result.raw:result }));

    if("hadErrors" in result)
    {
      if(!result.hasErrors)
      {
        handleOpenDialog();
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
    let path = await window.electronAPI.openFile().then((filepath:string|undefined)=> 
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
          <h4>Next Steps:</h4>
          <DialogContentText id="alert-dialog-description">
            - Download Visual Studio 2022
          </DialogContentText>
          <DialogContentText id="alert-dialog-description">
            - Install specflow for Visual Studio 2022 extension
          </DialogContentText>
          <DialogContentText id="alert-dialog-description">
            - Build and run your first test
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
