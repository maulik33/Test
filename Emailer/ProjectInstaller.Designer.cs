namespace Emailer
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.EmailerServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.EmailerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // EmailerServiceProcessInstaller1
            // 
            this.EmailerServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.EmailerServiceProcessInstaller1.Password = null;
            this.EmailerServiceProcessInstaller1.Username = null;
            // 
            // EmailerServiceInstaller
            // 
            this.EmailerServiceInstaller.Description = "Automated email service";
            this.EmailerServiceInstaller.ServiceName = "RN Email Service";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.EmailerServiceProcessInstaller1,
            this.EmailerServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller EmailerServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller EmailerServiceInstaller;
    }
}