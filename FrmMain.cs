using System.Diagnostics;
using System.Security.Cryptography;

namespace OUTRIDERS_EAC_Changer
{
    public partial class Form1 : Form
    {
        public static string CurrentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.Default.EACCurrentFileName);
        public static string BackupBypassFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.Default.EACFilePath, Config.Default.EACBypassFileName);
        public static string BackupOriginalFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.Default.EACFilePath, Config.Default.EACOriginalFileName);

        protected FileCheckResult CurrentFileVersion { get; set; }
        public Form1()
        {
            InitializeComponent();
            CurrentFileVersion = new FileCheckResult() { Version = FileVersion.Unknown, FileHash = "" };
        }

        private void bwkrGetfileinfo_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (e.Argument is string filepath)
            {
                FileCheckResult ret = new FileCheckResult();
                try
                {
                    var md5 = ComputeMD5(filepath);
                    ret.Version=CheckFileVersion(md5);
                    ret.FileHash = md5;
                }catch(Exception ex)
                {
                    ret.Version = FileVersion.Unknown;
                    ret.FileHash = ex.Message;
                }
                finally
                {
                    e.Result=ret;
                }
               
            }
            else
            {
                e.Result = new FileCheckResult() { Version = FileVersion.Unknown };
            }
        }

        private void bwkrGetfileinfo_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result is FileCheckResult result)
            {
                label2.Text = $"[{result.Version}]{result.FileHash}";
                CurrentFileVersion = result;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bwkrGetfileinfo.RunWorkerAsync(CurrentFilePath);
        }

        private FileVersion CheckFileVersion(string md5)
        {
            if (md5==Config.Default.EACBypassHash)
                return FileVersion.Bypass;

            if (md5 == Config.Default.EACOriginalHash)
                return FileVersion.Original;

            return FileVersion.Unknown;
        }

        private string ComputeMD5(string filename)
        {
            //var filebytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.Default.EACCurrentFileName));
            var filebytes = File.ReadAllBytes(filename);
            MD5 hash = MD5.Create();
            return string.Join("", hash.ComputeHash(filebytes).Select(b => b.ToString("x2"))).ToUpper();
        }

        protected enum FileVersion
        {
            Original,
            Bypass,
            Unknown
        }

        protected class FileCheckResult
        {
            public FileVersion Version { get; set; }

            public string FileHash { get; set; } = "";
        }

        private void label2_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show(label2.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChangeEACVersion(FileVersion.Bypass);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ChangeEACVersion(FileVersion.Original);
        }

        private void ChangeEACVersion(FileVersion ver)
        {
            try
            {
                if (!bwkrGetfileinfo.IsBusy)
                {
                    bool needcheck = false;
                    switch (ver)
                    {
                        case FileVersion.Original:
                            if (CurrentFileVersion.Version== FileVersion.Original)
                            {
                                MessageBox.Show($"Already {CurrentFileVersion.Version}");
                            }
                            else
                            {
                                File.Copy(BackupOriginalFileName, CurrentFilePath, true);
                                needcheck = true;
                            }
                            
                            break;
                        case FileVersion.Bypass:

                            if (CurrentFileVersion.Version== FileVersion.Bypass)
                            {
                                MessageBox.Show($"Already {CurrentFileVersion.Version}");
                            }
                            else
                            {
                                File.Copy(BackupBypassFileName, CurrentFilePath, true);
                                needcheck = true;
                            }
                            break;
                        case FileVersion.Unknown:
                        default:
                            break;
                    }
                    if (needcheck)
                    {
                        bwkrGetfileinfo.RunWorkerAsync(CurrentFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("bwkrGetfileinfoÕýÃ¦");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { Arguments = Config.Default.OutridersStartupArgument, FileName = Config.Default.OutridersStarProgram, WorkingDirectory = Path.GetDirectoryName(Config.Default.OutridersStarProgram) };
            Process.Start(startInfo);
        }
    }
}