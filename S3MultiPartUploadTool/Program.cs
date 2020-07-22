using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Program
{
    class Program
    {
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest2;
        private static IAmazonS3 s3Client;
       

        public static void Main()
        {
            RedText("*Make sure to set your AWS Profile name to default in the credentials file* \n");

            
            Console.WriteLine("Input Bucket Name");
            var bucketName = Console.ReadLine();

            // Retrieves the number of files to upload and the relevant file paths 
            List<string> filePathToUpload = new List<string>();
            filePathToUpload.AddRange(FilePath());

            var fileNumber = 1;
            // Loops until no files to upload are left
            for (int i = 0; i < filePathToUpload.Count; i++)
            {
                var filePath = filePathToUpload[i];
                
                s3Client = new AmazonS3Client(bucketRegion);
                Console.WriteLine("Uploading file " + fileNumber++);
                
                UploadFileAsync(filePath, bucketName).Wait();


            }
            Console.WriteLine("All Files Successfully Uploaded ");
            Console.ReadLine();


        }

        private static async Task UploadFileAsync(string filePath,  string bucketName)
        {
            try
            {
                var fileTransferUtility =
                    new TransferUtility(s3Client);

                
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
                    // You need to change the storage class depending on where you want the file to be stored on AWS
                    StorageClass = S3StorageClass.Glacier,
                    // Part sizes need to be in powers of 2 
                    PartSize = 134217728

                };

                // This is MetaData for use on AWS. You can remove or add these depending on whether you need them or not.
                //fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                //fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                Console.WriteLine("Upload completed");
                
                
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

        // Requests the number of files to upload
        // Requests the file path of each file
        public static List<String> FilePath()
        {
            List<string> filePaths = new List<string>();
            var noOfFiles = 0;
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
                    RedText("Invalid Input");
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

        // Presents important messages in red text
        public static string RedText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;

            return text;
        }
    }
}
