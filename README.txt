Timestamp Log Parser (TLP)

Version:0.4
Author: Dreambig Ou (dreambig.ou[at]gmail.com)

Description:
	Timestamp Log Parser (TLP) is deisgned to pre-process the log data. Developers can use the output file for other numerical analyis software such as Excel. TLP is for calcuating the time consuming between each stage in software components. For example, a messages oriented middleware (MOM) is using to deliver the message to another software component. We want to evaluate how much time does a message spend between each processing stage of software components.

Format:
	Input file:
		Input file could be one or multiple text files (.txt or .csv). Each line records a data.
		The format should be:
			MessageID,Event,StageName,Timestamp

			MessageID:
				Datatype: sting or integer.
				Description: MessageID should be unique identified.

			Event:
				Datatype: sting or integer.
				Description: To indentify what type of record.

			StageName:
				Datatype: sting or integer.
				Description: To identify which stage when records the data.

			Timestamp:
				Datatype: integer.
				Description: Current version of TLP only supports integer. Timestamp should be monotonic increasing (avoid time drift problem in the OS). For unix developers, you can use "CLOCK_MONOTONIC" in "clock_gettime". We will make it support other formated timestamp in the next version.

	Output file:
		Output file is calculate by TLP to record time period between each stage. The first row records the header line of the file. The 3rd clounmn to the (n-1)-th column records the time period spending on each stages. The n-th column records the overall time sepnding on the all software components. The record data start at the second row of output file.			 	
		
		Exapmle: MessageID,Event,T1-T0,T2-T1,T3-T2,T4-T3,Overall

Usage:
	>> .\TimestampLogParser.exe "Input1.txt,Input2.txt...InputN.txt" Output.txt "Stage1,Stage2,Stage3"

	First parameter:
		Input files. Mutiple files should be separated by comma (,). The connected string shoud be placed in the double quotes (""). No sapce between each stage.

	Second parameter:
		Output file name. If it does not exist, the TLP will create it.

	Third parameter:
		Stage name shown in the input files. Mutiple stages should be separated by comma (,). The connected string shoud placed in the double quotes ("").  No sapce between each stage.

	Sample: 
		The sample input files are Input1.txt to Input3.txt. There are two types of message (i.e., "event" shown in the input file) and one message each. There are three components to process the messages and each component has two stage ex: T1 (into the component) and T2 (out of the component) for component 1. (Note: "Stage1,Stage2,Stage3\" should be in the order.)

		Try: .\TimestampLogParser.exe "Input1.txt,Input2.txt,Input3.txt" Output.txt "T1,T2,T3,T4,T5,T6"
