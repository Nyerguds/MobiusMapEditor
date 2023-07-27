﻿//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
namespace MobiusEditor.RedAlert
{
	/// <summary>
	/// Split off into its own thing to avoid creating clutter in the InfantryTypes class file.
	/// Copied from CONST.CPP from the original Red Alert source code.
	/// https://github.com/electronicarts/CnC_Remastered_Collection/blob/master/REDALERT/CONST.CPP
	/// </summary>
	public static class InfantryClassicRemap
	{
		public static byte[] RemapCiv2 =
			{
			000,001,002,003,004,005,006,209,008,009,010,011,012,013,012,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,187,188,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,209,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,167,013,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};
		public static byte[] RemapCiv4 = {
			000,001,002,003,004,005,006,187,008,009,010,011,012,013,014,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,118,110,119,	// 096..111
			112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,188,207,	// 192..207
			208,209,182,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};
		public static byte[] RemapCiv5 = {
			000,001,002,003,004,005,006,109,008,009,010,011,131,013,014,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,177,110,178,	// 096..111
			112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,111,201,202,203,204,205,111,207,	// 192..207
			208,209,182,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};
		public static byte[] RemapCiv6 = {
			000,001,002,003,004,005,006,120,008,009,010,011,012,013,238,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,236,206,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,111,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};
		public static byte[] RemapCiv7 = {
			000,001,002,003,004,005,006,007,008,009,010,011,012,013,131,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,157,212,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,007,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,118,119,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};
		public static byte[] RemapCiv8 = {
			000,001,002,003,004,005,006,182,008,009,010,011,012,013,131,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,215,007,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,182,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,198,199,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,111,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};

		/// <summary>
		/// 014 => 007,
		/// 118 => 163,
		/// 119 => 165
		/// 159 => 200,
		/// 187 => 111,
		/// 188 => 013
		/// </summary>
		public static byte[] RemapCiv9 = {
			000,001,002,003,004,005,006,007,008,009,010,011,012,013,007,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,163,165,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,200,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,111,013,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};

		/// <summary>
		/// 007 => 137,
		/// 014 => 015,
		/// 118 => 129,
		/// 159 => 137,
		/// 187 => 163
		/// </summary>
		public static byte[] RemapCiv10 = {
			000,001,002,003,004,005,006,137,008,009,010,011,012,013,015,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,129,131,120,121,122,123,124,125,126,127,	// 112..127
			128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,137,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,163,165,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};

		/// <summary>
		/// Fix for the fact the DOS graphics of Einstein use the Mobius graphics.
		/// 014 => 120
		/// 130 => 138
		/// 131 => 119
		/// </summary>
		public static byte[] RemapEins = {
			000,001,002,003,004,005,006,007,008,009,010,011,012,013,120,015,	// 000..015
			016,017,018,019,020,021,022,023,024,025,026,027,028,029,030,031,	// 016..031
			032,033,034,035,036,037,038,039,040,041,042,043,044,045,046,047,	// 032..047
			048,049,050,051,052,053,054,055,056,057,058,059,060,061,062,063,	// 048..063
			064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,	// 064..079
			080,081,082,083,084,085,086,087,088,089,090,091,092,093,094,095,	// 080..095
			096,097,098,099,100,101,102,103,104,105,106,107,108,109,110,111,	// 096..111
			112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,	// 112..127
			128,129,138,119,132,133,134,135,136,137,138,139,140,141,142,143,	// 128..143
			144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,	// 144..159
			160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,	// 160..175
			176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,	// 176..191
			192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,	// 192..207
			208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,	// 208..223
			224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,	// 224..239
			240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255 	// 240..255
		};

	}
}
