using System.IO;

namespace Box.FileResizerLibrary
{
	public interface IFileResizer
	{
		Stream GetFileWithLimitedSize(string filename, int maxFileSize, bool usePngEncoder);
	}
}