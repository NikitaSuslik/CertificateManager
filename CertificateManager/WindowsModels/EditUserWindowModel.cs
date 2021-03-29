using CertificateManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CertificateManager.WindowsModels
{
    class EditUserWindowModel : MyWindowModel
    {
        private User u;
        public UserGroupBoxModel UserModel
        {
            get; set;
        } = new UserGroupBoxModel();

        private CommandRelise _CancelButton;
        public CommandRelise CancelButton
        {
            get 
            { 
                return _CancelButton ?? (_CancelButton = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.CloseCurrentWindow();
                })); 
            }
        }

        private CommandRelise _SaveButton;
        public CommandRelise SaveButton
        {
            get
            {
                return _SaveButton ?? (_SaveButton = new CommandRelise(obj =>
                {
                    try
                    {
                        User nu = UserModel.NewUser;
                        nu.ID = u.ID;
                        SQLManager.Shared.EditUser(nu);
                        WindowsManager.Shared.CloseCurrentWindow();
                    }
                    catch(Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Error!", err.Message, true);
                    }
                }));
            }
        }

        protected override void _PropsChanged()
        {
            User user = props[0] as User;

            u = user;
            UserModel.Login = user.Login;
            UserModel.Params = user.Params;
            UserModel.Password = user.Password;
            UserModel.props = props;
        }

    }
}
