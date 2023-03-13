using System;
using System.IO;
using TwinCAT.Ads;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace pdms_one_csv_streamwriter_modification_v2
{
    class Program
    {
        static string netId = "169.254.24.40.1.1";
        static int port = (int)AmsPort.PlcRuntime_851;

        // desktop path of the .csv file to save the samples
        static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString(), "test.csv");
        static string mpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString(), "main.csv");

        // ads symbol symbols for which the samples are collected
        // AdsSymbol adssymbol1 = new AdsSymbol("GVL_Datalogger.St_30_VAR.fActPos", typeof(double), 0, new NotificationSettings(AdsTransMode.Cyclic, 1, 0));
        static AdsSymbol adssymbol1 = new AdsSymbol("GVL_Datalogger.St_30_VAR.fCurrentScaled", typeof(double), 0, new NotificationSettings(AdsTransMode.Cyclic, 1, 0));
        static AdsSymbol adssymbol2 = new AdsSymbol("GVL_Datalogger.St_30_VAR.nSchneidenzahler", typeof(int), 0, new NotificationSettings(AdsTransMode.Cyclic, 1, 0));
        static AdsSymbol adssymbol3 = new AdsSymbol("GVL_Datalogger.St_31_VAR.nSchritt", typeof(short), 0, new NotificationSettings(AdsTransMode.Cyclic, 1, 0));

        static List<AdsSymbol> adssymbols = new List<AdsSymbol>();

        // object to collect data and start a measurement using a ads symbol
        static TraceAdsSymbol trace = null;// new TraceAdsSymbol(netId, port, adssymbol1, adssymbol2);

        static AdsClient adsclient = new AdsClient();

        static bool read_bool = true;
        //string pythonScriptPath = "real_test_2_variables.py";

        static void Main(string[] args)
        {
            //Parallel.Invoke( () => read_data(), () => pdms_py(mpath) );
            read_data();
        }

        static void read_data()
        {
            //Console.WriteLine("Hello World!");
            adssymbols.Add(adssymbol1);
            adssymbols.Add(adssymbol2);
            adssymbols.Add(adssymbol3);

            trace = new TraceAdsSymbol(netId, port, adssymbols);

            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine(DateTime.Now.ToString() + ":\t" + i);
                var_trace();
                read_bool = false;
                pdms_py(mpath);
            }
        }

        static void var_trace()
        {
            // start trace measurement for 5 seconds ( 5000 )
            trace.Start(60*1000); // 60*1000 = 1 minute

            // event to recognized that the measurement is finished
            trace.Completed += tracecompleted;

            adsclient.Connect(netId, 1000);

            Thread.Sleep(11000);
        }

        /// <summary>event is fired by the trace object when the specified time has elapsed</summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">data of the event</param>
        static void tracecompleted(object sender, EventArgs e)
        {
            try
            {
                // stringbuilder to copy collected data
                StringBuilder stringbuilder = new StringBuilder();

                // copy stringbuilder
                stringbuilder = trace.stringbuilder;

                // save stringbuilder as string in csv-file on the desktop
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.Write(stringbuilder.ToString());
                    writer.Flush();
                    Console.WriteLine("Temp Flush Done");
                }
                // Read the contents of the source file
                string fileContents = File.ReadAllText(path);

                using (StreamWriter sw = new StreamWriter(mpath, true))
                {
                    sw.Write(fileContents);
                    sw.Flush();
                    Console.WriteLine("Main Flush Done");
                }

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex.Message);
                // add additional debug statements to help identify the issue
            }
        }



        // sending the main csv file to python script
        static void pdms_py(string mpath)
        {
            if (read_bool == false)
            {
                

                //"C:\Program Files\Python37\python.exe"
                string pythonPath = @"C:/Program Files/Python37/python.exe";
                string scriptPath = @"C:/Users/Q610267/source/repos/pdms_one_csv_streamwriter_modification_v2/pdms_one_csv_streamwriter_modification_v2/real_test_2_variables.py";
                string arguments = mpath;

                // Create a new process start info object
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pythonPath;
                //startInfo.Arguments = string.Format("\"{0}\" \"{1}\"", scriptPath, arguments);
                startInfo.Arguments = scriptPath;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                using (Process process = Process.Start(startInfo))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        //Console.WriteLine(result);

                    }
                }
                
            }
        }
    }
}
