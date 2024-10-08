﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MobiusEditor.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MobiusEditor.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon GameIcon00 {
            get {
                object obj = ResourceManager.GetObject("GameIcon00", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ; This file defines all files that are automatically identified when opening a .mix archive.
        ///; The definitions can be split into separate files, per game.
        ///
        ///; Games list. Items in this list require consecutive numbers starting from 0.
        ///[Games]
        ///0=TiberianDawn
        ///1=RedAlert
        ///2=SoleSurvivor
        ///
        ///; Game definitions. Each section can define these things:
        ///; ContentInis:   Optional. Extra inis (comma-separated) to read to find the data for this game.
        ///;                All data will be looked up in these files firs [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string mixcontent {
            get {
                return ResourceManager.GetString("mixcontent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ; Filetype formats for Red Alert
        ///[TypesRa]
        ///0=MissionRa    ; All RA mission formats.
        ///1=AudioVarRa   ; Audio varying for units (v00/v01/r00/r01 etc).
        ///2=BuildableRa  ; RA buildable; generates a basic ????.shp and an ????icon.shp.
        ///3=BuildingRa   ; RA building, potentially theater sensitive, with build-up.
        ///4=IconRa       ; RA item which only has an icon but no basic shp file. This is used for superweapons and fake buildings.
        ///5=VortexRa     ; RA chrono vortex effect deformation tables.
        ///6=ResourceRa   ; RA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string mixcontent_ra1 {
            get {
                return ResourceManager.GetString("mixcontent_ra1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ; Filetype formats for Sole Survivor
        ///[TypesSole]
        ///0=MapSole       ; All Sole Survivor maps.
        ///1=EvaSole       ; EVA voices are (mostly) all the same but with a different prefix per voice set.
        ///2=BuildableTd   ; Override for TD&apos;s BuildableTd definition, with added ????icnh.shp
        ///3=InfantryTd    ; Override for TD&apos;s InfantryTd definition, with added ????icnh.shp
        ///
        ///[MapSole]
        ///0={0}s[00-99]ea.[(ini)(bin)]
        ///
        ///; EVA voices; c = Commando, e = EVA, m = Game show host, s = Sultry
        ///[EvaSole]
        ///0=c_{0}.aud
        ///0Info=(Comma [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string mixcontent_sole {
            get {
                return ResourceManager.GetString("mixcontent_sole", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ; Filetype formats for Tiberian Dawn
        ///[TypesTd]
        ///0=MissionTdBase ; All TD mission formats.
        ///1=MissionTdJCps ; Japanese briefing CPS images.
        ///2=MissionTdExp  ; Extra TD mission formats (v1.06)
        ///3=ThemeVar      ; Themes that have .var remix variants.
        ///4=AudioJuv      ; Audio files that have .juv juvenile variants.
        ///5=AudioVarTd    ; Audio varying for units (v00/v01 etc)
        ///6=BuildableTd   ; TD buildable object without special logic or extra files: vehicles, walls.
        ///7=BuildingTd    ; TD building, potentially the [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string mixcontent_td {
            get {
                return ResourceManager.GetString("mixcontent_td", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.IO.UnmanagedMemoryStream similar to System.IO.MemoryStream.
        /// </summary>
        internal static System.IO.UnmanagedMemoryStream Mmg1 {
            get {
                return ResourceManager.GetStream("Mmg1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Mobius {
            get {
                object obj = ResourceManager.GetObject("Mobius", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.IO.UnmanagedMemoryStream similar to System.IO.MemoryStream.
        /// </summary>
        internal static System.IO.UnmanagedMemoryStream Mthanks1 {
            get {
                return ResourceManager.GetStream("Mthanks1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.IO.UnmanagedMemoryStream similar to System.IO.MemoryStream.
        /// </summary>
        internal static System.IO.UnmanagedMemoryStream Mtiber1 {
            get {
                return ResourceManager.GetStream("Mtiber1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap UI_CustomMissionPreviewDefault {
            get {
                object obj = ResourceManager.GetObject("UI_CustomMissionPreviewDefault", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
