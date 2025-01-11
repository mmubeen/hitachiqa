using HitachiQA.Driver;
using Reqnroll.BoDi;

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
