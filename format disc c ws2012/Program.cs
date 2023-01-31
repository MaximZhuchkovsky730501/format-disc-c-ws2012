using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace format_disk_C
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Убедитесь, что вы достали все флешки, не подлежащие отчисткке, из компьютера!\n" +
                    "Вы увереры, что хотите удалить всю информацию с этого ПК? (yes/no)");
                string answer = Console.ReadLine();
                if (answer == "yes" || answer == "y" || answer == "YES" || answer == "Y" || answer == "Yes")
                {
                    string[] logical_driver = Environment.GetLogicalDrives();
                    List<string> list = new List<string>();
                    foreach (string driver in logical_driver)
                    {

                    }
                    foreach (string disc in Environment.GetLogicalDrives())
                    {
                        Format_all format_All = new Format_all(disc);
                        format_All.stop_oracle_services();
                        //format_All.kill_all_process();
                        Console.WriteLine("Удаление файлов");
                        format_All.delete_all(format_All.directoryInfo);
                        create_random_file();
                    }
                }
                else
                {
                    Console.WriteLine("Выполнение программы отменено");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
        public static void create_random_file()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            try
            {
                foreach (DriveInfo drive in drives)
                {
                    if (drive.IsReady)
                    {
                        int n = 0;
                        Console.WriteLine("Заполнение памяти...");
                        while (drive.TotalFreeSpace > 0)
                        {
                            long buffer_size;
                            if (drive.TotalFreeSpace > 4096)
                                buffer_size = 4096;
                            else
                                buffer_size = drive.TotalFreeSpace;
                            FileStream file = File.Open(drive + "\\" + n.ToString(), FileMode.Create, FileAccess.Write);
                            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                            byte[] buffer = new byte[buffer_size];
                            long writed = 0;
                            long size = drive.TotalFreeSpace;
                            file.SetLength(size);
                            while (writed < size)
                            {
                                rng.GetBytes(buffer);
                                file.Write(buffer, 0, buffer.Length);
                                writed += buffer_size;
                            }
                            file.Close();
                            n++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }

    class Format_all
    {
        public DirectoryInfo directoryInfo;
        public int deep;
        public Dictionary <int,string> current_directory;
        public struct Count
        {
            public int count_file, count_folders;
        }

        public Format_all(string disc)
        {
            directoryInfo = new DirectoryInfo(disc);
            deep = 0;
            current_directory = new Dictionary<int,string>();
            int n = 0;
            foreach (string folder in Environment.CurrentDirectory.Split('\\'))
            {
                string directory = "";
                if (n > 0)
                {
                    directory = directory + current_directory[n-1] + "\\";
                }
                directory = directory + folder;
                current_directory.Add(n++, directory);
            }
        }

        public void delete_all(DirectoryInfo directory)
        {
            deep++;
            Count count = new Count();
            count.count_file = 0;
            count.count_folders = 0;
            Console.WriteLine("Удаление файлов из каталога: " + directory.Name);
            try
            {
                foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                {
                    if (subdirectory.FullName != Environment.CurrentDirectory)
                    {
                        delete_all(subdirectory);
                    }
                }
                foreach (FileInfo fileInfo in directory.GetFiles())
                {
                    fileInfo.Delete();
                    //Console.WriteLine(fileInfo.Name);
                    count.count_file++;
                }
                foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                {
                    if (deep < current_directory.Count)
                    {
                        if (subdirectory.FullName != current_directory[deep])
                        {
                            subdirectory.Delete(true);
                            //Console.WriteLine(subdirectory.FullName);
                            count.count_folders++;
                        }
                        else 
                        { }
                    }
                    else
                    {
                        subdirectory.Delete(true);
                        //Console.WriteLine(subdirectory.FullName);
                        count.count_folders++;
                    }
                }
                if (deep < current_directory.Count)
                {
                    if (!directory.FullName.StartsWith(current_directory[deep]))
                    {
                        directory.Delete(true);
                        Console.WriteLine(directory.FullName);
                    }
                    else
                    { }
                }
                else
                {
                    directory.Delete(true);
                    Console.WriteLine(directory.FullName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            deep--;
            Console.WriteLine("Из каталога :" + directory.Name + " удалено " + count.count_folders + " папок, " + count.count_folders + " папок.");
        }

        public void kill_all_process()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        process.Kill();
                        Console.WriteLine(process.ProcessName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void stop_all_services()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                try
                {
                    //Console.WriteLine(service.ServiceName + ' ' + service.CanStop);
                    if (service.CanStop)
                    {
                        service.Stop();
                        Console.WriteLine(service.ServiceName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void stop_oracle_services()
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                try
                {
                    //Console.WriteLine(service.ServiceName + ' ' + service.CanStop);
                    if (service.CanStop && service.ServiceName.StartsWith("Oracle"))
                    {
                        service.Stop();
                        Console.WriteLine(service.ServiceName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
