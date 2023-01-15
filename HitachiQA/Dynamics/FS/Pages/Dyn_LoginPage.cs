using BoDi;
using HitachiQA.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Dynamics.FS.Pages
{
    public class Dyn_LoginPage : Dyn_BasePage
    {
        
        public Dyn_LoginPage(ObjectContainer ObjectContainer) : base(ObjectContainer)
        {
        }
        
        //'Next' & 'Sign in' buttons are both Submit
        public Element SubmitButton => Element("//input[@type='submit']");
        public Element UsernameTextField => Element("//input[@name='loginfmt']");
        public Element PasswordTextField => Element("//input[@name='passwd']");


    }
}
