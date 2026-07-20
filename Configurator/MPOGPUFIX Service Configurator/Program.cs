namespace MPOGPUFIX_Service_Configurator
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            Application.ThreadException += (s, e) => { MessageBox.Show(e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);


            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}