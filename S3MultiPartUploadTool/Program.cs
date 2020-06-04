using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amazon.DocSamples.S3
{
    class UploadFileMPULowLevelAPITest
    {
        private static string bucketName = "";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest2;
        private static IAmazonS3 s3Client;
        static int noOfFiles;

        public static void Main()
        {
            RedText("*Make sure to provide AWS profile in the APP.config file* \n");

            
            Console.WriteLine("Input Bucket Name");
            bucketName = Console.ReadLine();

            // Retrieves the number of files to upload and the relevant file paths 
            List<string> filePathToUpload = new List<string>();
            filePathToUpload.AddRange(FilePath());

            // Loops until no files to upload are left
            for (int i = 0; i < filePathToUpload.Count; i++)
            {
                var filePath = filePathToUpload[i];

                s3Client = new AmazonS3Client(bucketRegion);
                Console.WriteLine("Uploading an object");
                UploadFileAsync(filePath).Wait();
            }

           
        }

        private static async Task UploadFileAsync(string filePath)
        {
            try
            {
                var fileTransferUtility =
                    new TransferUtility(s3Client);

                // Option 4. Specify advanced settings.
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
                    StorageClass = S3StorageClass.Glacier,
                    PartSize = 1048576
                    
                };
                fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                Console.WriteLine("Upload 4 completed");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }
        public static List<String> FilePath()
        {
            List<string> filePaths = new List<string>();

            // Loop to stop incorrect datatype exception 
            bool input = true;
            while (input)
            {
                try
                {
                    Console.WriteLine("How Many files are you uploading? ");
                    noOfFiles = Convert.ToInt32(Console.ReadLine());

                    if (noOfFiles >= 1)
                    {
                        input = false;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid Input");
                }
            }

            for (int i = 0; i < noOfFiles; i++)
            {
                var counter = i + 1;
                bool incorrect = true;
                Console.WriteLine("Input File Path " + counter + ": ");
                while (incorrect)
                {
                    string file = Console.ReadLine();

                    if (filePaths.Contains(file))
                    {
                        RedText("File Already Added");
                    }
                    else if (File.Exists(file))
                    {
                        filePaths.Add(file);
                        incorrect = false;
                    }
                    else
                    {
                        RedText("Invalid file path");
                        Console.WriteLine("Input File Path " + counter + " (e.g. C:\\Users\\User\\Documents\\Image.jpg)");
                    }
                }

            }
            return filePaths;
        }
        public static string RedText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;

            return text;
        }

       

    }
}
