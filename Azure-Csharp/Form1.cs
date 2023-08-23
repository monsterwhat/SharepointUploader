using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
using System;
using System.IO;
using System.Windows.Forms;

namespace Azure_Csharp
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            subirArchivo();
        }

        private void examButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:/";
            this.openFileDialog1.Title = "Seleccione un archivo";
            this.openFileDialog1.FileName = "*.*";
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog1.FileName;
                ArchivoTxt.Text = selectedFilePath;
            }
        }

        public void subirArchivo()
        {
            string siteUrl = "https://{your_tenant}.sharepoint.com/sites/{your_site_name}"; //Nombre del sitio de SharePoint
            string libraryName = "Documentos"; // Nombre de la biblioteca en la que deseas subir el archivo
            string fileName = "test.pdf"; // Nombre del archivo que deseas subir
            string filePath = "C:\\Users\\USER\\Desktop"; // Ruta local del archivo
            string clientId = "your_app_client_id";// ID de cliente de la aplicación en Azure AD
            string clientSecret = "valor_secreto_del_app";// Secreto de cliente de la aplicación en Azure AD

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri("https://login.microsoftonline.com/{your_tenant}.onmicrosoft.com/"))
                .Build();

            string[] scopes = new string[] { "https://{your_tenant}.sharepoint.com/.default" };
            var result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result;

            using (ClientContext context = new ClientContext(siteUrl))
            {
                context.ExecutingWebRequest += (s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + result.AccessToken;
                };

                Web web = context.Web;
                List documentsLibrary = web.Lists.GetByTitle(libraryName);

                context.Load(documentsLibrary.RootFolder);
                context.ExecuteQuery();

                // Crea el archivo en la biblioteca de SharePoint
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    FileCreationInformation fileInfo = new FileCreationInformation
                    {
                        ContentStream = fs,
                        Url = Path.Combine(documentsLibrary.RootFolder.ServerRelativeUrl, fileName),
                        Overwrite = true
                    };

                    Microsoft.SharePoint.Client.File newFile = documentsLibrary.RootFolder.Files.Add(fileInfo);
                    context.Load(newFile);
                    context.ExecuteQuery();

                    MessageBox.Show("Archivo subido correctamente.");
                }
            }
        }
    }
}
