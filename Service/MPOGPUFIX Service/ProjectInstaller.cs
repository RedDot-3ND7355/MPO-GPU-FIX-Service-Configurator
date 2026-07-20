using System.ComponentModel;
using System.Configuration.Install;

namespace MPOGPUFIX_Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
