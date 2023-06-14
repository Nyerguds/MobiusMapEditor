using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Interface
{
    public interface ITilesetManager
    {
        void Reset(TheaterType theater);
        bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined);
        bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile);
        bool GetTileData(string name, int shape, out Tile tile, bool generateFallback, bool onlyIfDefined);
        bool GetTileData(string name, int shape, out Tile tile);
        int GetTileDataLength(string name);
    }
}
