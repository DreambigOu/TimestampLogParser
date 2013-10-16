using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TimestampLogParser
{
    class Program
    {
        static void Main(string[] args)
        {            
            // In version 0.4, the input file can be mulitple rather than one input file in before version 0.3
            string[] inputFilePath = { "" };
            string outputFilePath = "";      
            string[] stages = { "" };
            int NumberOfStages = 0;

            char[] delimiterChars = {','};
            SortedDictionary<string, Dictionary<string, ulong>> outterDict = new SortedDictionary<string, Dictionary<string, ulong>>();
            Dictionary<string, string> EventDict = new Dictionary<string, string>();
            SortedDictionary<string, List<KeyValuePair<string, ulong>>> resultDict = new SortedDictionary<string, List<KeyValuePair<string, ulong>>>();
            Dictionary<string, ulong> innerDict = new Dictionary<string, ulong>();
                        
            // Read parameters             
            // Command line usage: >> .\TimestampLogParser.exe "Input1.txt,Input2.txt...InputN.txt" Output.txt "StageName1,StageName2,StageName3"
            // "StageName1,StageName2,StageName3" should be in the order. The parser will base on this order to calculate the time duration
            if (args.Length == 0)
            {
                Console.WriteLine("Message Log Parser usage:");
                Console.WriteLine("\t >> .\\TimestampLogParser.exe \"Input1.txt,Input2.txt...InputN.txt\" Output.txt \"Stage1,Stage2,Stage3\"");
                Console.WriteLine("\t \"Stage1,Stage2,Stage3\" should be in the order.");
                Console.WriteLine("The parser will calculate the time duration base on this order");
                Console.WriteLine("\t Try: .\\TimestampLogParser.exe \"Input1.txt,Input2.txt,Input3.txt\" Output.txt \"T1,T2,T3,T4,T5,T6\"");
            }
            
            if (args.Length == 3)
            {
                // Input file path
                if (args[0] != "")
                {
                    // ToDo: Error Checking

                    // Split the path of input files
                    inputFilePath = args[0].Split(delimiterChars);

                    foreach (string inputFile in inputFilePath)
                    {
                        if (File.Exists(inputFile))
                        {
                            Console.WriteLine("Input file: " + inputFile + " exists.");
                        }
                        else
                        {
                            Console.WriteLine("Input file: " + inputFile + " does not exists.");
                            Console.WriteLine("Please retry.");
                            return;
                        }
                    }
                }

                // Output file path
                if (args[1] != "")
                {
                    // ToDo: Error Checking                 
                    outputFilePath = args[1];        
                }
                    
                // Stage name
                if (args[2] != "")
                {
                    // ToDo: Error Checking                                     
                    stages = args[2].Split(delimiterChars);
                    NumberOfStages = stages.Length;        
                }

                Console.WriteLine("Number of input file(s): {0}", inputFilePath.Length);
                Console.WriteLine("Output file path: {0}", outputFilePath);
                Console.WriteLine("Number of stage(s): {0}", NumberOfStages);
                Console.Write("Stages: ");
                foreach( string stage in stages)
                {
                    Console.Write("{0} ", stage);                                
                }
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("Parameters error. Please retry");
                return;
            }
                         
            // Read input log file into dictionary
            foreach(string inputFile in inputFilePath)
            {
                using (TextReader textReader = new StreamReader(inputFile))
                {
                    string line = "";

                    // Read all lines into dictionary  
                    while ((line = textReader.ReadLine()) != null)
                    {
                        // Split line by delimiter
                        string[] parsedLine = line.Split(delimiterChars);                      
                    
                        // Extract MessageID, ,Event, Stage, Timestamp
                        string MessageID    = parsedLine[0].Trim();
                        string Event        = parsedLine[1].Trim();
                        string Stage        = parsedLine[2].Trim();

                        // ToDo: Make timestamp to UTC time
                        ulong Timestamp     = Convert.ToUInt64(parsedLine[3].Trim());
                    
                        // Insert data to dictionary
                        if (!outterDict.ContainsKey(MessageID))
                        {
                            // If the MessageID does not exist, then create a new dictionary
                            innerDict = new Dictionary<string, ulong>();
                            innerDict.Add(Stage, Timestamp);
                            outterDict.Add(MessageID, innerDict);
                        
                            // For the message with the same message ID, the event is the same.
                            // So only add into the dictionary at first time.
                            EventDict.Add(MessageID, Event);
                        }
                        else
                        {
                            outterDict[MessageID].Add(Stage, Timestamp);
                        }                                           
                    } // end of while  
                }
            } // end of forwach

            // Caculate the difference and put into resultDict
            foreach (KeyValuePair<string, Dictionary<string, ulong>> outterKVP in outterDict)
            {
                List<KeyValuePair<string, ulong>> timeDurationList = new List<KeyValuePair<string, ulong>>();
                Dictionary<string, ulong> timeDict = outterKVP.Value;
                string MessageID = outterKVP.Key;
                ulong overallDuaration = 0;

                // Check availability of all stages
                foreach (string stage in stages)                
                {                    
                    if (!timeDict.ContainsKey(stage))
                    {
                        // error handling
                        Console.WriteLine("Message ID: {0} does not have {1} message ", MessageID, stage);
                        // Assign 0 represents null
                        timeDict[stage] = 0;
                    }
                }

                // Calculate the time duation between each stage
                for (int i = 1; i < NumberOfStages; i++)
                {                    
                    // Current stage: begin with T2;
                    string CurrentStage = stages[i];
                    // Previous stage: begin with T1;
                    string PrevStage = stages[i-1];
                    string Index = CurrentStage + "-" + PrevStage;
                    ulong Duration      = timeDict[CurrentStage] - timeDict[PrevStage];
                    overallDuaration    += Duration;
                                        
                    timeDurationList.Add(new KeyValuePair<string, ulong>(Index, Duration));
                }
                
                timeDurationList.Add(new KeyValuePair<string, ulong>("Overall", overallDuaration));                
                resultDict.Add(outterKVP.Key, timeDurationList);
            }

            // Write resultDict to file
            StreamWriter outputFile = new StreamWriter(outputFilePath);
            string row = "";
            row = "MessageID"+","+"Event";
            // Wirte first row to output file
            foreach (KeyValuePair<string, ulong> innerKVP in resultDict.ElementAt(0).Value)
            {
                row += ("," + innerKVP.Key);            
            }
            outputFile.WriteLine(row);

            // Wirte data to output file            
            foreach (KeyValuePair<string, List<KeyValuePair<string, ulong>>> outterKVP in resultDict)
            {
                row = "";

                // Output pattern: MessageID,Event,StageN - StageN-1....,Overall
                row = outterKVP.Key;
                string Event;
                if (!EventDict.TryGetValue(outterKVP.Key, out Event))
                { 
                    // error handling: Cannot find the event with specified MessageID (It is impossible)
                }
                
                row += ("," + Event);

                foreach (KeyValuePair<string, ulong> innerKVP in outterKVP.Value)
                {
                    row += ("," + innerKVP.Value);
                }
                outputFile.WriteLine(row);
            }
            outputFile.Close();
            Console.WriteLine("Finished");           
        }
    }
}
