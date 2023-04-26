import React, { useEffect, useState } from 'react';
import { marked } from 'marked';
import { Typography } from '@mui/material';
import KeyboardDoubleArrowDownIcon from '@mui/icons-material/KeyboardDoubleArrowDown';

const MarkdownDisplay = () => {
  const [markdown, setMarkdown] = useState('');

  useEffect(() => {
     window.electronAPI.getReadme().then(result=>{
      console.log(result);
      setMarkdown(result);
    });

  }, []);

  return (
    <div>
      <Typography align='center' variant='h2'>README
      <KeyboardDoubleArrowDownIcon fontSize='inherit'></KeyboardDoubleArrowDownIcon></Typography>
    {markdown=="" ? <p>loading readme</p> :
      <div dangerouslySetInnerHTML={{ __html: marked.parse(markdown) }}></div>}
    </div>
    
  );
};

export default MarkdownDisplay;
