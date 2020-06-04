using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
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
        private static string keyName = "";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest2;
        private static IAmazonS3 s3Client;
        static int noOfFiles;

        public static void Main()
        {
            RedText("*Make sure to provide AWS profile in the APP.config file* \n");

            string s3File = "";
            Console.WriteLine("Input Bucket Name");
            bucketName = Console.ReadLine();

            Console.WriteLine("Provide a name for the uploaded object");
            keyName = Console.ReadLine();

            // Retrieves the number of files to upload and the relevant file paths 
            List<string> s3FileToUpload = new List<string>();
            s3FileToUpload.AddRange(FilePath());

            // Loops until no files to upload are left
            for (int i = 0; i < s3FileToUpload.Count; i++)
            {
                s3File = s3FileToUpload[i];

                s3Client = new AmazonS3Client(bucketRegion);
                Console.WriteLine("Uploading an object");
                UploadObjectAsync(s3File).Wait();
            }

           
        }

        private static async Task UploadObjectAsync(string s3File)
        {
            // Create list to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

            // Setup information required to initiate the multipart upload.
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            // Initiate the upload.
            InitiateMultipartUploadResponse initResponse =
                await s3Client.InitiateMultipartUploadAsync(initiateRequest);

            // Upload parts.
            long contentLength = new FileInfo(s3File).Length;
            long partSize = 1048576;

            try
            {
                Console.WriteLine("Uploading parts");

                long filePosition = 0;
                for (int i = 1; filePosition < contentLength; i++)
                {
                    UploadPartRequest uploadRequest = new UploadPartRequest
                    {
                        BucketName = bucketName,
                        Key = keyName,
                        UploadId = initResponse.UploadId,
                        PartNumber = i,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        FilePath = s3File
                    };

                    // Track upload progress.
                    uploadRequest.StreamTransferProgress +=
                        new EventHandler<StreamTransferProgressArgs>(UploadPartProgressEventCallback);

                    // Upload a part and add the response to our list.
                    uploadResponses.Add(await s3Client.UploadPartAsync(uploadRequest));

                    filePosition += partSize;
                }

                // Setup to complete the upload.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = initResponse.UploadId
                };
                completeRequest.AddPartETags(uploadResponses);

                // Complete the upload.
                CompleteMultipartUploadResponse completeUploadResponse =
                    await s3Client.CompleteMultipartUploadAsync(completeRequest);
            }
            catch (Exception exception)
            {
                Console.WriteLine("An AmazonS3Exception was thrown: { 0}", exception.Message);

                // Abort the upload.
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = initResponse.UploadId
                };
                await s3Client.AbortMultipartUploadAsync(abortMPURequest);
            }
        }
        public static void UploadPartProgressEventCallback(object sender, StreamTransferProgressArgs e)
        {
            // Process event. 
            Console.WriteLine("{0}/{1}", e.TransferredBytes, e.TotalBytes);
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
