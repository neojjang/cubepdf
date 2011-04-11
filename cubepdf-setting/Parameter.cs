﻿/* ------------------------------------------------------------------------- */
/*
 *  Parameter.cs
 *
 *  Copyright (c) 2009 - 2011 CubeSoft Inc. All rights reserved.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see < http://www.gnu.org/licenses/ >.
 */
/* ------------------------------------------------------------------------- */
using System;

namespace CubePDF {
    /* --------------------------------------------------------------------- */
    ///
    ///  Parameter
    ///  
    ///  <summary>
    ///  各種パラメータの値を定義したクラス．パラメータは，CubePDF の
    ///  コンボボックスのインデックスと一致させている．コ
    ///  ンボボックスの
    ///  並びなどを変更してしまうと過去のバージョンと整合性が取れなくなる
    ///  恐れがあるので注意する必要がある．
    ///  </summary>
    ///
    /* --------------------------------------------------------------------- */
    public abstract class Parameter {
        /* ----------------------------------------------------------------- */
        /// FileTypes
        /* ----------------------------------------------------------------- */
        public enum FileTypes : int {
            PDF, PS, EPS, SVG, PNG, JPEG, BMP, TIFF,
        };

        /* ----------------------------------------------------------------- */
        /// PDFVersions
        /* ----------------------------------------------------------------- */
        public enum PDFVersions : int {
            Ver1_7, Ver1_6, Ver1_5, Ver1_4, Ver1_3, Ver1_2,
        };

        /* ----------------------------------------------------------------- */
        ///
        /// ExistedFiles
        /// 
        /// <summary>
        /// 出力ファイルが既に存在する場合の処理．上書き，先頭に結合，
        /// 末尾に結合の 3種類．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public enum ExistedFiles : int {
            Overwrite, MergeHead, MergeTail,
        };

        /* ----------------------------------------------------------------- */
        /// PostProcesses
        /* ----------------------------------------------------------------- */
        public enum PostProcesses : int {
            Open, None, UserProgram,
        };

        /* ----------------------------------------------------------------- */
        /// Resolutions
        /* ----------------------------------------------------------------- */
        public enum Resolutions : int {
            Resolution72, Resolution150, Resolution300, Resolution450, Resolution600,
        };

        /* ----------------------------------------------------------------- */
        /// DownSamplings
        /* ----------------------------------------------------------------- */
        public enum DownSamplings : int {
            None, Average, Bicubic, Subsample,
        };

        /* ----------------------------------------------------------------- */
        /// Extension
        /* ----------------------------------------------------------------- */
        public static string Extension(FileTypes index) {
            switch (index) {
            case FileTypes.PDF:  return ".pdf";
            case FileTypes.PS:   return ".ps";
            case FileTypes.EPS:  return ".eps";
            case FileTypes.PNG:  return ".png";
            case FileTypes.JPEG: return ".jpg";
            case FileTypes.BMP:  return ".bmp";
            case FileTypes.TIFF: return ".tiff";
            case FileTypes.SVG:  return ".svg";
            default: break;
            }
            return "";
        }

#if HAVE_GHOSTSCRIPT
        /* ----------------------------------------------------------------- */
        /// Device
        /* ----------------------------------------------------------------- */
        public static Ghostscript.Device Device(FileTypes index, bool grayscale) {
            switch (index) {
            case FileTypes.PDF:  return Ghostscript.Device.PDF;
            case FileTypes.PS:   return Ghostscript.Device.PS;
            case FileTypes.EPS:  return Ghostscript.Device.EPS;
            case FileTypes.PNG:  return grayscale ? Ghostscript.Device.PNG_Gray : Ghostscript.Device.PNG;
            case FileTypes.JPEG: return grayscale ? Ghostscript.Device.JPEG_Gray : Ghostscript.Device.JPEG;
            case FileTypes.BMP:  return grayscale ? Ghostscript.Device.BMP_Gray : Ghostscript.Device.BMP;
            case FileTypes.TIFF: return grayscale ? Ghostscript.Device.TIFF_Gray : Ghostscript.Device.TIFF;
            case FileTypes.SVG:  return Ghostscript.Device.SVG;
            default:
                break;
            }
            return Ghostscript.Device.PDF;
        }
#endif

        /* ----------------------------------------------------------------- */
        /// PDFVersionValue
        /* ----------------------------------------------------------------- */
        public static double PDFVersionValue(PDFVersions index) {
            switch (index) {
            case PDFVersions.Ver1_7: return 1.7;
            case PDFVersions.Ver1_6: return 1.6;
            case PDFVersions.Ver1_5: return 1.5;
            case PDFVersions.Ver1_4: return 1.4;
            case PDFVersions.Ver1_3: return 1.3;
            case PDFVersions.Ver1_2: return 1.2;
            }
            return 1.7;
        }

        /* ----------------------------------------------------------------- */
        /// ResolutionValue
        /* ----------------------------------------------------------------- */
        public static int ResolutionValue(Resolutions index) {
            switch (index) {
            case Resolutions.Resolution72:  return 72;
            case Resolutions.Resolution150: return 150;
            case Resolutions.Resolution300: return 300;
            case Resolutions.Resolution450: return 450;
            case Resolutions.Resolution600: return 600;
            default: break;
            }
            return 300;
        }
    }
}