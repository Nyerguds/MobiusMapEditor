using MobiusEditor.Interface;

namespace MobiusEditor.Model
{
    public class MapLoadInfo
    {
        public string FileName;
        public FileType FileType;
        public IGamePlugin Plugin;
        public bool MapLoaded;
        public string[] Errors;

        public MapLoadInfo(string fileName, FileType fileType, IGamePlugin plugin, string[] errors, bool mapLoaded)
        {
            this.FileName = fileName;
            this.FileType = fileType;
            this.Plugin = plugin;
            this.MapLoaded = mapLoaded;
            this.Errors = errors;
        }

        public MapLoadInfo(string fileName, FileType fileType, IGamePlugin plugin, string[] errors)
            : this(fileName, fileType, plugin, errors, false)
        {
        }
    }
}
