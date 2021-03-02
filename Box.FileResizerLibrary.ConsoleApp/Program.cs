using System;
using System.IO;

namespace Box.FileResizerLibrary.ConsoleApp
{
	class Program
	{
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			Console.WriteLine("Hello.");
			Console.Write("Please enter a file name: ");
			string filename = Console.ReadLine();
			bool saveNewFile = false;
			bool usePngEncoder = true;

			var resizer = new FileResizer();

			using (var fileStream = resizer.GetFileWithLimitedSize(filename, 524288, false))
			{
				if (fileStream == null)
				{
					Console.WriteLine("Unable to process file.");
					return;
				}

				if (saveNewFile)
				{
					using (var newFileStream = File.Create(filename + (usePngEncoder ? ".png" : ".jpg")))
					{
						fileStream.Seek(0, SeekOrigin.Begin);
						await fileStream.CopyToAsync(newFileStream);
					}
				}
			}
		}
	}
}
