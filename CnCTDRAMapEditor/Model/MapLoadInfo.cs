using MobiusEditor.Interface;

namespace MobiusEditor.Model
{
    public class MapLoadInfo
    {
        public string FileName;
        public FileType FileType;
        public IGamePlugin Plugin;
        public string[] Errors;

        public MapLoadInfo(string fileName, FileType fileType, IGamePlugin plugin, string[] errors)
        {
            this.FileName = fileName;
            this.FileType = fileType;
            this.Plugin = plugin;
            this.Errors = errors;
        }
    }
}
