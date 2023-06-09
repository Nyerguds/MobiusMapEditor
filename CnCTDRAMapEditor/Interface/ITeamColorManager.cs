using MobiusEditor.Model;
using System.Drawing;

namespace MobiusEditor.Interface
{
    public interface ITeamColorManager
    {
        Color RemapBaseColor { get; }
        ITeamColor this[string key] { get; }

        /// <summary>Gets a general color representing this team color.</summary>
        /// <param name="key">Color key</param>
        /// <returns>The basic color for this team color.</returns>
        Color GetBaseColor(string key);

        void Load(string path);
        void Reset(GameType gameType, TheaterType theater);
    }
}