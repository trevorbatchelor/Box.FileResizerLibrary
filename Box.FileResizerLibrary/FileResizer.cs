using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace Box.FileResizerLibrary
{
	public class FileResizer : IFileResizer
	{
		const int MAXFILESIZE = 524288;

		/// <summary>
		/// Converts an image file into a stream with limited file size.
		/// </summary>
		/// <returns>
		/// Returns a memory stream containing the resized image or null if the image cannot be loaded.
		/// </returns>
		/// <remarks>
		/// This class uses the SixLabors ImageSharpe library to resize images.
		/// </remarks>
		/// <param name="filename">The full path name of the file to be loaded.</param>
		/// <param name="maxFileSize">The maximum size in bytes of the memory stream returned. The default size is 524288 bytes.</param>
		/// <param name="usePngEncoder">Specifies whether the memory stream should be returned in a PNG format. By default, the JPG format is used.</param>
		public Stream GetFileWithLimitedSize(string filename, int maxFileSize = MAXFILESIZE, bool usePngEncoder = false)
		{
			MemoryStream imageStream = null;

			// check that file exists
			if (!File.Exists(filename))
			{
				Console.WriteLine($"File \"{filename}\" was not found.");
				return imageStream;
			}

			// load image from file
			using (Image image = LoadImageFile(filename))
			{
				if (image == null)
				{
					return imageStream;
				}

				// select encoder (PNG or JPG) from factory
				IImageEncoder imageEncoder = GetEncoder(usePngEncoder);

				// determine optimum size
				Size size = GetOptimumImageSize(image, maxFileSize, imageEncoder);
				Console.WriteLine($"{size.Width} x {size.Height}");

				// resize image
				ResizeImage(image, size);

				// create new stream
				imageStream = new MemoryStream();
				image.Save(imageStream, imageEncoder);
			}

			// return memory stream
			return imageStream;
		}

		private Image LoadImageFile(string filename)
		{
			Image image = null;

			try
			{
				image = Image.Load(filename);
			}
			catch (Exception e)
			{
				Console.WriteLine($"File \"{filename}\" could not be loaded.");
			}

			return image;
		}

		private Size GetOptimumImageSize(Image image, int maxFileSize, IImageEncoder imageEncoder)
		{
			Size size = image.Size();
			int maxPossibleWidth = size.Width;
			int minPossibleWidth = GetMinimumPossibleWidth(size, maxFileSize);

			long imageFileSize = 0;
			long lastImageFileSize;
			Size trySize = size;

			// use binary search to find maximum size within filesize constraint
			do
			{
				lastImageFileSize = imageFileSize;
				imageFileSize = GetImageFileSize(image, trySize, imageEncoder);

				if (imageFileSize > maxFileSize)
				{
					maxPossibleWidth = trySize.Width - 1;
				}
				else
				{
					minPossibleWidth = trySize.Width;
				}

				int tryWidth = (minPossibleWidth + maxPossibleWidth) / 2;
				int tryHeight = (tryWidth * size.Height) / size.Width;
				trySize = new Size(tryWidth, tryHeight);
			} while (maxPossibleWidth > (minPossibleWidth + 1));

			return new Size(minPossibleWidth, (minPossibleWidth * size.Height) / size.Width);
		}

		private IImageEncoder GetEncoder(bool usePngEncoder)
		{
			if (usePngEncoder)
			{
				return new PngEncoder();
			}

			return new JpegEncoder
			{
				Quality = 95,
				Subsample = JpegSubsample.Ratio420
			};
		}

		private int GetMinimumPossibleWidth(Size size, int maxFileSize)
		{
			return (size.Width == 0 || size.Height == 0) ? 0 : maxFileSize / (4 * size.Height);
		}

		private long GetImageFileSize(Image image, Size size, IImageEncoder imageEncoder)
		{
			long result = 0;

			using (var resizedImage = image.Clone(imageProcessingContext =>
			{
				imageProcessingContext.Resize(new ResizeOptions
				{
					Mode = ResizeMode.Max,
					Size = new Size(size.Width, size.Height)
				});
			}))
			{
				using (var tmpStream = new MemoryStream())
				{
					tmpStream.SetLength(0);
					resizedImage.Save(tmpStream, imageEncoder);
					result = tmpStream.Position;
				}
			}

			return result;
		}

		void ResizeImage(Image image, Size size)
		{
			image.Mutate(imageProcessingContext =>
			{
				imageProcessingContext.Resize(new ResizeOptions
				{
					Mode = ResizeMode.Max,
					Size = size
				});
			});

		}
	}
}
