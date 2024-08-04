using Aspose.Zip.Gzip;
using Aspose.Zip.SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SteamAchievmentViewer.Form1;
using static System.Net.Mime.MediaTypeNames;

namespace SteamAchievmentViewer
{
    public partial class HashMapGenTool : Form
    {
        public delegate void AddTextDelegate(String text = "");
        String emuDeckRomPath = "";
        String emuDeckCachePath = "";
        Dictionary<String, Dictionary<String, String>> hashMap = new Dictionary<String, Dictionary<String, String>>();
        // Add Finished Event
        private Action<String> finishedProcessingAction = null;
        public Action hashMapReady = null;
        String hashErrorLogPath = "";


        public HashMapGenTool(String emuDeckRomPath, string emuDeckCachePath)
        {
            InitializeComponent();
            this.emuDeckRomPath = emuDeckRomPath;
            finishedProcessingAction += processingDone;
            this.emuDeckCachePath = emuDeckCachePath;
            hashErrorLogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\hashErrors.txt";

            displayTxtBox.DetectUrls = false;

            this.BackColor = Color.FromArgb(80, 84, 82);
            this.ForeColor = Color.White;

            displayTxtBox.BackColor = Color.FromArgb(120, 120, 120);
            displayTxtBox.ForeColor = Color.White;

            // Set Minimum to 1 to represent the first file being copied.
            progressBar.Minimum = 1;
            // Set the initial value of the ProgressBar.
            progressBar.Value = 1;
            // Set the Step property to a value of 1 to represent each file being copied.
            progressBar.Step = 1;
        }

        private void processingDone(String message)
        {
            // Detaches the event handler
            if (hashMapReady != null)
            {
                hashMapReady();
            }
            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
            });
        }

        private void HashMapGenTool_Shown(object sender, EventArgs e)
        {
            if (Directory.Exists(emuDeckRomPath) && Directory.Exists(emuDeckRomPath + $"\\snes"))
            {
                hashMap = loadHashMap(emuDeckCachePath);
                if (File.Exists(this.emuDeckCachePath))
                {
                    File.Delete(this.emuDeckCachePath);
                }
                // Start Generation
                Thread thread = new Thread(getEDHashMap);
                thread.IsBackground = true;
                thread.Start(this.emuDeckRomPath);
            }
            else
            {
                finishedProcessingAction($"Emudeck Path {emuDeckRomPath} does not exist.");
            }
        }

        public void addTextToDisplayBox(String text)
        {
            displayTxtBox.AppendText(text);
            displayTxtBox.ScrollToCaret();
        }

        public void InvokeAddTextToDisplayBox(String text)
        {
            if (this.InvokeRequired)
            {
                object[] parameters = new object[] { text };
                this.Invoke(new AddTextDelegate(addTextToDisplayBox), parameters);
            }
            else
            {
                addTextToDisplayBox(text);
            }
        }
        private List<String> troubleMakers = new List<string>();
        public void updatehashMapForFiles(Object obj)
        {
            List<String> files = (List<String>)obj;
            Parallel.ForEach(files, file =>      //foreach (String file in files)
            {
                InvokeAddTextToDisplayBox($"Processsing File {file}\n");
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar.PerformStep();
                });
                try
                {
                    byte[] data = null;
                    long lastModifiedTime = File.GetLastWriteTimeUtc(file).ToFileTimeUtc();
                    String systemFolderName = file.Replace(emuDeckRomPath, "").Trim().TrimStart('/').TrimStart('\\').Split(new char[] { '\\' })[0].ToLower();
                    List<String> arcadeFolders = new List<String>() { "arcade", "fbneo", "fba", "mame2003", "mame2010", "gamecom", "vsmile" };
                    List<String> zipExt = new List<String>() { ".zip", ".gz", ".gzip", ".7z", ".7zip" };

                    String hash = "";
                    if (zipExt.Contains(Path.GetExtension(file).ToLower()) && !arcadeFolders.Contains(systemFolderName))
                    {
                        hash = DecompressAndHash(file);
                    }
                    else
                    {
                        hash = calcMD5Hash(file);
                    }


                    Dictionary<String, String> tmp = new Dictionary<String, String>() { { "hash", hash }, { "modified", $"{lastModifiedTime}" } };
                    if (hashMap.ContainsKey(file))
                    {
                        hashMap[file] = tmp;
                    }
                    else
                    {
                        hashMap.Add(file, tmp);

                    }

                }
                catch
                {
                    // SKIP FIles if theyre too big 2GB+
                    troubleMakers.Add(file);
                }
            });
        }

        public void getEDHashMap(object obj)
        {
            String path = (String)obj;
            Stopwatch timer = new Stopwatch();
            List<String> files = new List<String>();
            timer.Start();
            int chunkSize = 500;
            int skipCount = 0;

            troubleMakers = new List<String>();

            if (Directory.Exists(path))
            {
                files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Select(s => s).ToList();
                this.Invoke((MethodInvoker)delegate
                {
                    // Set Maximum to the total number of files to copy.
                    progressBar.Maximum = files.Count;
                });

                foreach (KeyValuePair<String, Dictionary<String, String>> kvp in hashMap)
                {
                    if (files.Contains(kvp.Key) && File.GetLastWriteTimeUtc(kvp.Key).ToFileTimeUtc() == long.Parse(kvp.Value["modified"]))
                    {
                        files.Remove(kvp.Key);
                        skipCount++;
                        InvokeAddTextToDisplayBox($"Hash Exists: {kvp.Key}\n");
                        this.Invoke((MethodInvoker)delegate
                        {
                            progressBar.PerformStep();
                        });
                    }
                }

                // Thread every 500 files
                //Parallel.ForEach(files, file =>      //foreach (String file in files)
                //{
                //    InvokeAddTextToDisplayBox($"Processsing File {file}\n");
                //    this.Invoke((MethodInvoker)delegate
                //    {
                //        // Set Maximum to the total number of files to copy.
                //        progressBar.PerformStep();
                //    });
                //    try
                //    {
                //        byte[] data = null;
                //        if (Path.GetExtension(file).ToLower() == ".zip")
                //        {
                //            data = DecompressRom(File.ReadAllBytes(file));
                //        }
                //        else
                //        {
                //            data = File.ReadAllBytes(file);
                //            if (data == null)
                //            {

                //            }
                //        }


                //        hashMap.Add(file, calcMD5Hash(data));
                //    }
                //    catch
                //    {
                //        // SKIP FIles if theyre too big 2GB+
                //        troubleMakers.Add(file);
                //    }
                //});
                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < files.Count; i+=chunkSize){
                    Thread tmp = new Thread(updatehashMapForFiles);
                    tmp.IsBackground = true;
                    threads.Add(tmp);
                    tmp.Start(files.GetRange(i, ((i + chunkSize) < files.Count) ? chunkSize : files.Count - i));
                }

                foreach (Thread tmp in threads)
                {
                    tmp.Join();
                }
            }
            timer.Stop();
            if (hashMap.Keys.Count > 0)
            {
                String emuDeckJson = "{\n";

                emuDeckJson += String.Join(", \n\t", hashMap.Select(kvp => $"\"{kvp.Key.Replace("\\", "\\\\")}\": {{\n\t\t{String.Join(", \n\t\t", kvp.Value.Select(kvp2 => $"\"{kvp2.Key}\" : \"{kvp2.Value}\""))}\n\t}}"));

                emuDeckJson += "\n}";
                File.WriteAllText(emuDeckCachePath, emuDeckJson);
            }
            if (File.Exists(hashErrorLogPath))
            {
                File.Delete(hashErrorLogPath);
            }
            if (troubleMakers.Count > 0)
            {
                File.WriteAllText(hashErrorLogPath, "\n" + String.Join("\n", troubleMakers));
            }

            MessageBox.Show($"Hashed {hashMap.Keys.Count}/{files.Count + skipCount} files in {timer.Elapsed.TotalSeconds} s.{((troubleMakers.Count > 0) ? $"\nFailed to generate a hash for {troubleMakers.Count} files.\nSee hashErrors.txt for more info." : "")}");

            finishedProcessingAction("Hash Map Generation Complete.");
        }


        static String DecompressAndHash(String path)
        {
            String rv = "";
            Stream romData = null;
            String ext = Path.GetExtension(path).ToLower();

            if (ext == ".zip")
            {
                ZipArchive arch = new ZipArchive(File.Open(path, FileMode.Open));
                if (arch.Entries.Count > 0)
                {
                    ZipArchiveEntry ent = arch.Entries[0];
                    romData = arch.Entries[0].Open();
                }
            }
            else if (ext == ".7z" || ext == ".7zip")
            {
                SevenZipArchive archive = new SevenZipArchive(path);
                romData = archive.Entries[0].Open();

            }
            else if (ext == ".gz" || ext == ".gzip")
            {
                GzipArchive archive = new GzipArchive(path);
                romData = archive.Open();
            }
            else
            {
                throw new Exception($"Zip format {ext} not supported");
            }



            if (romData != null)
            {
                rv = calcMD5Hash(romData);
                romData.Close();
            }

            return rv;
        }

        public static String calcMD5Hash(String path)
        {
            String rv = "";
            using (FileStream fs = File.OpenRead(path))
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(fs);

                rv = Convert.ToHexString(hashBytes).ToLower(); // .NET 5 +
                fs.Close();
            }
            return rv;
        }

        public static String calcMD5Hash(Stream data)
        {
            String rv = "";
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);

                rv = Convert.ToHexString(hashBytes).ToLower(); // .NET 5 +
            }
            return rv;
        }
    }
}
