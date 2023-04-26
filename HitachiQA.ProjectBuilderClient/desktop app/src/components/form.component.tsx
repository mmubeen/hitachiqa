import * as React from 'react';
import { useState } from 'react';
import { TextField, FormControl, InputLabel, Select, MenuItem, Button, Box,  Typography, SelectProps } from '@mui/material';
import LoadingButton from '@mui/lab/LoadingButton';



interface FormData {
  projectName: string;
  host: string;
  targetFramework: string;
  outputFolder: string;
  outputFolderError: boolean;
  result: string
}

const targetFrameworkOptions = ['net6.0', 'net7.0'];

const Form: React.FC = () => {
  const [formData, setFormData] = useState<FormData>({
    projectName: '',
    host: 'https://www.hitachi.us',
    targetFramework: targetFrameworkOptions[0],
    outputFolder: '',
    outputFolderError: false,
    result: ''
  });

  const [loading, setLoading] = useState<boolean>(false);

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
    let args = ` -projectName ${formData.projectName} -targetFramework ${formData.targetFramework} -targetHost ${formData.host} -outputFolder ${formData.outputFolder}`
    let result = await window.electronAPI.runScript("/assets/createSolution.ps1"+args)
                .finally(()=>{
                  setLoading(false);
                })
    setFormData((prevFormData) => ({ ...prevFormData, "result": 'raw' in result? result.raw:result }));

    console.log("result: ")
    console.log(result)
  }




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
        <InputLabel id="target-framework-label">Target Framework</InputLabel>
        <Select
          labelId="target-framework-label"
          id="target-framework-select"
          name="targetFramework"
          value={formData.targetFramework}
          label="Target Framework"
          onChange={handleChange}
          style={{minWidth:"150px"}}
        >
          {targetFrameworkOptions.map((option) => (
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
      {/* <Button type="submit" variant="contained">
        Build Project
      </Button> */}
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
  );
};

export default Form;
