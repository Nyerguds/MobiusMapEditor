using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MobiusEditor.Model
{

    [Flags]
    public enum FileTypeFlags
    {
        None          /**/ = 0,      // No flags set.
        InternalUse   /**/ = 1 << 0, // For internal/debug use only. Not normally enabled.
        HideFromList  /**/ = 1 << 1, // Duplicates added for autodetect purposes. Don't show in actual open/save dialogs.
        ExpandedType  /**/ = 1 << 2, // Extra types that can be enabled with specific program settings.
    }

    [DebuggerDisplay("{Description}")]
    public class FileTypeInfo
    {
        public FileType FileType { get; set; }
        public string Description { get; set; }
        public FileTypeFlags Flags { get; private set; }
        public bool InternalUse => Flags.HasFlag(FileTypeFlags.InternalUse);
        public bool HideFromList => Flags.HasFlag(FileTypeFlags.HideFromList);
        public bool ExpandedType => Flags.HasFlag(FileTypeFlags.ExpandedType);
        public string[] SaveExtensionSingle { get; set; }
        public FileType[] SaveExtsSingleTypes { get; set; }
        public string[] SaveExtensionMulti { get; set; }
        public FileType[] SaveExtsMultiTypes { get; set; }


        public FileTypeInfo(FileType fileType, string descriptionSave, string[] extensionSingle, string[] extensionMulti)
        : this(fileType, descriptionSave, FileTypeFlags.None, extensionSingle, extensionMulti, null, null)
        { }

        public FileTypeInfo(FileType fileType, string description, string[] extensionSingle, string[] extensionMulti,
            FileType[] extensionSingleTypes, FileType[] extensionMultiTypes)
        : this(fileType, description, FileTypeFlags.None, extensionSingle, extensionMulti, extensionSingleTypes, extensionMultiTypes)
        { }

        public FileTypeInfo(FileType fileType, string description, FileTypeFlags flags, string[] extensionSingle, string[] extensionMulti)
            : this(fileType, description, flags, extensionSingle, extensionMulti, null, null)
        { }

        public FileTypeInfo(FileType fileType, string description, FileTypeFlags flags, string[] extensionSingle, string[] extensionMulti,
            FileType[] extensionSingleTypes, FileType[] extensionMultiTypes)
        {
            FileType = fileType;
            Description = description;
            Flags = flags;
            SaveExtensionSingle = extensionSingle;
            SaveExtsSingleTypes = new FileType[extensionSingle.Length];
            for (int i = 0; i < SaveExtsSingleTypes.Length; ++i)
            {
                SaveExtsSingleTypes[i] = extensionSingleTypes != null && extensionSingleTypes.Length > i ? extensionSingleTypes[i] : FileType;
            }
            SaveExtensionMulti = extensionMulti;
            SaveExtsMultiTypes = new FileType[extensionMulti.Length];
            for (int i = 0; i < SaveExtsSingleTypes.Length; ++i)
            {
                SaveExtsSingleTypes[i] = extensionSingleTypes != null && extensionSingleTypes.Length > i ? extensionSingleTypes[i] : FileType;
            }
        }

        public string GetSaveFilters(bool forSinglePlay, List<FileType> types, out string[] extensions, out FileType[] typesForExtensions)
        {
            List<string> filters = new List<string>();
            string[] exten = forSinglePlay ? SaveExtensionSingle : SaveExtensionMulti;
            FileType[] fts = forSinglePlay ? SaveExtsSingleTypes : SaveExtsMultiTypes;

            List<string> allExtensionsFull = new List<string>();
            List<string> allExtensionsSmall = new List<string>();
            List<FileType> allTypesForExt = new List<FileType>();
            for (int i = 0; i < exten.Length; ++i)
            {
                string ext = exten[i];
                string fullExt = ext.StartsWith("*.") ? ext : ("*." + ext);
                allExtensionsFull.Add(fullExt);
                string smallExt = ext.StartsWith("*.") ? ext.Substring(2) : ext;
                allExtensionsSmall.Add(smallExt);
                allTypesForExt.Add(fts[i]);
            }
            extensions = allExtensionsSmall.ToArray();
            typesForExtensions = allTypesForExt.ToArray();
            string extFilter = String.Join(";", allExtensionsFull.ToArray());
            string filter = Description + " (" + extFilter + ")|" + extFilter;
            if (types != null)
            {
                types.Add(FileType);
            }
            return filter;
        }

        public static int GetPreferredIndex(FileTypeInfo[] infos, bool expandedTypes, bool forSinglePlay)
        {
            return 0;
        }
    }
}
